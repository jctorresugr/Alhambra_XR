using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class JSONMessage
{
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
}
