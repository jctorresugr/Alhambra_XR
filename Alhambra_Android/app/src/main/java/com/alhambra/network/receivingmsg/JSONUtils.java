package com.alhambra.network.receivingmsg;

import org.json.JSONArray;
import org.json.JSONException;

public class JSONUtils
{
    public static float[] jsonArrayToFloatArray(JSONArray arr) throws JSONException
    {
        float[] res = new float[arr.length()];
        for(int i = 0; i < arr.length(); i++)
            res[i] = (float)arr.getDouble(i);
        return res;
    }
}
