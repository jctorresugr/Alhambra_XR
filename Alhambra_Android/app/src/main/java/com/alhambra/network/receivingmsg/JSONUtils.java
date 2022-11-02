package com.alhambra.network.receivingmsg;

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
}
