package com.alhambra.experiment;

import com.alhambra.network.JSONUtils;

import java.io.File;
import java.io.IOException;
import java.io.PrintWriter;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;

public class ExperimentDataCollection {

    public static final ExperimentDataCollection instance = new ExperimentDataCollection();

    protected ArrayList<ExperimentRecord> historyRecord = new ArrayList<>();
    private File folder = null;
    protected long timeIndex = System.currentTimeMillis();
    protected int blockIndex =0;
    protected int threshold = 1024*1024*8; //> this bytes, save a file (avoid OOM)
    protected int currentCount = 0;

    private static SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy_MM_dd_HH_mm");

    public static class ExperimentBlockRecord{
        public long time;
        public int blockIndex;
        public ArrayList<ExperimentRecord> records;
    }

    public static class ExperimentRecord{
        public long time;
        public String tag;
        public String content;
    }

    protected String getFileName(){
        return "Exp"+dateFormat.format(new Date())+"_"+ blockIndex +".json";
    }

    protected ExperimentBlockRecord generateBlockRecord(){
        ExperimentBlockRecord r = new ExperimentBlockRecord();
        r.time = System.currentTimeMillis();
        r.blockIndex = currentCount;
        r.records = historyRecord;
        return r;
    }

    public void saveFile(){
        if(historyRecord.size()==0){
            return;
        }
        File file = new File(folder,getFileName());
        if(!file.exists()){
            blockIndex =0;
            timeIndex = System.currentTimeMillis();
        }
        while(file.exists()){
            blockIndex++;
            file = new File(folder,getFileName());
        }

        PrintWriter out = null;

        try {
            if(file.createNewFile()){
                out = new PrintWriter(file);
                out.println(JSONUtils.gson.toJson(generateBlockRecord()));
                historyRecord.clear();
            }
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            if(out!=null){
                out.close();
            }
        }
    }


    public void addRecord(String tag,String s){
        ExperimentRecord er = new ExperimentRecord();
        er.tag=tag;
        er.content=s;
        er.time = System.currentTimeMillis();
        historyRecord.add(er);
        currentCount+=s.length();
        if(currentCount>threshold){
            saveFile();
            currentCount=0;
        }
    }

    public static void add(String tag, String s){
        instance.addRecord(tag,s);
    }

    public void addRecord(String tag,Object... objects){
        addRecord(tag,JSONUtils.gson.toJson(objects));
    }
    public static void add(String tag,Object... objects){
        instance.addRecord(tag,objects);
    }
    public static void save(){
        instance.saveFile();
    }

    public File getFolder() {
        return folder;
    }

    public void setFolder(File folder) {
        this.folder = folder;
    }

    @Override
    protected void finalize() throws Throwable {
        super.finalize();
        saveFile();
    }
}
