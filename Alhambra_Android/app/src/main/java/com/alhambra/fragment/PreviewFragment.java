package com.alhambra.fragment;

import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import com.alhambra.Dataset;
import com.alhambra.R;
import com.sereno.Tree;
import com.sereno.view.TreeView;

import java.util.HashMap;

public class PreviewFragment extends AlhambraFragment
{
    /** The dataset associated with this application*/
    private Dataset m_dataset = null;

    /*--------------------------------------*/
    /*----The Widgets about the Preview-----*/
    /*--------------------------------------*/

    /** All the entries shown in the Previous tree object*/
    private HashMap<Integer, Tree<View>> m_datasetEntries = new HashMap<>();

    /** The current selected entry*/
    private Integer m_currentSelection = null;

    /** The current group of selection*/
    private int[] m_currentGroup = new int[0];

    /** The context associated with this fragment*/
    private Context m_ctx = null;

    /*--------------------------------------*/
    /*------The Pre-Registered Widgets------*/
    /*--------------------------------------*/

    /** The preview tree*/
    TreeView m_treeView     = null;

    /** The image of the selected entry*/
    ImageView m_mainImageView = null;

    /** The text of the selected entry*/
    TextView m_mainTextView = null;

    /** The button to show the previous entry*/
    ImageButton m_previousBtn = null;

    /** The button to show the next entry*/
    ImageButton m_nextBtn     = null;

    public PreviewFragment()
    {
        super();
    }

    public void setDataset(Dataset d)
    {
        m_dataset = d;

        //Cannot update the layout...
        if(m_ctx == null)
            return;

        //Clear preview entries
        m_treeView.getModel().clear();
        m_datasetEntries.clear();

        LayoutInflater inflater = LayoutInflater.from(m_ctx);

        //Populate the preview tree
        final Tree<View> treeModel = m_treeView.getModel();
        for(Integer i : m_dataset.getIDs())
        {
            //Inflate the layout
            View preview = inflater.inflate(R.layout.preview_key_entry, null);
            preview.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

            //Configure the text
            TextView textView = preview.findViewById(R.id.preview_key_entry_name);
            textView.setText(i.toString());

            //Configure the preview image
            ImageView imageView = preview.findViewById(R.id.preview_key_entry_drawable);
            imageView.setImageDrawable(m_dataset.getDataFromID(i).getImage());

            //Put that in the tree view and set all the interactive listeners
            Tree<View> idTree = new Tree<>(preview);
            preview.setOnClickListener(view -> setMainEntryID(i));
            treeModel.addChild(idTree, -1);
            m_datasetEntries.put(i, idTree);
        }
        setMainEntryID(0);
    }

    /** Init the layout of the application*/
    private void initLayout(View v)
    {
        m_treeView      = v.findViewById(R.id.previewLayout);
        m_mainImageView = v.findViewById(R.id.mainImageEntry);
        m_mainTextView  = v.findViewById(R.id.mainTextEntry);
        m_previousBtn   = v.findViewById(R.id.previousEntryButton);
        m_nextBtn       = v.findViewById(R.id.nextEntryButton);

        //Listeners
        m_previousBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                setMainEntryID(findPreviousID());
        });

        m_nextBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                setMainEntryID(findNextID());
        });

        setDataset(m_dataset);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceStates)
    {
        View v = inflater.inflate(R.layout.preview_fragment, container, false);
        m_ctx = getContext();
        initLayout(v);
        return v;
    }

    /** Set the main entry ID. This function will change what is rendered in the main view
     * with the data that has ID == i
     * @param i the ID to check*/
    public void setMainEntryID(Integer i)
    {
        if(m_dataset == null)
            return;

        //If the ID is not valid, nothing to do
        if(!m_dataset.isIDValid(i) || isEntryGreyed(i))
            return;

        //Remove the highlight around the previous preview selected
        if(m_currentSelection != null)
        {
            Tree<View> previous = m_datasetEntries.get(m_currentSelection);
            if(previous != null) //Should not happen
                previous.value.setBackgroundResource(isEntryGreyed(m_currentSelection) ? R.color.dark : 0);
        }

        //Select the new one
        Dataset.Data data = m_dataset.getDataFromID(i);
        m_currentSelection = i;
        m_mainImageView.setImageDrawable(data.getImage());
        m_mainTextView.setText(data.getText());

        //And highlight its entry
        Tree<View> current = m_datasetEntries.get(i);
        if(current != null) //Should not happen
            current.value.setBackgroundResource(R.drawable.round_rectangle_background);
    }

    /** Set the current selection group (with IDs) to browse in the interface
     * Usually, this selection group comes from a selection on the hololens that contains, for a single position, multiple data
     * @param selections the new selection group. If selections.length == 0, there is no more group*/
    public void setCurrentGroupSelection(int[] selections)
    {
        if(m_dataset == null)
            return;

        m_currentGroup = selections;

        //Reset the background to all preview entries
        if(m_currentGroup.length == 0)
        {
            for(Tree<View> view : m_datasetEntries.values())
                view.value.setBackgroundResource(0);
            setMainEntryID(m_currentSelection); //Redo the background
            return;
        }

        //Grey out all the data that are not part of group selection
        for(Integer dataID : m_dataset.getIDs())
        {
            Tree<View> view = m_datasetEntries.get(dataID);
            view.value.setBackgroundResource(isEntryGreyed(dataID) ? R.color.dark : 0);
        }

        //Set the selection to the first group
        setMainEntryID(m_currentGroup[0]);
    }

    /** Should the preview entry be greyed in the interface based on m_currentGroup?
     * @return true if yes (the entry is not in m_currentGroup and m_currentGroup.length > 0), false otherwise*/
    public boolean isEntryGreyed(int entry)
    {
        if(m_currentGroup.length == 0)
            return false;

        for(int i : m_currentGroup)
            if(i == entry)
                return false;
        return true;
    }

    /** Find the next ID to select based on m_currentSelection and m_currentGroup
     * @return -1 if there is nothing to select next, the next ID to select otherwise*/
    private int findNextID()
    {
        if(m_dataset == null)
            return -1;

        for(int i = 0; i < m_currentGroup.length-1; i++)
            if(m_currentGroup[i] == m_currentSelection)
                return (m_dataset.isIDValid(m_currentGroup[i+1]) ? m_currentGroup[i+1] : -1);


        return (m_dataset.isIDValid(m_currentSelection+1) ? m_currentSelection+1 : -1);
    }

    /** Find the previous ID to select based on m_currentSelection and m_currentGroup
     * @return -1 if there is nothing to select before, the previous ID to select otherwise*/
    private int findPreviousID()
    {
        if(m_dataset == null)
            return -1;

        for(int i = 1; i < m_currentGroup.length; i++)
            if(m_currentGroup[i] == m_currentSelection)
                return (m_dataset.isIDValid(m_currentGroup[i-1]) ? m_currentGroup[i-1] : -1);

        return (m_dataset.isIDValid(m_currentSelection-1) ? m_currentSelection-1 : -1);
    }
}
