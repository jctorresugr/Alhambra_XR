package com.alhambra;

import android.content.res.AssetManager;
import android.graphics.drawable.Drawable;

import com.sereno.CSVReader;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;
import java.util.Set;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

/** Class parsing the whole dataset for later uses*/
public class Dataset
{
    /** Basic listener to monitor changes in the dataset status*/
    public interface IDatasetListener
    {
        /** Function called when the main entry index of the dataset has been changed
         * @param dataset the dataset calling this function
         * @param index the new entry Index*/
        void onSetMainEntryIndex(Dataset dataset, int index);

        /** Function called when the active highlighting selection of the dataset has been changed
         * @param dataset the dataset calling this function
         * @param selections the new indexes to highlight. If selections.length == 0, then there is nothing to highlight*/
        void onSetSelection(Dataset dataset, int[] selections);
    }

    /** This class describes the chunk of data as saved in the database
     * Each chunk of data is readonly once it is created.*/
    public static class Data
    {
        /** The index of this chunk of data*/
        private int    m_index = 0;

        /** The ID of this chunk of data*/
        private int    m_id = 0;

        /** The layer ID to which this chunk of data belongs to*/
        private int    m_layer = 0;

        /** The color representing this chunk of data*/
        private int    m_color = 0;

        /** The text associated with this chunk of data*/
        private String m_text = "";

        /** The image associated with this chunk of data*/
        private Drawable m_drawable = null;

        /** Constructor
         * @param index the Index of this chunk of data
         * @param id  The ID of this chunk of data inside its layer
         * @param layer The layer ID to which this chunk of data belongs to
         * @param color  The color representing this chunk of data
         * @param text The text associated with this chunk of data
         * @param img The image drawable describing this data chunk*/
        public Data(int index, int layer, int id, int color, String text, Drawable img)
        {
            m_index    = index;
            m_layer    = layer;
            m_id       = id;
            m_color    = color;
            m_text     = text;
            m_drawable = img;
        }

        /** Get the index of this chunk of data*/
        public int getIndex() {return m_index;}

        /** Get the ID of this chunk of data*/
        public int getID() {return m_id;}

        /** Get the layer ID to which this chunk of data belongs to*/
        public int getLayer() {return m_layer;}

        /** Get the color representing this chunk of data*/
        public int getColor() {return m_color;}

        /** Get the text associated with this chunk of data*/
        public String getText() {return m_text;}

        /** Get the image drawable describing this data chunk*/
        public Drawable getImage() {return m_drawable;}
    }

    /** All the stored chunks of data
     * Key: ID
     * Value: Data*/
    private HashMap<Integer, Data> m_data = new HashMap<>();

    /** The HashMap of all available Layers and their underlying data. We prefer to pre-compute this list for fast access
     * Key: Layer ID
     * Value: List of Data chunk*/
    private HashMap<Integer, List<Data>> m_layers = new HashMap<>();

    /** The general default data*/
    private Data m_defaultData = null;

    /** The listeners to notify changes*/
    private ArrayList<IDatasetListener> m_listeners = new ArrayList<IDatasetListener>();

    /** The current main entry to consider*/
    private int m_currentEntryIndex = 0;

    /** The current selections to consider*/
    private int[] m_currentSelection = new int[0];

    /** Constructor. Read, from the asset manager, the dataset described by assetHeader
     * @param assetManager the asset manager to read text data directly stored in the "assets" directory
     * @param assetHeader  the main file describing the dataset*/
    public Dataset(AssetManager assetManager, String assetHeader) throws IOException, IllegalArgumentException
    {
        InputStream dataset = assetManager.open(assetHeader);

        List<String[]> csvData = CSVReader.read(dataset);
        dataset.close();

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

                //Get ID...
                int index = Integer.parseInt(row[0]);
                if(m_data.containsKey(index))
                    throw new IllegalArgumentException("The Index " + index + " is duplicated in the dataset");

                //...and read the text associated to it
                InputStream textStream = assetManager.open("text"+row[0]+".txt");
                ByteArrayOutputStream text = new ByteArrayOutputStream();
                byte[] buffer = new byte[1024];
                for (int length; (length = textStream.read(buffer)) != -1; ) {
                    text.write(buffer, 0, length);
                }
                textStream.close();
                //...and read the image associated to it
                InputStream imgStream = assetManager.open("img"+row[0]+".png");
                Drawable img = Drawable.createFromStream(imgStream, null);
                imgStream.close();

                //Check if this chunk is the default entry describing the whole dataset
                //If it is, then it is associated with no layer (layer == -1) and its color is set to transparency (color == 0x00000000)
                boolean isDefault = true;
                for(int j = 1; j < 4; j++)
                {
                    if(!row[j].equals("-"))
                    {
                        isDefault = false;
                        break;
                    }
                }

                //If default: No color and no layer and add the data chunk
                if(isDefault)
                {
                    if(m_defaultData != null)
                        throw new IllegalArgumentException("The dataset contains multiple default data entry");
                    m_defaultData = new Data(index, -1, -1, 0x00, text.toString("UTF-8"), img);
                    m_data.put(index, m_defaultData);
                }

                //Else, read color + layer and add the data chunk
                else
                {
                    //Get the IDs
                    int layer = Integer.parseInt(row[2]);
                    int id    = Integer.parseInt(row[3]);

                    //Get the color as RGBA following the format (r,g,b,a)
                    if(row[1].charAt(0) != '(' && row[1].charAt(row[1].length()-1) != ')')
                        throw new IllegalArgumentException("One color entry is invalid");
                    String colorStr    = row[1].substring(1, row[1].length()-1);
                    String[] colorArray = colorStr.split(",");
                    if(colorArray.length != 4)
                        throw new IllegalArgumentException("One color entry is invalid");
                    int colorRGBA = 0;
                    for(int j = 0; j < colorArray.length; j++)
                        colorRGBA += Math.max(0, Math.min(255, Integer.parseInt(colorArray[j]))) << (byte)(4*j);

                    //Save the content
                    Data data = new Data(index, layer, id, colorRGBA, text.toString("UTF-8"), img);
                    m_data.put(index, data);
                    if(m_layers.containsKey(layer))
                        m_layers.get(layer).add(data);
                    else
                    {
                        ArrayList<Data> arr = new ArrayList<>();
                        arr.add(data);
                        m_layers.put(layer, arr);
                    }
                }
            }
        }

        if(getIndexes().size() > 0)
            m_currentEntryIndex = getIndexes().iterator().next();
    }

    /** @brief Add a new listener
     * @param l the new listener*/
    public void addListener(IDatasetListener l)
    {
        m_listeners.add(l);
    }

    /** @brief Remove an old listener
     * @param l the listener to remove*/
    public void removeListener(IDatasetListener l)
    {
        m_listeners.remove(l);
    }

    /** Get the datachunk at ID==id
     * @param index the index to look for
     * @return the data that contains this, normally, unique ID. See isIndexValid before calling this function to check that the ID is a valid one*/
    public Data getDataFromIndex(int index)
    {
        return m_data.get(index);
    }

    /** Get all the data contained in layer == layer
     * @param layer the layer to look for
     * @return the list of of data this layer contains. An empty list is returned if the layer is not found (see also isLayerValid and getLayers)*/
    public List<Data> getDataAtLayer(int layer)
    {
        if(m_layers.containsKey(layer))
            return m_layers.get(layer);
        return new ArrayList<>();
    }

    /** Get the index of a dataset from its ID information.
     * @param layer the layer where the data chunk is in
     * @param id the ID of this data chunk INSIDE this layer
     * @return -1 if the data is not found, the index otherwise*/
    public int getIndexFromID(int layer, int id)
    {
        for(Data d : m_data.values())
            if(d.getLayer() == layer && d.getID() == id)
                return d.getIndex();
        return -1;
    }

    /** Is the Index "id" available in the dataset?
     * @param index the index to look for
     * @return true if yes, false otherwise*/
    public boolean isIndexValid(int index) {return m_data.containsKey(index);}

    /** Is the Layer "layer" registered in the dataset?
     * @param layer the layer to look for
     * @return true if yes, false otherwise*/
    public boolean isLayerValid(int layer) {return m_layers.containsKey(layer);}

    /** Get all the different datachunks' IDs
     * @return the list of IDs stored in this dataset*/
    public Set<Integer> getIndexes() {return m_data.keySet();}

    /** Get all the different layers stored in this dataset
     * @return the list of layert IDs*/
    public Set<Integer> getLayers() {return m_layers.keySet();}

    /** Set the current entry to consider
     * @param layer the layer ID of the new entry to consider.
     * @param id the data ID inside the layer of the new entry to consider
     * @return true if the pair of (layer, id) is valid, false otherwise*/
    public boolean setMainEntryID(int layer, int id)
    {
        for(Data d : m_data.values())
            if(d.getLayer() == layer && d.getID() == id)
                return setMainEntryIndex(d.getIndex());
        return false;
    }

    /** Set the current entry to consider
     * @param index the new entry Index to consider. If the Index is invalid, this function does nothing
     * @return true if the index is valid, false otherwise*/
    public boolean setMainEntryIndex(int index)
    {
        if(isIndexValid(index))
        {
            m_currentEntryIndex = index;
            for(IDatasetListener l : m_listeners)
                l.onSetMainEntryIndex(this, index);
            return true;
        }
        return false;
    }

    /** Get the current entry to consider
     * @return the current entry Index. Use getDataFromID() to get the actual data*/
    public int getMainEntryIndex()
    {
        return m_currentEntryIndex;
    }

    /** Set the current selection to highlight
     * @param selections the new entry Indexes to highlight. Its size can be 0 (hence, nothing particular is to highlight).
     *                   If one of the Index is invalid, this function does nothing
     * @return true if all the IDs are valid, false otherwise*/
    public boolean setCurrentSelection(int[] selections)
    {
        for(int id : selections)
            if(!isIndexValid(id))
                return false;

        m_currentSelection = selections;
        for(IDatasetListener l : m_listeners)
            l.onSetSelection(this, selections);
        return true;
    }

    /** Get the current selected entries
     * @return the selected entry IDs. Use getDataFromID() to get the actual data*/
    public int[] getCurrentSelection()
    {
        return m_currentSelection;
    }
}
