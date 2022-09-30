package com.alhambra.network.receivingmsg;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

import com.alhambra.Dataset;

/** The class representing the "selection" action received from the server*/
public class SelectionMessage
{
    public static class PairLayoutID
    {
        public int layout;
        public int id;
        public PairLayoutID(int _layout, int _id)
        {
            layout = layout;
            id     = _id;
        }
    }

    /** The IDs selected*/
    private PairLayoutID[] m_ids;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public SelectionMessage(@NonNull JSONObject data) throws JSONException
    {
        JSONArray idsArr = data.getJSONArray("ids");
        m_ids = new PairLayoutID[idsArr.length()];
        for(int i = 0; i < idsArr.length(); i++)
        {
            JSONObject pairArr = idsArr.getJSONObject(i);
            m_ids[i] = new PairLayoutID(pairArr.getInt("layout"), pairArr.getInt("layout"));
        }
    }

    /** Get the parsed IDs of the selection
     * @return an array of IDs*/
    public PairLayoutID[] getIDs() { return m_ids; }
}
