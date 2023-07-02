package com.alhambra.network.receivingmsg;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

/** The class representing the "selection" action received from the server*/
public class SelectionMessage
{
    /** Class containing ID information per data chunk*/
    public static class PairLayerID
    {
        /** The layer ID where the data is in*/
        public int layer;

        /** The ID of the data INSIDE this layer*/
        public int id;

        /** Constructor. Initialize the class with default values
         * @param _layer the layer ID where this data chunk belongs to
         * @param _id the ID of this data chunk INSIDE this layer*/
        public PairLayerID(int _layer, int _id)
        {
            layer = _layer;
            id    = _id;
        }
    }

    /** The IDs selected*/
    private final PairLayerID[] ids;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public SelectionMessage(@NonNull JSONObject data) throws JSONException
    {
        JSONArray idsArr = data.getJSONArray("ids");
        ids = new PairLayerID[idsArr.length()];
        for(int i = 0; i < idsArr.length(); i++)
        {
            JSONObject pairArr = idsArr.getJSONObject(i);
            ids[i] = new PairLayerID(pairArr.getInt("layer"), pairArr.getInt("id"));
        }
    }

    /** Get the parsed IDs of the selection
     * @return an array of IDs*/
    public PairLayerID[] getIDs() { return ids; }
}
