package com.alhambra;

import com.google.gson.JsonElement;


public interface IReceiveMessageListener {
    void OnReceiveMessage(JsonElement jsonElement);
}
