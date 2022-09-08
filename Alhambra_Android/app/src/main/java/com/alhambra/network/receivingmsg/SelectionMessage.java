package com.alhambra.network.receivingmsg;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

/** The class representing the "selection" action received from the server*/
public class SelectionMessage
{
    /** The IDs selected*/
    private int[] m_ids;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public SelectionMessage(@NonNull JSONObject data) throws JSONException
    {
        JSONArray idsArr = data.getJSONArray("ids");
        m_ids = new int[idsArr.length()];
        for(int i = 0; i < idsArr.length(); i++)
            m_ids[i] = idsArr.getInt(i);
    }

    /** Get the parsed IDs of the selection
     * @return an array of IDs*/
    public int[] getIDs() { return m_ids; }
}
