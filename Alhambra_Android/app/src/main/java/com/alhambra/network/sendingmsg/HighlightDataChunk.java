package com.alhambra.network.sendingmsg;

public class HighlightDataChunk
{
    /** Generate the message "highlight data chunk" meant for the HoloLens
     * @param layer the layer where the data chunk is in
     * @param id the ID of this data chunk INSIDE the layer*/
    public static String generateJSON(int layer, int id)
    {
        return "{" +
                SendingMessage.generateIncr(4) + "\"action\": \"highlight\",\n"+
                SendingMessage.generateIncr(4) + "\"data\": {\n" +
                SendingMessage.generateIncr(8) + "\"layer\":" + layer + ",\n" +
                SendingMessage.generateIncr(8) + "\"id\":"    + id + "\n" +
                SendingMessage.generateIncr(4) + "}\n}";
    }
}
