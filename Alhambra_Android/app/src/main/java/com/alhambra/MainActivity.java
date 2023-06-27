package com.alhambra;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.EditText;

import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.UserData;
import com.alhambra.interactions.DatasetSync;
import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.fragment.AlhambraFragment;
import com.alhambra.fragment.AnnotationFragment;
import com.alhambra.fragment.OverviewFragment;
import com.alhambra.fragment.PageViewer;
import com.alhambra.fragment.PreviewFragment;
import com.alhambra.fragment.ViewPagerAdapter;
import com.alhambra.interactions.IInteraction;
import com.alhambra.interactions.MinimapInteraction;
import com.alhambra.interactions.SearchInteraction;
import com.alhambra.interactions.SelectionInteraction;
import com.alhambra.network.JSONUtils;
import com.alhambra.network.PackedJSONMessages;
import com.alhambra.network.receivingmsg.AddAnnotationMessage;
import com.alhambra.network.receivingmsg.AnnotationMessage;
import com.alhambra.network.receivingmsg.SelectionMessage;
import com.alhambra.network.SocketManager;
import com.alhambra.network.sendingmsg.FinishAnnotation;
import com.alhambra.network.sendingmsg.HighlightDataChunk;
import com.alhambra.network.sendingmsg.OverviewMessage;
import com.alhambra.network.sendingmsg.StartAnnotation;
import com.alhambra.view.FloatMiniMapView;
import com.google.android.material.chip.Chip;
import com.google.android.material.tabs.TabLayout;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;

/** The Main Activity of this application. This is the first thing that is suppose to start*/
public class MainActivity
        extends AppCompatActivity
        implements
            AlhambraFragment.IFragmentListener,
            PreviewFragment.IPreviewFragmentListener,
            AnnotationFragment.IAnnotationFragmentListener,
            OverviewFragment.OverviewFragmentListener
{
    /** The TAG to use for logging information*/
    public static final String TAG = "Alhambra";

    /** The ID of the preview fragment tab*/
    public static final int PREVIEW_FRAGMENT_TAB = 0;

    /** The ID of the annotation fragment tab*/
    public static final int ANNOTATION_FRAGMENT_TAB = 1;

    /** The user-defined configuration of this application*/
    private Configuration m_config = null;

    /** The dataset*/
    private AnnotationDataset m_Annotation_dataset = null;

    /** The client socket manager*/
    private SocketManager m_socket = null;

    /** The view pager handling all our fragments*/
    private PageViewer      m_viewPager;

    /** The TabLayout handling all our fragments (preview and annotation)*/
    private TabLayout m_tabLayout;

    /** The preview tab*/
    private PreviewFragment m_previewFragment = null;

    /** The annotation tab*/
    private AnnotationFragment m_annotationFragment = null;

    /** The overview tab*/
    private OverviewFragment m_overviewFragment = null;

    private HashMap<String, IReceiveMessageListener> m_receiveMessageListener = new HashMap<>();

    private ArrayList<IInteraction> interactions = new ArrayList<>();

    public EditText filterEditor;

    public SelectionData selectionData = new SelectionData();

    public UserData userData = new UserData();

    private MinimapInteraction minimapInteraction;

    /*--------------------------------------*/
    /*-----Initialization of everything-----*/
    /*--------------------------------------*/

    public PreviewFragment getPreviewFragment(){
        return m_previewFragment;
    }

    private void addInteraction(IInteraction interaction){
        interaction.regNetworkIO(this);
        interactions.add(interaction);
    }

    private void initFunctions(){
        this.addInteraction(new DatasetSync());
        this.addInteraction(new SelectionInteraction());
        this.addInteraction(new SearchInteraction());
        minimapInteraction = new MinimapInteraction();
        minimapInteraction.selectionData=selectionData;
        minimapInteraction.viewPager=m_viewPager;
        this.addInteraction(minimapInteraction);
    }

    public void regReceiveMessageListener(String actionName, IReceiveMessageListener l){
        if(m_receiveMessageListener.containsKey(actionName)){
            Log.w(TAG,"Already exists message listener, replace it: "+actionName);
        }
        if(l!=null && actionName!=null) {
            m_receiveMessageListener.put(actionName,l);
        }

    }
    /** Read the configuration file "config.json"*/
    private void initConfiguration()
    {
        File configFile = new File(getExternalFilesDir(null), "config.json");
        if(!configFile.exists())
        {
            Log.i(TAG, "Creating a default config.json in " + getExternalFilesDir(null).getAbsolutePath()+"config.json");

            //Duplicate Assets/config.json to externalDir/config.json
            try {
                //Read the original file
                InputStream is = getAssets().open("config.json");
                ByteArrayOutputStream data = new ByteArrayOutputStream();
                byte[] buffer = new byte[1024];
                for (int length; (length = is.read(buffer)) != -1; ) {
                    data.write(buffer, 0, length);
                }
                is.close();

                //Write to destination
                if(!configFile.createNewFile())
                    throw new IOException("Cannot create the new file " + configFile.getAbsolutePath());
                FileOutputStream outputStream = new FileOutputStream(configFile);
                outputStream.write(data.toByteArray());
                outputStream.close();
            }
            catch(IOException e) {
                Log.e(TAG, "Could not duplicate the config.json file... Revert to hard-coded default configuration.");
                m_config = new Configuration();
                return;
            }
        }

        m_config = new Configuration(configFile);
    }

    /** Read and initialize all components linked to the dataset to be used*/
    private void initDataset()
    {
        //Read the whole dataset
        try {
            m_Annotation_dataset = new AnnotationDataset(getAssets(), "SpotList.txt");
        }
        catch(Exception e) {
            Log.e(TAG, e.toString());
            finishAndRemoveTask();
        }
    }

    /** Initialize the network communication*/
    private void initNetwork()
    {
        Log.i(TAG, "Trying to connect to " + m_config.getServerIP() + ":" + m_config.getServerPort());

        //Instantiate the client network interface
        m_socket = new SocketManager(m_config.getServerIP(), m_config.getServerPort());
        m_socket.addListener(new SocketManager.ISocketManagerListener() {
            @Override
            public void onDisconnection(SocketManager socket)
            {
                Log.w(TAG, "Disconnected");
                MainActivity.this.runOnUiThread(() -> m_Annotation_dataset.clearServerAnnotations());
            }

            @Override
            public void onRead(SocketManager socket, String jsonMsg)
            {
                if(jsonMsg.length()>1000) {
                    Log.i("Network",jsonMsg.substring(0,1000)+"> Truncated, Total "+jsonMsg.length());
                }else{
                    Log.i("Network",jsonMsg);
                }

                try
                {
                    final JSONObject reader = new JSONObject(jsonMsg);

                    //Determine the action to do
                    String action = reader.getString("action");
                    Log.i(TAG,"receive action <"+action+">");
                    //Selection case
                    if (action.equals(PackedJSONMessages.ACTION_NAME)) {
                        JsonElement jsonElement = JsonParser.parseString(jsonMsg);
                        JsonObject asJsonObject = jsonElement.getAsJsonObject();
                        PackedJSONMessages messages = JSONUtils.gson.fromJson(asJsonObject.get("data"),PackedJSONMessages.class);
                        for(String message:messages.actions) {
                            onRead(socket,message);
                        }
                    }
                    else if(action.equals("selection"))
                    {
                        final SelectionMessage selection = new SelectionMessage(reader.getJSONObject("data"));
                        MainActivity.this.runOnUiThread(() ->
                        {
                            //Get the relevant IDs (and discard the default ones)
                            ArrayList<Integer> indexes = new ArrayList<>();
                            for(SelectionMessage.PairLayerID id: selection.getIDs())
                            {
                                int index = m_Annotation_dataset.getIndexFromID(id.layer, id.id);
                                if(index != -1)
                                    indexes.add(index);
                            }
                            int[] indexArr = new int[indexes.size()];
                            for(int i = 0; i < indexes.size(); i++)
                                indexArr[i] = indexes.get(i);

                            //show shallowest layer first
                            int mainEntryIndex = -1;
                            if(indexArr.length > 0)
                                mainEntryIndex = indexArr[0];

                            //Set the current selection and jump to the preview fragment on the tablet to highlight that the action is made
                            m_Annotation_dataset.setCurrentSelection(indexArr);
                            if(mainEntryIndex != -1)
                                m_Annotation_dataset.setMainEntryIndex(mainEntryIndex);
                            m_viewPager.setCurrentItem(PREVIEW_FRAGMENT_TAB);
                        });
                    }

                    //Start an annotation
                    else if(action.equals("annotation"))
                    {
                        final AnnotationMessage annotation = new AnnotationMessage(reader.getJSONObject("data"));
                        MainActivity.this.runOnUiThread(() -> {
                            m_tabLayout.getTabAt(ANNOTATION_FRAGMENT_TAB).view.setVisibility(View.VISIBLE);
                            m_viewPager.setCurrentItem(ANNOTATION_FRAGMENT_TAB);
                            m_annotationFragment.startNewAnnotation(annotation.getWidth(), annotation.getHeight(), annotation.getBitmap(), annotation.getCameraPos(), annotation.getCameraRot());
                            m_viewPager.setPagingEnabled(true);
                        });
                    }

                    else if(action.equals("addAnnotation"))
                    {
                        final AddAnnotationMessage addAnnotation = new AddAnnotationMessage(reader.getJSONObject("data"));
                        MainActivity.this.runOnUiThread(() -> {
                            m_Annotation_dataset.addServerAnnotation(addAnnotation);
                        });
                    }
                    else if(m_receiveMessageListener.containsKey(action)) {
                        IReceiveMessageListener iReceiveMessageListener = m_receiveMessageListener.get(action);
                        if (iReceiveMessageListener != null) {
                            JsonElement jsonElement = JsonParser.parseString(jsonMsg);
                            JsonObject asJsonObject = jsonElement.getAsJsonObject();
                            MainActivity.this.runOnUiThread(() -> {
                                iReceiveMessageListener.OnReceiveMessage(MainActivity.this,asJsonObject.get("data"));
                                    });

                        }
                    }else{
                        Log.w(TAG,"Unknown socket information: "+jsonMsg);
                    }
                }
                catch(JSONException e)
                {
                    Log.e(TAG, "Issue with the JSON object received through the network: " + e.toString());
                    return;
                }
            }

            @Override
            public void onReconnection(SocketManager socket) {
                Log.i(TAG, "Reconnected");
            }
        });
    }

    FloatMiniMapView floatView;
    /** Initialize the layout and all its widgets.*/
    private void initLayout()
    {
        setContentView(R.layout.activity_main);

        filterEditor = (EditText)findViewById(R.id.filterTextEdit);

        m_viewPager = (PageViewer)findViewById(R.id.viewpager);
        ViewPagerAdapter adapter = new ViewPagerAdapter(getSupportFragmentManager());

        //Add "Datasets" tab
        m_previewFragment = new PreviewFragment();
        m_previewFragment.addListener((AlhambraFragment.IFragmentListener)this);
        m_previewFragment.addListener((PreviewFragment.IPreviewFragmentListener)this);
        adapter.addFragment(m_previewFragment, "Datasets");

        //Add "Datasets" tab
        m_annotationFragment = new AnnotationFragment();
        m_annotationFragment.addListener((AlhambraFragment.IFragmentListener)this);
        m_annotationFragment.addListener((AnnotationFragment.IAnnotationFragmentListener)this);
        m_annotationFragment.setDataset(m_Annotation_dataset);
        adapter.addFragment(m_annotationFragment, "Annotation");

        //Add Overview tab
        m_overviewFragment = new OverviewFragment();
        m_overviewFragment.addListener((OverviewFragment.OverviewFragmentListener) this);
        m_overviewFragment.setAnnotationDataset(m_Annotation_dataset);
        m_overviewFragment.setUserData(userData);
        adapter.addFragment(m_overviewFragment, "Overview");


        //Link the PageViewer with the adapter, and link the TabLayout with the PageViewer
        m_viewPager.setAdapter(adapter);
        m_tabLayout = (TabLayout)findViewById(R.id.tabs);
        m_tabLayout.setupWithViewPager(m_viewPager);

        //disableAnnotationTab();

        //Set the dataset on the UI thread for redoing all the widgets of the PreviewFragment
        this.runOnUiThread(() -> m_previewFragment.setDataset(m_Annotation_dataset,selectionData));


        floatView = new FloatMiniMapView(MainActivity.this);
        floatView.setData(m_Annotation_dataset,userData);
        floatView.pager=m_viewPager;
    }

    @Override
    protected void onStart() {
        super.onStart();
    }

    @Override
    public void onAttachedToWindow() {
        super.onAttachedToWindow();
        floatView.show();
    }

    /** Disable the annotation tab. This will lock the application in the preview tab (works because we only have two tabs here)*/
    private void disableAnnotationTab()
    {
        m_tabLayout.getTabAt(ANNOTATION_FRAGMENT_TAB).view.setVisibility(View.GONE);
        m_viewPager.setCurrentItem(PREVIEW_FRAGMENT_TAB);
        m_viewPager.setPagingEnabled(false);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        initConfiguration();
        initDataset();
        initLayout();
        initNetwork();
        initFunctions();
    }

    @Override
    public void onEnableSwiping(Fragment fragment)
    {
        m_viewPager.setPagingEnabled(true);
    }

    @Override
    public void onDisableSwiping(Fragment fragment)
    {
        m_viewPager.setPagingEnabled(false);
    }

    @Override
    public void onHighlightDataChunk(PreviewFragment fragment, AnnotationInfo annotationInfo)
    {
        m_socket.push(HighlightDataChunk.generateJSON(annotationInfo.getLayer(), annotationInfo.getID()));
    }

    @Override
    public void onClickChip(Chip chip, int annotationJoint) {

    }

    @Override
    public void askStartAnnotation(AnnotationFragment frag) {
        m_socket.push(StartAnnotation.generateJSON());
    }

    @Override
    public void onConfirmAnnotation(AnnotationFragment frag)
    {
        ArrayList<String> jointTokens = frag.getJointTokens();
        m_socket.push(FinishAnnotation.generateJSON(true,
                frag.getAnnotationCanvasData().getGeometries(),
                frag.getAnnotationCanvasData().getWidth(),
                frag.getAnnotationCanvasData().getHeight(),
                frag.getAnnotationDescription(),
                frag.getCameraPos(),
                frag.getCameraRot(),
                frag.getSelectedAnnotationJointIDs(),
                jointTokens));
        frag.clearAnnotation();
        //runOnUiThread(this::disableAnnotationTab);
    }

    public void sendServerData(String s) {
        m_socket.push(s);
    }

    public void sendServerAction(String actionName, Object data) {
        m_socket.push(JSONUtils.createActionJson(actionName, data));
    }

    @Override
    public void onCancelAnnotation(AnnotationFragment frag)
    {
        m_socket.push(FinishAnnotation.generateJSON(
                false,
                frag.getAnnotationCanvasData().getGeometries(),
                frag.getAnnotationCanvasData().getWidth(),
                frag.getAnnotationCanvasData().getHeight(),
                frag.getAnnotationDescription(),
                frag.getCameraPos(),
                frag.getCameraRot(),
                frag.getSelectedAnnotationJointIDs(),
                new ArrayList<>()
                ));
        frag.clearAnnotation();
        //runOnUiThread(this::disableAnnotationTab);
    }

    @Override
    public void showAllAnnotation(OverviewFragment frag) {
        m_socket.push(OverviewMessage.generateShowAllJSON());
    }

    @Override
    public void stopShowAllAnnotation(OverviewFragment frag) {
        m_socket.push(OverviewMessage.generateStopShowAllJSON());
    }

    @Override
    public void onOverViewUIInit(OverviewFragment frag) {
        minimapInteraction.setMapView(m_overviewFragment.getMapView());
    }

    public AnnotationDataset getAnnotationDataset() {
        return m_Annotation_dataset;
    }

}