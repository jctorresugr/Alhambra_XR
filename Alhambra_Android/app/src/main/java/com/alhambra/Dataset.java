package com.alhambra;

import android.content.res.AssetManager;

import com.sereno.CSVReader;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Set;

/** Class parsing the whole dataset for later uses*/
public class Dataset
{
    /** This class describes the chunk of data as saved in the database
     * Each chunk of data is readonly once it is created.*/
    public static class Data
    {
        /** The ID of this chunk of data*/
        private int    m_id = 0;

        /** The layout ID to which this chunk of data belongs to*/
        private int    m_layout = 0;

        /** The color representing this chunk of data*/
        private int    m_color = 0;

        /** The text associated with this chunk of data*/
        private String m_text = "";

        /** Constructor
         * @param id  The ID of this chunk of data
         * @param layout The layout ID to which this chunk of data belongs to
         * @param color  The color representing this chunk of data
         * @param text The text associated with this chunk of data*/
        public Data(int id, int layout, int color, String text)
        {
            m_id     = id;
            m_layout = layout;
            m_color  = color;
            m_text   = text;
        }

        /** Get the ID of this chunk of data*/
        public int getID() {return m_id;}

        /** Get the layout ID to which this chunk of data belongs to*/
        public int getLayout() {return m_layout;}

        /** Get the color representing this chunk of data*/
        public int getColor() {return m_color;}

        /** Get the text associated with this chunk of data*/
        public String getText() {return m_text;}
    }

    /** All the stored chunks of data
     * Key: ID
     * Value: Data*/
    HashMap<Integer, Data> m_data = new HashMap<>();

    /** The HashMap of all available Layout and their underlying data. We prefer to pre-compute this list for fast access
     * Key: Layout ID
     * Value: List of Data chunk*/
    HashMap<Integer, List<Data>> m_layouts = new HashMap<>();

    /** The general default data*/
    Data m_defaultData = null;

    /** Constructor. Read, from the asset manager, the dataset described by assetHeader
     * @param assetManager the asset manager to read text data directly stored in the "assets" directory
     * @param assetHeader  the main file describing the dataset*/
    public Dataset(AssetManager assetManager, String assetHeader) throws IOException, IllegalArgumentException
    {
        InputStream dataset = assetManager.open(assetHeader);

        List<String[]> csvData = CSVReader.read(dataset);
        if(csvData.size() == 0)
            return;

        //Check that we have 4 values per row FOR ALL ROWS
        for(int i = 0; i < csvData.size(); i++)
        {
            String[] row = csvData.get(i);

            //Check that we have 4 values per row FOR ALL ROWS
            if(row.length != 4)
                throw new IllegalArgumentException("The CSV file does not contain the correct number of entries per row");

            if(i != 0)
            {
                //Get ID and read the text associated to it
                int id         = Integer.parseInt(row[0]);
                if(m_data.containsKey(id))
                    throw new IllegalArgumentException("The ID " + id + " is duplicated in the dataset");
                InputStream textStream = assetManager.open("text"+row[0]+".txt");
                ByteArrayOutputStream text = new ByteArrayOutputStream();
                byte[] buffer = new byte[1024];
                for (int length; (length = textStream.read(buffer)) != -1; ) {
                    text.write(buffer, 0, length);
                }

                //Check if this chunk is the default entry describing the whole dataset
                //If it is, then it is associated with no layout (layout == -1) and its color is set to transparency (color == 0x00000000)
                boolean isDefault = true;
                for(int j = 1; j < 4; j++)
                {
                    if(!row[j].equals("-"))
                    {
                        isDefault = false;
                        break;
                    }
                }
                if(isDefault)
                {
                    if(m_defaultData != null)
                        throw new IllegalArgumentException("The dataset contains multiple default data entry");
                    m_defaultData = new Data(id, -1, 0x00, text.toString("UTF-8"));
                    m_data.put(id, m_defaultData);
                }
                else
                {
                    //Get the layout
                    int layout = Integer.parseInt(row[2]);

                    //Get the color as RGBA following the format (r,g,b,a)
                    if(row[1].charAt(0) != '(' && row[1].charAt(row[1].length()-1) != ')')
                        throw new IllegalArgumentException("One color entry is invalid");
                    String colorStr    = row[1].substring(1, row[1].length()-1);
                    String[] colorArray = colorStr.split(";");
                    if(colorArray.length != 4)
                        throw new IllegalArgumentException("One color entry is invalid");
                    int colorRGBA = 0;
                    for(int j = 0; j < colorArray.length; j++)
                        colorRGBA += Math.max(0, Math.min(255, Integer.parseInt(colorArray[j]))) << (byte)(4*j);

                    //Save the content
                    Data data = new Data(id, layout, colorRGBA, text.toString("UTF-8"));
                    m_data.put(id, data);
                    if(m_layouts.containsKey(layout))
                        m_layouts.get(layout).add(data);
                    else
                    {
                        ArrayList<Data> arr = new ArrayList<>();
                        arr.add(data);
                        m_layouts.put(layout, arr);
                    }
                }
            }
        }
    }

    /** Get the datachunk at ID==id
     * @param id the id to look for
     * @return the data that contains this, normally, unique ID. See isIDValid before calling this function to check that the ID is a valid one*/
    public Data getDataFromID(int id)
    {
        return m_data.get(id);
    }

    /** Get all the data contained in layout == layout
     * @param layout the layout to look for
     * @return the list of of data this layout contains. An empty list is returned if the layout is not found (see also isLayoutValid and getLayouts)*/
    public List<Data> getDataAtLayout(int layout)
    {
        if(m_layouts.containsKey(layout))
            return m_layouts.get(layout);
        return new ArrayList<>();
    }

    /** Is the ID "id" available in the dataset?
     * @param id the ID to look for
     * @return true if yes, false otherwise*/
    public boolean isIDValid(int id) {return m_data.containsKey(id);}

    /** Is the Layout "layout" registered in the dataset?
     * @param layout the layout to look for
     * @return true if yes, false otherwise*/
    public boolean isLayoutValid(int layout) {return m_layouts.containsKey(layout);}

    /** Get all the different datachunks' IDs
     * @return the list of IDs stored in this dataset*/
    public Set<Integer> getIDs() {return m_data.keySet();}

    /** Get all the different layouts stored in this dataset
     * @return the list of layout IDs*/
    public Set<Integer> getLayouts() {return m_layouts.keySet();}
}
