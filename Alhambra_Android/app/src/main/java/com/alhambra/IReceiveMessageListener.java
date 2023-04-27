package com.alhambra;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;


public interface IReceiveMessageListener {
    void OnReceiveMessage(MainActivity mainActivity, JsonElement jsonElement);
}
