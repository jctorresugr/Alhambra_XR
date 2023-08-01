package com.alhambra.experiment;

import com.alhambra.network.NetworkJsonParser;
import com.google.gson.JsonElement;

import java.util.HashMap;

public class ExperimentNetworkCollector implements NetworkJsonParser.OnNetworkJsonParseListener {

    protected HashMap<String,Float> filterFrequency = new HashMap<>();
    protected HashMap<String,Float> frequencyCount = new HashMap<>();

    public void addLimit(String tag){
        filterFrequency.put(tag,10.0f);
    }

    /**
     *
     * @param tag
     * @param freqLimit Every `freqLimit` times, drop rate. 1.0 means 100%
     */
    public void addLimit(String tag, float freqLimit){
        filterFrequency.put(tag,freqLimit);
        frequencyCount.put(tag,0.0f);
    }

    @Override
    public void beforeProcess(String tag, JsonElement data, String jsonMsg) {
        Float freqLimit = filterFrequency.get(tag);
        if(freqLimit!=null){
            if(freqLimit>=1.0f){
                return;
            }
            boolean dropFlag = false;
            Float freq = frequencyCount.get(tag);
            freq+=freqLimit;
            if(freq>=1.0f){
                freq-=1.0f;
                dropFlag = true;
            }
            frequencyCount.put(tag,freq);
            if(dropFlag){
                return;
            }
        }
        ExperimentDataCollection.add("net_rec_"+tag,jsonMsg);
    }

    @Override
    public void failedProcess(String tag, String jsonMsg) {

    }
}
