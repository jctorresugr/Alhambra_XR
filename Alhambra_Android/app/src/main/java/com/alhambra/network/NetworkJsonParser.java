package com.alhambra.network;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.alhambra.IReceiveMessageListener;
import com.alhambra.ListenerSubscriber;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import java.util.HashMap;
import java.util.PriorityQueue;

public class NetworkJsonParser implements SocketManager.ISocketManagerListener {
    private static final String LOG_TAG = "NetworkJsonParser";
    private HashMap<String, IReceiveMessageListener> m_receiveMessageListener = new HashMap<>();
    private SocketManager m_socket = null;
    private Handler mainThreadHandler;

    //TODO: add a basic filter in the experiment data collection
    public interface OnNetworkJsonParseListener {
        void beforeProcess(String tag, JsonElement data, String jsonMsg);
        void failedProcess(String tag, String jsonMsg);
    }

    public ListenerSubscriber<OnNetworkJsonParseListener> listeners = new ListenerSubscriber<>();

    public void init(SocketManager socket){
        mainThreadHandler = new Handler(Looper.getMainLooper());
        m_receiveMessageListener = new HashMap<>();
        this.m_socket = socket;
        this.m_socket.addListener(this);
        seqRecord.clear();
    }

    public void regReceiveMessageListener(String actionName, IReceiveMessageListener l){
        if(m_receiveMessageListener.containsKey(actionName)){
            Log.w(LOG_TAG,"Already exists message listener, replace it: "+actionName);
        }
        if(l!=null && actionName!=null) {
            m_receiveMessageListener.put(actionName,l);
        }
    }

    @Override
    public void onDisconnection(SocketManager socket) {

    }

    @Override
    public void onRead(SocketManager socket, String jsonMsg) {
        if(jsonMsg.length()>500) {
            Log.i("Network",jsonMsg.substring(0,500)+"> Truncated, Total "+jsonMsg.length());
        }else{
            Log.i("Network",jsonMsg);
        }
        //Determine the action to do
        String action;
        JsonElement jsonElement = JsonParser.parseString(jsonMsg);
        JsonObject jsonObject = jsonElement.getAsJsonObject();

        action = jsonObject.get("action").getAsString();
        JsonElement data = jsonObject.get("data");
        Log.i(LOG_TAG,"receive action <"+action+">");
        if (action.equals(PackedJSONMessages.ACTION_NAME)) {
            PackedJSONMessages messages = JSONUtils.gson.fromJson(data,PackedJSONMessages.class);
            for(String message:messages.actions) {
                onRead(socket,message);
            }
        }if (action.equals(SeqJSONMessage.ACTION_NAME)) {
            SeqJSONMessage seqJSONMessage = JSONUtils.gson.fromJson(data,SeqJSONMessage.class);
            processSeqMessage(socket, seqJSONMessage);
        } else if(m_receiveMessageListener.containsKey(action)) {
            listeners.invoke(l->l.beforeProcess(action,data,jsonMsg));
            IReceiveMessageListener iReceiveMessageListener = m_receiveMessageListener.get(action);
            if (iReceiveMessageListener != null) {
                mainThreadHandler.post(
                        ()->
                                iReceiveMessageListener.OnReceiveMessage(
                                        data)
                );

            }
        }else{
            Log.w(LOG_TAG,"Unknown socket information: "+jsonMsg);
            listeners.invoke(l->l.failedProcess(action,jsonMsg));
        }

    }

    @Override
    public void onReconnection(SocketManager socket) {

    }

    private final HashMap<Integer, SeqInfo> seqRecord = new HashMap<>();

    private static class SeqInfo{
        public int curActionSeq=-1;
        //public int packNum =-1;
        //public int flag = -1;
        public PriorityQueue<SeqJSONMessage> waitingList = new PriorityQueue<>();
    }

    private void processSeqMessage(SocketManager socket, SeqJSONMessage msg){
        int packNum = msg.packNum;
        //int flag = msg.flag;
        //int actionSeq = msg.actionSeq;
        SeqInfo curInfo = seqRecord.get(packNum);
        if(curInfo==null){
            curInfo = new SeqInfo();
            seqRecord.put(packNum,curInfo);
        }
        curInfo.waitingList.add(msg);
        //process waitinglist
        while(curInfo.waitingList.size()>0){
            SeqJSONMessage curMeg = curInfo.waitingList.peek();
            if(curMeg.actionSeq== curInfo.curActionSeq+1){
                this.onRead(socket,msg.data);
                curInfo.curActionSeq++;
                curInfo.waitingList.poll();
                if(curMeg.flag==SeqJSONMessage.END){
                    seqRecord.remove(packNum);
                }
            }else{
                break;
            }
        }
    }

}
