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

import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.R;
import com.sereno.Tree;
import com.sereno.view.TreeView;

import java.util.ArrayList;
import java.util.HashMap;

public class PreviewFragment extends AlhambraFragment implements AnnotationDataset.IDatasetListener
{
    /** Interface containing events fired from the PreviewFragment*/
    public interface IPreviewFragmentListener
    {
        /** Highlight a particular data chunk
         * @param fragment the fragment calling this event
         * @param annotationInfo the data chunk to highlight*/
        void onHighlightDataChunk(PreviewFragment fragment, AnnotationInfo annotationInfo);
    }

    /** The dataset associated with this application*/
    private AnnotationDataset m_Annotation_dataset = null;

    /** All the entries shown in the Previous tree object*/
    private HashMap<Integer, Tree<View>> m_datasetEntries = new HashMap<>();

    /** The listeners to fire events to*/
    private ArrayList<IPreviewFragmentListener> m_listeners = new ArrayList<>();

    /** The context associated with this fragment*/
    private Context m_ctx = null;

    /*--------------------------------------*/
    /*------The Pre-Registered Widgets------*/
    /*--------------------------------------*/

    /** The current highlighted selection*/
    private Integer m_currentSelection = null;

    /** The preview tree*/
    private TreeView m_treeView     = null;

    /** The button user uses to highlight on the hololens a particular data chunk*/
    private Button m_highlightBtn   = null;

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

    /** Add a new listener
     * @param l the new listener*/
    public void addListener(IPreviewFragmentListener l)
    {
        m_listeners.add(l);
    }

    /** emove an old listener
     * @param l the listener to remove*/
    public void removeListener(IPreviewFragmentListener l)
    {
        m_listeners.remove(l);
    }

    /** Set the dataset to manipulate and render
     * @param d the new dataset*/
    public void setDataset(AnnotationDataset d)
    {
        //Set the dataset
        if(m_Annotation_dataset != null)
            m_Annotation_dataset.removeListener(this);
        m_Annotation_dataset = d;
        m_Annotation_dataset.addListener(this);

        //Cannot update the layout...
        if(m_ctx == null)
            return;

        //Clear preview entries
        m_treeView.getModel().clear();
        m_datasetEntries.clear();

        //Populate the preview tree
        for(Integer i : m_Annotation_dataset.getIndexes())
            addDataChunk(i);

        //Setup the view based on the model information
        onSetMainEntryIndex(d, d.getMainEntryIndex());
        onSetSelection(d, d.getCurrentSelection());
    }

    /** Add the UI components of a data chunk
     * @param idx the data chunk index in the linked dataset*/
    private void addDataChunk(int idx)
    {
        LayoutInflater inflater = LayoutInflater.from(m_ctx);
        final Tree<View> treeModel = m_treeView.getModel();

        //Inflate the layout
        View preview = inflater.inflate(R.layout.preview_key_entry, null);
        preview.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        //Configure the text
        TextView textView = preview.findViewById(R.id.preview_key_entry_name);
        textView.setText(Integer.toString(idx));

        //Configure the preview image
        ImageView imageView = preview.findViewById(R.id.preview_key_entry_drawable);
        imageView.setImageDrawable(m_Annotation_dataset.getDataFromIndex(idx).getImage());

        //Put that in the tree view and set all the interactive listeners
        Tree<View> idTree = new Tree<>(preview);
        preview.setOnClickListener(view -> m_Annotation_dataset.setMainEntryIndex(idx));
        treeModel.addChild(idTree, -1);
        m_datasetEntries.put(idx, idTree);
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
        m_quitSelectionBtn = v.findViewById(R.id.quitSelectionButton);
        m_highlightBtn     = v.findViewById(R.id.highlightButton);

        //Initialize Listeners
        m_previousBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_Annotation_dataset.setMainEntryIndex(findPreviousID());
        });

        m_nextBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_Annotation_dataset.setMainEntryIndex(findNextID());
        });

        m_quitSelectionBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                m_Annotation_dataset.setCurrentSelection(new int[0]);
        });

        m_highlightBtn.setOnClickListener(view -> {
            if(m_Annotation_dataset.getMainEntryIndex() != -1)
                for(IPreviewFragmentListener l : m_listeners)
                    l.onHighlightDataChunk(this, m_Annotation_dataset.getDataFromIndex(m_Annotation_dataset.getMainEntryIndex()));
        });

        //Reinit the dataset if needed
        setDataset(m_Annotation_dataset);
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
        if(m_Annotation_dataset.getCurrentSelection().length == 0)
            return false;

        for(int i : m_Annotation_dataset.getCurrentSelection())
            if(i == entry)
                return false;
        return true;
    }

    /** Find the next ID to select based on m_currentSelection and m_currentGroup
     * @return -1 if there is nothing to select next, the next ID to select otherwise*/
    private int findNextID()
    {
        if(m_Annotation_dataset == null)
            return -1;

        int[] currentGroup = m_Annotation_dataset.getCurrentSelection();
        for(int i = 0; i < currentGroup.length-1; i++)
            if(currentGroup[i] == m_currentSelection)
                return (m_Annotation_dataset.isIndexValid(currentGroup[i+1]) ? currentGroup[i+1] : -1);

        if(currentGroup.length > 0) return -1;
            //return (m_dataset.isIndexValid(currentGroup[0]) ? currentGroup[0] : -1);
        return (m_Annotation_dataset.isIndexValid(m_currentSelection+1) ? m_currentSelection+1 : -1);
    }

    /** Find the previous ID to select based on m_currentSelection and m_currentGroup
     * @return -1 if there is nothing to select before, the previous ID to select otherwise*/
    private int findPreviousID()
    {
        if(m_Annotation_dataset == null)
            return -1;

        int[] currentGroup = m_Annotation_dataset.getCurrentSelection();
        for(int i = 1; i < currentGroup.length; i++)
            if(currentGroup[i] == m_currentSelection)
                return (m_Annotation_dataset.isIndexValid(currentGroup[i-1]) ? currentGroup[i-1] : -1);

        if(currentGroup.length > 0) return -1;
            //return (m_dataset.isIndexValid(currentGroup[currentGroup.length-1]) ? currentGroup[currentGroup.length-1] : -1);
        return (m_Annotation_dataset.isIndexValid(m_currentSelection-1) ? m_currentSelection-1 : -1);
    }

    @Override
    public void onSetMainEntryIndex(AnnotationDataset d, int i)
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
        AnnotationInfo annotationInfo = m_Annotation_dataset.getDataFromIndex(i);
        m_currentSelection = i;
        m_mainImageView.setImageDrawable(annotationInfo.getImage());
        m_mainTextView.setText(annotationInfo.getText());

        //And highlight its entry
        Tree<View> current = m_datasetEntries.get(i);
        if(current != null) //Should not happen
            current.value.setBackgroundResource(R.drawable.round_rectangle_background);
    }

    @Override
    public void onSetSelection(AnnotationDataset d, int[] selections)
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
            m_quitSelectionBtn.setVisibility(View.GONE); //Hide the quit selection, as there is no selection
            return;
        }
        m_quitSelectionBtn.setVisibility(View.VISIBLE); //Show the quit selection button

        //Grey out all the data that are not part of group selection
        for(Integer dataID : m_Annotation_dataset.getIndexes())
        {
            Tree<View> view = m_datasetEntries.get(dataID);
            view.value.setBackgroundResource(isEntryGreyed(dataID) ? R.color.dark : 0);
        }

        //Set the selection to the first group
        m_Annotation_dataset.setMainEntryIndex(selections[0]);
    }

    @Override
    public void onAddDataChunk(AnnotationDataset annotationDataset, AnnotationInfo annotationInfo)
    {
        addDataChunk(annotationInfo.getIndex());
    }

    @Override
    public void onRemoveDataChunk(AnnotationDataset annotationDataset, AnnotationInfo annotationInfo)
    {
        final Tree<View> treeModel = m_treeView.getModel();

        Tree<View> entry = m_datasetEntries.get(annotationInfo.getIndex());
        treeModel.removeChild(entry);
    }
}
