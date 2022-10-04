package com.alhambra.fragment;

import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import com.alhambra.Dataset;
import com.alhambra.R;
import com.sereno.Tree;
import com.sereno.view.TreeView;

import java.util.HashMap;

public class PreviewFragment extends AlhambraFragment implements Dataset.IDatasetListener
{
    /** The dataset associated with this application*/
    private Dataset m_dataset = null;

    /** All the entries shown in the Previous tree object*/
    private HashMap<Integer, Tree<View>> m_datasetEntries = new HashMap<>();

    /** The context associated with this fragment*/
    private Context m_ctx = null;

    /*--------------------------------------*/
    /*------The Pre-Registered Widgets------*/
    /*--------------------------------------*/

    /** The current highlighted selection*/
    private Integer m_currentSelection = null;

    /** The preview tree*/
    private TreeView m_treeView     = null;

    /** The image of the selected entry*/
    private ImageView m_mainImageView = null;

    /** The text of the selected entry*/
    private TextView m_mainTextView = null;

    /** The button to show the previous entry*/
    private ImageButton m_previousBtn = null;

    /** The button to show the next entry*/
    private ImageButton m_nextBtn     = null;

    /** The button to quit the selection */
    private Button m_quitSelectionBtn = null;

    /** Default constructor*/
    public PreviewFragment()
    {
        super();
    }

    /** Set the dataset to manipulate and render
     * @param d the new dataset*/
    public void setDataset(Dataset d)
    {
        //Set the dataset
        if(m_dataset != null)
            m_dataset.removeListener(this);
        m_dataset = d;
        m_dataset.addListener(this);

        //Cannot update the layout...
        if(m_ctx == null)
            return;

        //Clear preview entries
        m_treeView.getModel().clear();
        m_datasetEntries.clear();

        /*--------------------------------------*/
        /*Create as many preview as data chunks-*/
        /*--------------------------------------*/
        LayoutInflater inflater = LayoutInflater.from(m_ctx);

        //Populate the preview tree
        final Tree<View> treeModel = m_treeView.getModel();
        for(Integer i : m_dataset.getIndexes())
        {
            //Inflate the layout
            View preview = inflater.inflate(R.layout.preview_key_entry, null);
            preview.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

            //Configure the text
            TextView textView = preview.findViewById(R.id.preview_key_entry_name);
            textView.setText(i.toString());

            //Configure the preview image
            ImageView imageView = preview.findViewById(R.id.preview_key_entry_drawable);
            imageView.setImageDrawable(m_dataset.getDataFromIndex(i).getImage());

            //Put that in the tree view and set all the interactive listeners
            Tree<View> idTree = new Tree<>(preview);
            preview.setOnClickListener(view -> m_dataset.setMainEntryIndex(i));
            treeModel.addChild(idTree, -1);
            m_datasetEntries.put(i, idTree);
        }

        //Setup the view based on the model information
        onSetMainEntryIndex(d, d.getMainEntryIndex());
        onSetSelection(d, d.getCurrentSelection());
    }

    /** Init the layout of the application
     * @param v the inflated view of this Fragment*/
    private void initLayout(View v)
    {
        //Find all the widgets of this fragment
        m_treeView         = v.findViewById(R.id.previewLayout);
        m_mainImageView    = v.findViewById(R.id.mainImageEntry);
        m_mainTextView     = v.findViewById(R.id.mainTextEntry);
        m_previousBtn      = v.findViewById(R.id.previousEntryButton);
        m_nextBtn          = v.findViewById(R.id.nextEntryButton);
        m_quitSelectionBtn = v.findViewById(R.id.quitSelection);

        //Initialize Listeners
        m_previousBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_dataset.setMainEntryIndex(findPreviousID());
        });

        m_nextBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_dataset.setMainEntryIndex(findNextID());
        });

        m_quitSelectionBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_dataset.setCurrentSelection(new int[0]);
        });

        //Reinit the dataset if needed
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

    /** Should the preview entry be greyed in the interface based on m_currentGroup?
     * @return true if yes (the entry is not in m_currentGroup and m_currentGroup.length > 0), false otherwise*/
    public boolean isEntryGreyed(int entry)
    {
        if(m_dataset.getCurrentSelection().length == 0)
            return false;

        for(int i : m_dataset.getCurrentSelection())
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

        int[] currentGroup = m_dataset.getCurrentSelection();
        for(int i = 0; i < currentGroup.length-1; i++)
            if(currentGroup[i] == m_currentSelection)
                return (m_dataset.isIndexValid(currentGroup[i+1]) ? currentGroup[i+1] : -1);


        return (m_dataset.isIndexValid(m_currentSelection+1) ? m_currentSelection+1 : -1);
    }

    /** Find the previous ID to select based on m_currentSelection and m_currentGroup
     * @return -1 if there is nothing to select before, the previous ID to select otherwise*/
    private int findPreviousID()
    {
        if(m_dataset == null)
            return -1;

        int[] currentGroup = m_dataset.getCurrentSelection();
        for(int i = 1; i < currentGroup.length; i++)
            if(currentGroup[i] == m_currentSelection)
                return (m_dataset.isIndexValid(currentGroup[i-1]) ? currentGroup[i-1] : -1);

        return (m_dataset.isIndexValid(m_currentSelection-1) ? m_currentSelection-1 : -1);
    }

    @Override
    public void onSetMainEntryIndex(Dataset d, int i)
    {
        //Nothing to be done, UI-wise
        if(m_ctx == null)
            return;

        //If the ID is not valid, nothing to do
        if(isEntryGreyed(i))
            return;

        //Remove the highlight around the previous preview selected
        if(m_currentSelection != null)
        {
            Tree<View> previous = m_datasetEntries.get(m_currentSelection);
            if(previous != null) //Should not happen
                previous.value.setBackgroundResource(isEntryGreyed(m_currentSelection) ? R.color.dark : 0);
        }

        //Select the new one
        Dataset.Data data  = m_dataset.getDataFromIndex(i);
        m_currentSelection = i;
        m_mainImageView.setImageDrawable(data.getImage());
        m_mainTextView.setText(data.getText());

        //And highlight its entry
        Tree<View> current = m_datasetEntries.get(i);
        if(current != null) //Should not happen
            current.value.setBackgroundResource(R.drawable.round_rectangle_background);
    }

    @Override
    public void onSetSelection(Dataset d, int[] selections)
    {
        //Nothing to be done UI-wise
        if(m_ctx == null)
            return;

        //Reset the background to all preview entries
        if(selections.length == 0)
        {
            for(Tree<View> view : m_datasetEntries.values())
                view.value.setBackgroundResource(0);
            onSetMainEntryIndex(d, m_currentSelection); //Redo the background
            m_quitSelectionBtn.setVisibility(View.GONE);
            return;
        }
        m_quitSelectionBtn.setVisibility(View.VISIBLE);

        //Grey out all the data that are not part of group selection
        for(Integer dataID : m_dataset.getIndexes())
        {
            Tree<View> view = m_datasetEntries.get(dataID);
            view.value.setBackgroundResource(isEntryGreyed(dataID) ? R.color.dark : 0);
        }

        //Set the selection to the first group
        m_dataset.setMainEntryIndex(selections[0]);
    }
}
