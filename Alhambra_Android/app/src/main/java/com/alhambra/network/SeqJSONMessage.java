package com.alhambra.network;

public class SeqJSONMessage {
    public static final String ACTION_NAME = "SeqMessage";

    //action sequence, it will receive message by sequence
    public int actionSeq;

    //package nums
    public int packNum;

    //notify begin or end
    public int flag;

    public static final int BEGIN=0;
    public static final int PROCESSING=1;
    public static final int END=2;

}
