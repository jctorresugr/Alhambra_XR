package com.alhambra;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import com.alhambra.network.SocketManager;
import com.sereno.Tree;
import com.sereno.view.TreeView;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;

public class MainActivity extends AppCompatActivity
{

    /** The TAG to use for logging information*/
    public static String TAG = "Alhambra";

    /** The user-defined configuration of this application*/
    private Configuration m_config = null;

    /** The dataset*/
    private Dataset m_dataset = null;

    /** The client socket manager*/
    private SocketManager m_socket = null;

    /*--------------------------------------*/
    /*----The Widgets about the Preview-----*/
    /*--------------------------------------*/

    /** All the entries shown in the Previous tree object*/
    private HashMap<Integer, Tree<View>> m_datasetEntries = new HashMap<>();

    /** The current selected entry*/
    private Integer m_currentSelection = null;

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

    /*--------------------------------------*/
    /*-----Initialization of everything-----*/
    /*--------------------------------------*/

    /** Init the layout of the application*/
    private void initLayout()
    {
        setContentView(R.layout.activity_main);

        m_treeView      = findViewById(R.id.previewLayout);
        m_mainImageView = findViewById(R.id.mainImageEntry);
        m_mainTextView  = findViewById(R.id.mainTextEntry);
        m_previousBtn   = findViewById(R.id.previousEntryButton);
        m_nextBtn       = findViewById(R.id.nextEntryButton);

        //Listeners
        m_previousBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                setMainEntryID(m_currentSelection-1);
        });

        m_nextBtn.setOnClickListener(view -> {
            if(m_currentSelection != null)
                setMainEntryID(m_currentSelection+1);
        });
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
            m_dataset = new Dataset(getAssets(), "SpotList.txt");
        }
        catch(Exception e) {
            Log.e(TAG, e.toString());
            finishAndRemoveTask();
        }

        //Populate the preview tree
        final Tree<View> treeModel = m_treeView.getModel();
        for(Integer i : m_dataset.getIDs())
        {
            //Inflate the layout
            View preview = getLayoutInflater().inflate(R.layout.preview_key_entry, null);
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
            public void onRead(SocketManager socket, String jsonMsg) {
                socket.push("test".getBytes(StandardCharsets.UTF_8)); //This is to test the client socket. This is to be deleted later TODO
            }

            @Override
            public void onReconnection(SocketManager socket) {
                Log.i(TAG, "Reconnected");
            }
        });
    }

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        initLayout();
        initConfiguration();
        initDataset();
        initNetwork();
    }

    /** Set the main entry ID. This function will change what is rendered in the main view
     * with the data that has ID == i
     * @param i the ID to check*/
    private void setMainEntryID(Integer i)
    {
        //If the ID is not valid, nothing to do
        if(!m_dataset.isIDValid(i))
            return;

        //Remove the highlight around the previous preview selected
        if(m_currentSelection != null)
        {
            Tree<View> previous = m_datasetEntries.get(m_currentSelection);
            if(previous != null) //Should not happen
                previous.value.setBackgroundResource(0);
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
}