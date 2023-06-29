package com.alhambra.fragment;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.R;
import com.alhambra.dataset.data.AnnotationJoint;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.sereno.Tree;
import com.sereno.math.BBox;
import com.sereno.view.TreeView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Set;

public class PreviewFragment extends AlhambraFragment implements AnnotationDataset.IDatasetListener,SelectionData.ISelectionDataChange
{
    @Override
    public void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups) {
        updateChip(newIndex);
    }

    @Override
    public void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups) {
        updateChip(newIndex);
    }

    @Override
    public void onGroupClear() {
        for(Chip chip : m_chipMappings.values()) {
            chip.setChecked(false);
        }
    }

    @Override
    public void onKeywordChange(String text) {

    }

    private void updateChip(int index) {
        Chip chip = m_chipMappings.get(index);
        if(chip==null) {
            Log.i("PreviewFragment","Not found: joint ui "+index);
            return;
        }
        chip.setChecked(m_selection_data.containSelectedGroup(index));
    }

    /** Interface containing events fired from the PreviewFragment*/
    public interface IPreviewFragmentListener
    {
        /** Highlight a particular data chunk
         * @param fragment the fragment calling this event
         * @param annotationInfo the data chunk to highlight*/
        void onHighlightDataChunk(PreviewFragment fragment, AnnotationInfo annotationInfo);

        void onClickChip(Chip chip, int annotationJoint);
    }

    /** The dataset associated with this application*/
    private AnnotationDataset m_Annotation_dataset = null;

    private SelectionData m_selection_data = null;

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

    private ChipGroup m_chipGroup = null;
    //private HashMap<Chip, Integer> m_chipMappings = new HashMap<>();
    private HashMap<Integer,Chip> m_chipMappings = new HashMap<>();

    /** The button to show the previous entry*/
    private ImageButton m_previousBtn = null;

    /** The button to show the next entry*/
    private ImageButton m_nextBtn     = null;

    /** The button to quit the selection */
    private Button m_quitSelectionBtn = null;

    private final HashMap<AnnotationID, PreviewUI> annotationPreviewUIMapping = new HashMap<>();

    private static class PreviewUI{
        public View preview;
        public TextView textView;
        public ImageView imageView;
        //public ChipGroup chipGroup;

        public PreviewUI(View preview) {
            this.preview = preview;
            textView = preview.findViewById(R.id.preview_key_entry_name);
            imageView = preview.findViewById(R.id.preview_key_entry_drawable);
            //chipGroup = preview.findViewById(R.id.preview_chip_group);
        }
    }

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
    public void setDataset(AnnotationDataset d, SelectionData selectionData)
    {
        //Set the dataset
        if(m_Annotation_dataset != null)
            m_Annotation_dataset.removeListener(this);
        m_Annotation_dataset = d;
        m_selection_data = selectionData;
        m_selection_data.subscriber.addListener(this);
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
        if(m_ctx==null) {
            Log.e("PreviewFragment","Null context in add data chunk! This should not happen >"+idx);
            return;
        }
        AnnotationInfo dataInfo = m_Annotation_dataset.getDataFromIndex(idx);
        AnnotationID annotationID = dataInfo.getAnnotationID();
        // if exists, just update it, do not add a new one
        if(annotationPreviewUIMapping.containsKey(annotationID)){
            PreviewUI previewUI = annotationPreviewUIMapping.get(annotationID);
            previewUI.textView.setText(Integer.toString(idx));
            previewUI.imageView.setImageDrawable(dataInfo.getImage());
            return;
        }

        LayoutInflater inflater = LayoutInflater.from(m_ctx);
        final Tree<View> treeModel = m_treeView.getModel();

        //Inflate the layout
        View preview = inflater.inflate(R.layout.preview_key_entry, null);
        preview.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        PreviewUI previewUI = new PreviewUI(preview);


        //Configure the text
        previewUI.textView.setText(Integer.toString(idx));

        //Configure the preview image
        previewUI.imageView.setImageDrawable(dataInfo.getImage());

        annotationPreviewUIMapping.put(annotationID,previewUI);
        //Put that in the tree view and set all the interactive listeners
        Tree<View> idTree = new Tree<>(preview);
        preview.setOnClickListener(view -> m_Annotation_dataset.setMainEntryIndex(idx));
        treeModel.addChild(idTree, -1);
        m_datasetEntries.put(idx, idTree);

    }

    private Chip addChip(ChipGroup cg, String text){
        Chip chip = new Chip(cg.getContext());
        chip.setText(text);
        chip.setCheckable(true);
        cg.addView(chip);
        return chip;
    }

    /** Init the layout of the application
     * @param v the inflated view of this Fragment*/
    private void initLayout(View v)
    {
        m_datasetEntries.clear();
        annotationPreviewUIMapping.clear();
        //Find all the widgets of this fragment
        m_treeView         = v.findViewById(R.id.previewLayout);
        m_mainImageView    = v.findViewById(R.id.mainImageEntry);
        m_mainTextView     = v.findViewById(R.id.mainTextEntry);
        m_chipGroup        = v.findViewById(R.id.mainAnnotationChipGroup);
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
            if(m_currentSelection != null){
                m_Annotation_dataset.setCurrentSelection(new int[0]);
                m_selection_data.setKeyword("");
                m_selection_data.clearSelectedGroup();
            }

        });

        m_highlightBtn.setOnClickListener(view -> {
            if(m_Annotation_dataset.getMainEntryIndex() != -1)
                for(IPreviewFragmentListener l : m_listeners)
                    l.onHighlightDataChunk(this, m_Annotation_dataset.getDataFromIndex(m_Annotation_dataset.getMainEntryIndex()));
        });

        //Reinit the dataset if needed
        setDataset(m_Annotation_dataset,m_selection_data);
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
        m_currentSelection = i;
        updateCurrentAnnotation();

        //And highlight its entry
        Tree<View> current = m_datasetEntries.get(i);
        if(current != null) //Should not happen
            current.value.setBackgroundResource(R.drawable.round_rectangle_background);
    }

    public void updateCurrentAnnotation() {
        m_chipGroup.post(() -> {
            AnnotationInfo annotationInfo = m_Annotation_dataset.getDataFromIndex(m_currentSelection);
            if(annotationInfo==null){ // if you do not have any annotations, this will trigger.
                return;
            }
            m_mainImageView.setImageDrawable(annotationInfo.getImage());
            m_mainTextView.setText(annotationInfo.getText());
            m_chipGroup.removeAllViews();
            m_chipMappings.clear();
            Annotation annotation = m_Annotation_dataset.getAnnotation(annotationInfo.getAnnotationID());
            Set<AnnotationJoint> annotationJoints = annotation.getAnnotationJoints();
            for(AnnotationJoint aj:annotationJoints){
                Chip chip = addChip(m_chipGroup,aj.getName());
                chip.setChecked(m_selection_data.containSelectedGroup(aj.getId()));
                m_chipMappings.put(aj.getId(),chip);
                chip.setOnClickListener(v -> {
                    Chip chipClicked = (Chip) v;
                    for(IPreviewFragmentListener l : m_listeners)
                        l.onClickChip(chipClicked,aj.getId());
                });
            }
        });

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
            //for(Tree<View> view : m_datasetEntries.values())
            //    view.value.setBackgroundResource(0);
            for(Annotation annot: m_Annotation_dataset.getAnnotationList()) {
                int dataID = annot.info.getIndex();
                PreviewUI previewUI = annotationPreviewUIMapping.get(annot.id);
                Tree<View> view = m_datasetEntries.get(dataID);
                assert view != null;
                assert previewUI != null;
                view.value.setBackgroundResource(0);
                previewUI.imageView.setImageDrawable(annot.info.getImage());
                previewUI.textView.setText(String.valueOf(dataID));
            }
            onSetMainEntryIndex(d, m_currentSelection); //Redo the background
            m_quitSelectionBtn.setVisibility(View.GONE); //Hide the quit selection, as there is no selection
            return;
        }
        m_quitSelectionBtn.setVisibility(View.VISIBLE); //Show the quit selection button

        //Grey out all the data that are not part of group
        /*
        for(Integer dataID : m_Annotation_dataset.getIndexes())
        {
            Tree<View> view = m_datasetEntries.get(dataID);
            view.value.setBackgroundResource(isEntryGreyed(dataID) ? R.color.dark : 0);
            Annotation annot = m_Annotation_dataset.getAnnotation(dataID);
            PreviewUI previewUI = annotationPreviewUIMapping.get(annot.id);
            assert previewUI != null;
            previewUI.imageView.setImageDrawable(null);
        }*/

        for(Annotation annot: m_Annotation_dataset.getAnnotationList()) {
            int dataID = annot.info.getIndex();
            Tree<View> view = m_datasetEntries.get(dataID);
            PreviewUI previewUI = annotationPreviewUIMapping.get(annot.id);
            assert view != null;
            assert previewUI != null;
            if(m_Annotation_dataset.hasSelectionIndex(dataID)) {
                view.value.setBackgroundResource(0);
                previewUI.imageView.setImageDrawable(annot.info.getImage());
                previewUI.textView.setText(String.valueOf(dataID));
            }else{
                view.value.setBackgroundResource(isEntryGreyed(dataID) ? R.color.dark : 0);
                previewUI.imageView.setImageDrawable(null);
                previewUI.textView.setText("");
            }
        }

        //Set the selection to the first group
        m_Annotation_dataset.setMainEntryIndex(selections[0]);
    }

    @Override
    public void onAddDataChunk(AnnotationDataset annotationDataset, Annotation annotation)
    {
        addDataChunk(annotation.info.getIndex());
    }

    @Override
    public void onRemoveDataChunk(AnnotationDataset annotationDataset, Annotation annotation)
    {
        final Tree<View> treeModel = m_treeView.getModel();

        Tree<View> entry = m_datasetEntries.get(annotation.info.getIndex());
        treeModel.removeChild(entry);
    }

    @Override
    public void onAnnotationChange(Annotation annotation) {
        if(m_currentSelection==null) {
            return;
        }
        AnnotationInfo dataFromIndex = m_Annotation_dataset.getDataFromIndex(m_currentSelection);
        if(dataFromIndex.getAnnotationID().equals(annotation.info.getAnnotationID())){
            updateCurrentAnnotation();
        }
    }

    @Override
    public void onAddJoint(AnnotationJoint annotationJoint) {
    }

    @Override
    public void onRemoveJoint(AnnotationJoint annotationJoint) {

    }

    @Override
    public void onChangeJoint(AnnotationJoint annotationJoint) {

    }

    @Override
    public void onModelBoundsChange(AnnotationDataset annotationDataset, BBox bounds) {

    }
}
