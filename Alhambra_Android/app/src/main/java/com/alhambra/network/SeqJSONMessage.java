package com.alhambra.network;

import java.util.Comparator;

public class SeqJSONMessage implements Comparator<SeqJSONMessage>,Comparable<SeqJSONMessage> {
    public static final String ACTION_NAME = "SeqMessage";

    //action sequence, it will receive message by sequence
    public int actionSeq;

    //package nums
    public int packNum;

    //notify begin or end
    public int flag;

    public String data;
    public static final int BEGIN=0;
    public static final int PROCESSING=1;
    public static final int END=2;

    @Override
    public int compare(SeqJSONMessage o1, SeqJSONMessage o2) {
        return Integer.compare(o1.actionSeq, o2.actionSeq);
    }

    @Override
    public int compareTo(SeqJSONMessage o) {
        return Integer.compare(actionSeq,o.actionSeq);
    }
}
