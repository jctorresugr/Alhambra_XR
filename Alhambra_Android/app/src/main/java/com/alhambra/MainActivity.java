package com.alhambra;

import androidx.appcompat.app.AppCompatActivity;

import android.app.ActionBar;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
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


    TreeView m_treeView     = null;
    TextView m_mainTextView = null;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        m_treeView     = findViewById(R.id.previewLayout);
        m_mainTextView = findViewById(R.id.mainTextEntry);

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
            //Create the text view to show the ID
            TextView textView = new TextView(this);
            textView.setText(i.toString());
            textView.setLayoutParams(new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT));

            //Put that in the tree view and set all the interactive listeners
            Tree<View> idTree = new Tree<>(textView);
            textView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    setMainTextID(i);
                }
            });
            treeModel.addChild(idTree, -1);
            m_datasetEntries.put(i, idTree);
        }
        setMainTextID(0);

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

    private void setMainTextID(Integer i)
    {
        m_currentSelection = i;
        m_mainTextView.setText(m_dataset.getDataFromID(i).getText());
    }
}