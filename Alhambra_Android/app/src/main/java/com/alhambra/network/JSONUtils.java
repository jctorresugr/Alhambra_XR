package com.alhambra.network;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.sereno.json.BBoxJsonAdapter;
import com.sereno.math.BBox;

import org.json.JSONArray;
import org.json.JSONException;

/** Helper class for JSON objects */
public class JSONUtils
{
    /** Convert a JSON Array containing floating-point values to a Java float array
     * @param arr the JSON array to read data from
     * @return the converted JSON array to a float array. Length: arr.length()
     * @throws JSONException Error at parsing the JSONArray as a floating-point value array.*/
    public static float[] jsonArrayToFloatArray(JSONArray arr) throws JSONException
    {
        float[] res = new float[arr.length()];
        for(int i = 0; i < arr.length(); i++)
            res[i] = (float)arr.getDouble(i);
        return res;
    }

    /** Convert a JSON Array containing integer values that can be converted to bytes (i.e., each value is encoded within 8 bits) to a Java byte array
     * @param arr the JSON array to read data from
     * @return the converted JSON array to a byte array. Length: arr.length()
     * @throws JSONException Error at parsing the JSONArray as an array of integer values.*/
    public static byte[] jsonArrayToByteArray(JSONArray arr) throws JSONException
    {
        byte[] res = new byte[arr.length()];
        for(int i = 0; i < arr.length(); i++)
            res[i] = (byte)arr.getInt(i);
        return res;
    }

    public static final Gson gson;
    static {
        GsonBuilder gsonBuilder = new GsonBuilder();
        gsonBuilder.registerTypeAdapter(BBox.class,new BBoxJsonAdapter());
        gson = gsonBuilder.create();
    }

    public static String createActionJson(String actionName, Object data){
        return "{\"action\":\""+actionName+"\",\"data\":"+gson.toJson(data)+"}";
    }

    public static String createActionJson(String actionName){
        return "{\"action\":\""+actionName+"\",\"data\":{}}";
    }
}
