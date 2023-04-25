package com.sereno.json;

import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.sereno.math.BBox;
import com.sereno.math.Vector3;

import java.lang.reflect.Type;

public class BBoxJsonAdapter implements JsonDeserializer<BBox> {

    @Override
    public BBox deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
        JsonObject jsonObject = json.getAsJsonObject();
        BBox bbox = new BBox();
        Vector3 center =context.deserialize(jsonObject.get("m_Center"),Vector3.class);
        Vector3 extent =context.deserialize(jsonObject.get("m_Extent"),Vector3.class);
        bbox.min = new Vector3(center.x-extent.x,center.y-extent.y, center.z- extent.z);
        bbox.max = new Vector3(center.x+extent.x,center.y+extent.y, center.z+ extent.z);
        return bbox;
    }

}
