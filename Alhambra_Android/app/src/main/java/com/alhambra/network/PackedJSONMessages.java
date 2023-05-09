package com.alhambra.network;

import java.util.ArrayList;

public class PackedJSONMessages {
    public static final String ACTION_NAME="PackedMessage";
    public ArrayList<String> actions = new ArrayList<String>();

    public void addAction(String actionName, Object data){
        actions.add(JSONUtils.createActionJson(actionName,data));
    }

    public void addString(String json){
        actions.add(json);
    }
}
