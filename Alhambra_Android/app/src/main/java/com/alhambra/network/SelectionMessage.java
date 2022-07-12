package com.alhambra.network;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.List;

public class SelectionMessage
{
    private int[] m_ids;

    public SelectionMessage(JSONObject data) throws JSONException
    {
        JSONArray idsArr = data.getJSONArray("ids");
        m_ids = new int[idsArr.length()];
        for(int i = 0; i < idsArr.length(); i++)
            m_ids[i] = idsArr.getInt(i);
    }

    public int[] getIDs() { return m_ids; }
}
