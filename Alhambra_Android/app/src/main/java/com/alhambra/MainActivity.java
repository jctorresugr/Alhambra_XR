package com.alhambra;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import android.os.Bundle;
import android.util.Log;

import com.alhambra.fragment.AlhambraFragment;
import com.alhambra.fragment.AnnotationFragment;
import com.alhambra.fragment.PageViewer;
import com.alhambra.fragment.PreviewFragment;
import com.alhambra.fragment.ViewPagerAdapter;
import com.alhambra.network.SelectionMessage;
import com.alhambra.network.SocketManager;
import com.google.android.material.tabs.TabLayout;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;

public class MainActivity extends AppCompatActivity implements AlhambraFragment.IFragmentListener
{
    /** The TAG to use for logging information*/
    public static String TAG = "Alhambra";

    /** The user-defined configuration of this application*/
    private Configuration m_config = null;

    /** The dataset*/
    private Dataset m_dataset = null;

    /** The client socket manager*/
    private SocketManager m_socket = null;

    /** The view pager handling all our fragments*/
    private PageViewer      m_viewPager;

    /** The preview tab*/
    private PreviewFragment m_previewFragment = null;

    /** The annotation tab*/
    private AnnotationFragment m_annotationFragment = null;

    /*--------------------------------------*/
    /*-----Initialization of everything-----*/
    /*--------------------------------------*/

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
            m_dataset = new Dataset(getAssets(), "SpotList.txt");
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
            public void onDisconnection(SocketManager socket) {
                Log.w(TAG, "Disconnected");
            }

            @Override
            public void onRead(SocketManager socket, String jsonMsg)
            {
                try
                {
                    final JSONObject reader = new JSONObject(jsonMsg);

                    //Determine the action to do
                    String action = reader.getString("action");

                    //Selection case
                    if(action.equals("selection"))
                    {
                        final SelectionMessage selection = new SelectionMessage(reader.getJSONObject("data"));
                        MainActivity.this.runOnUiThread(() -> m_previewFragment.setCurrentGroupSelection(selection.getIDs()));
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

    /** Initialize the layout and all its widgets.*/
    private void initLayout()
    {
        setContentView(R.layout.activity_main);

        m_viewPager = (PageViewer)findViewById(R.id.viewpager);
        ViewPagerAdapter adapter = new ViewPagerAdapter(getSupportFragmentManager());

        //Add "Datasets" tab
        m_previewFragment = new PreviewFragment();
        m_previewFragment.addListener(this);
        adapter.addFragment(m_previewFragment, "Datasets");

        //Add "Datasets" tab
        m_annotationFragment = new AnnotationFragment();
        m_annotationFragment.addListener(this);
        adapter.addFragment(m_annotationFragment, "Annotation");

        //Link the PageViewer with the adapter, and link the TabLayout with the PageViewer
        m_viewPager.setAdapter(adapter);
        TabLayout tabLayout = (TabLayout)findViewById(R.id.tabs);
        tabLayout.setupWithViewPager(m_viewPager);

        //Set the dataset on the UI thread for redoing all the widgets of the PreviewFragment
        this.runOnUiThread(() -> {
            m_previewFragment.setDataset(m_dataset);
        });
    }

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        initConfiguration();
        initDataset();
        initLayout();
        initNetwork();
    }

    @Override
    public void onEnableSwipping(Fragment fragment)
    {
        m_viewPager.setPagingEnabled(true);
    }

    @Override
    public void onDisableSwipping(Fragment fragment)
    {
        m_viewPager.setPagingEnabled(false);
    }
}