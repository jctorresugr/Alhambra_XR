
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class JSONMessage
{
    public static String QuoteString(String s)
    {
        StringBuilder res = new StringBuilder();
        res.Append('"');
        for(int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\')
                res.Append("\\\\");
            else if (s[i] == '\n')
                res.Append("\\n");
            else if (s[i] == '\t')
                res.Append("\\t");
            else if (s[i] == '"')
                res.Append("\\\"");
            else if (s[i] == '/')
                res.Append("\\/");
            else
                res.Append(s[i]);
        }
        res.Append('"');
        return res.ToString();
    }

    /// <summary>
    /// Create the JSON message corresponding to a selection from a picked color from the index color
    /// </summary>
    /// <param name="c">The three layers color pixel describing up to four layers selection. Note that all 0 values are discarded and not sent</param>
    /// <returns>A selection message to be sent to the, e.g., tablet client</returns>
    public static String SelectionToJSON(Color c)
    {
        int cr = (int)(c.r * 255);
        int cg = (int)(c.g * 255);
        int cb = (int)(c.b * 255);
        
        String crS = (cr > 0 ? $"{{\"layer\": 0, \"id\": {cr.ToString()}}}{(cg > 0 || cb > 0 ? "," : "")}" : "");
        String cgS = (cg > 0 ? $"{{\"layer\": 1, \"id\": {cg.ToString()}}}{(cb > 0           ? "," : "")}" : "");
        String cbS = (cb > 0 ? $"{{\"layer\": 2, \"id\": {cb.ToString()}}}"                                : "");

        return
             "{" +
             "    \"action\": \"selection\",\n" +
             "    \"data\":\n" + 
             "    {\n" +
            $"        \"ids\": [{crS}{cgS}{cbS}]\n" +
             "    }\n" +
             "}";
    }

    /// <summary>
    /// Create the JSON message containing all the necessary information for the client to annotate a part of the 3D space
    /// </summary>
    /// <param name="pixels">The RGBA32 image of the background that the client will annotate</param>
    /// <param name="width">The width of "pixels"</param>
    /// <param name="height">The height of "pixels"</param>
    /// <param name="cameraPos">The camera position where the snapshot "pixels" was taken, useful to anchor back the annotation</param>
    /// <param name="cameraRot">The camera orientation where the snapshot "pixels" was taken, useful to anchor back the annotation</param>
    /// <returns>The "annotation" JSON message to be sent to, e.g., tablet client</returns>
    public static String StartAnnotationToJSON(byte[] pixels, int width, int height, Vector3 cameraPos, Quaternion cameraRot)
    {
        return 
             "{" +
             "    \"action\": \"annotation\",\n" +
             "    \"data\":\n" +
             "    {\n" +
            $"        \"base64\": \"{System.Convert.ToBase64String(pixels)}\",\n" +
            $"        \"width\": {width},\n"   +
            $"        \"height\": {height},\n" +
            $"        \"cameraPos\": [{cameraPos.x}, {cameraPos.y}, {cameraPos.z}],\n" +
            $"        \"cameraRot\": [{cameraRot.w}, {cameraRot.x}, {cameraRot.y}, {cameraRot.z}]\n" +
             "    }\n" +
             "}";
    }

    /// <summary>
    /// Create the JSON message containing all the necessary information for the client to store an annotation
    /// <param name="annot">The annotation data</param>
    /// <returns>The "addAnnotation" JSON message to be sent to, e.g., tablet client</returns>
    /// </summary>
    public static String AddAnnotationToJSON(Annotation annot)
    {
        return 
            "{" +
            "    \"action\": \"addAnnotation\",\n" +
            "    \"data\":\n" +
            "    {\n" +
           $"        \"snapshotBase64\": \"{System.Convert.ToBase64String(annot.info.SnapshotRGBA)}\",\n" +
           $"        \"snapshotWidth\":  {annot.info.SnapshotWidth},\n" +
           $"        \"snapshotHeight\": {annot.info.SnapshotHeight},\n" +
           $"        \"annotationColor\": [{annot.info.Color.r}, {annot.info.Color.g}, {annot.info.Color.b}, {annot.info.Color.a}],\n" +
           $"        \"description\": {QuoteString(annot.info.Description)}," +
           $"        \"renderInfo\": {JsonUtility.ToJson(annot.renderInfo)}" +
            "    }\n" +
            "}";
    }

    public static string ActionJSON<T>(string actionName, T data)
    {
        string dataString = JsonUtility.ToJson(data);
        return string.Format($"{{\"action\":\"{actionName}\",\"data\":{dataString}}}");
    }
}
