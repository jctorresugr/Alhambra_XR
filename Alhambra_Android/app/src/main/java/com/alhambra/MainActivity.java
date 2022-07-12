package com.alhambra;

import androidx.appcompat.app.AppCompatActivity;

import android.app.ActionBar;
import android.media.Image;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.alhambra.network.SocketManager;
import com.sereno.Tree;
import com.sereno.view.TreeView;

import java.nio.charset.StandardCharsets;
import java.util.HashMap;

public class MainActivity extends AppCompatActivity {

    /** The TAG to use for logging information*/
    public static String TAG = "Alhambra";

    /** The client socket manager*/
    private SocketManager m_socket = null;

    /** The dataset*/
    private Dataset m_dataset = null;

    /** All the entries shown in the Previous tree object*/
    private HashMap<Integer, Tree<View>> m_datasetEntries = new HashMap<>();

    /** The current selected entry*/
    private Integer m_currentSelection = null;

    /*--------------------------------------*/
    /*-------The Widgets of this View-------*/
    /*--------------------------------------*/

    /** The preview tree*/
    TreeView m_treeView     = null;
    /** The image of the selected entry*/
    ImageView m_mainImageView = null;
    /** The text of the selected entry*/
    TextView m_mainTextView = null;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        m_treeView      = findViewById(R.id.previewLayout);
        m_mainImageView = findViewById(R.id.mainImageEntry);
        m_mainTextView  = findViewById(R.id.mainTextEntry);

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
            preview.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    setMainEntryID(i);
                }
            });
            treeModel.addChild(idTree, -1);
            m_datasetEntries.put(i, idTree);
        }
        setMainEntryID(0);

        //Instantiate the client network interface
        m_socket = new SocketManager("192.168.2.132", 8080);
        m_socket.addListener(new SocketManager.ISocketManagerListener() {
            @Override
            public void onDisconnection(SocketManager socket) {
                Log.w(TAG, "Disconnected");
            }

            @Override
            public void onRead(SocketManager socket, String jsonMsg) {
                Log.i(TAG, "Message read: " + jsonMsg);
                Log.i(TAG, "Sending 'test'");
                socket.push("test".getBytes(StandardCharsets.UTF_8));
            }

            @Override
            public void onReconnection(SocketManager socket) {
                Log.i(TAG, "Reconnected");
            }
        });
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
            previous.value.setBackgroundResource(0);
        }

        //Select the new one
        Dataset.Data data = m_dataset.getDataFromID(i);
        m_currentSelection = i;
        m_mainImageView.setImageDrawable(data.getImage());
        m_mainTextView.setText(data.getText());

        //And highlight its entry
        Tree<View> current = m_datasetEntries.get(i);
        current.value.setBackgroundResource(R.drawable.round_rectangle_background);
    }
}