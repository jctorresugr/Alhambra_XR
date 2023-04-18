using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Data management, all annotation data stores here
/// </summary>
public class DataManager : MonoBehaviour
{
    public List<Annotation> annotations;
    public List<AnnotationJoint> annotationJoints;
    private bool isInited = false;

    public void Init()
    {
        if(isInited)
        {
            return;
        }
        isInited = true;
        annotationJoints = new List<AnnotationJoint>();
        annotations = new List<Annotation>();
        //LoadData(this);
    }

    public void Awake()
    {
        Init();
    }

    public Annotation FindID(AnnotationID id)
    {
        return annotations.Find(x => x.ID == id);
    }

    public Annotation AddAnnotation(AnnotationID id)
    {
        Annotation annot = FindID(id);
        if (annot==null)
        {
            annot = new Annotation(id);
            annotations.Add(annot);
            return annot;
        }
        return null;
    }

    // compatible with old code
    public void AddAnnoationRenderInfo(AnnotationRenderInfo renderInfo)
    {
        AnnotationID id = new AnnotationID(renderInfo.Color);
        Annotation annot = FindID(id);
        if (annot==null)
        {
            Annotation newAnnotation = new Annotation(id);
            newAnnotation.renderInfo = renderInfo;
            newAnnotation.info = new AnnotationInfo(renderInfo.Color, new byte[4], 1, 1, "Unknown annotation");
            annotations.Add(newAnnotation);
            Debug.Log("Add annotation (render) " + id);
        }
        else
        {
            annot.renderInfo = renderInfo;
            Debug.Log("Fill annotation (render) " + id);
        }
    }

    public void AddAnnotationInfo(AnnotationInfo info)
    {
        AnnotationID id = new AnnotationID(info.Color);
        Annotation annot = FindID(id);
        if (annot == null)
        {
            Annotation newAnnotation = new Annotation(id);
            newAnnotation.info = info;
            newAnnotation.renderInfo = new AnnotationRenderInfo();
            annotations.Add(newAnnotation);
            Debug.Log("Add annotation (info) " + id);
        }
        else
        {
            annot.info = info;
            Debug.Log("Fill annotation (info) " + id);
        }
    }


    public static void LoadData(DataManager dataManager)
    {
        const string folder = "Database/";
        TextAsset ta = Resources.Load<TextAsset>(folder + "SpotList");
        string[][] csvData = DataLoader.CSVLoader(ta.text);
        if (csvData.Length <= 0)
        {
            return;
        }

        // InputStream dataset = assetManager.open(assetHeader);

        //List<String[]> csvData = CSVReader.read(dataset);
        //dataset.close();

        //if (csvData.size() == 0)
        //    return;

        HashSet<int> indexes = new HashSet<int>();
        //Check that we have 4 values per row FOR ALL ROWS
        for (int i = 0; i < csvData.Length; i++)
        {
            string[] row = csvData[i];

            if (row.Length == 0)
            {
                continue;
            }


            //Check that we have 4 values per row FOR ALL ROWS
            if (row.Length != 4)
                throw new Exception("The CSV file does not contain the correct number of entries per row");

            if (i != 0)
            {

                //Get ID...
                int index = int.Parse(row[0]);
                if (indexes.Contains(index))
                    throw new Exception("The Index " + index + " is duplicated in the dataset");

                //...and read the text associated to it

                TextAsset textStream = Resources.Load<TextAsset>(folder + "text" + row[0]);
                if(textStream==null)
                {
                    Debug.LogError("Cannot open " + folder + "text" + row[0]);
                }
                //InputStream textStream = assetManager.open("text" + row[0] + ".txt");
                //ByteArrayOutputStream text = new ByteArrayOutputStream();

                //byte[] buffer = new byte[1024];
                //for (int length; (length = textStream.read(buffer)) != -1;)
                //{
                //    text.write(buffer, 0, length);
                //}
                //textStream.close();
                //...and read the image associated to it
                Texture2D img = Resources.Load<Texture2D>(folder + "img" + row[0]);
                img = Utils.MakeTextureReadable(img);
                //byte[] colors = Utils.ArrayColor32ToByte(img.GetPixels32(0));
                //ImageConversion.LoadImage(img, imageAsset.bytes)
                //img.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
                //InputStream imgStream = assetManager.open("img" + row[0] + ".png");
                //Drawable img = Drawable.createFromStream(imgStream, null);
                //imgStream.close();

                //Check if this chunk is the default entry describing the whole dataset
                //If it is, then it is associated with no layer (layer == -1) and its color is set to transparency (color == 0x00000000)
                bool isDefault = true;
                for (int j = 1; j < 4; j++)
                {
                    if (row[j] != "-")
                    {
                        isDefault = false;
                        break;
                    }
                }

                //If default: No color and no layer and add the data chunk
                //By Yucheng: I just ignore this anyway
                if (isDefault)
                {
                    /*
                    if (m_defaultAnnotationInfo != null)
                        throw new IllegalArgumentException("The dataset contains multiple default data entry");
                    m_defaultAnnotationInfo = new AnnotationInfo(index, -1, -1, 0x00, text.toString("UTF-8"), img);
                    m_data.put(index, m_defaultAnnotationInfo);*/
                }

                //Else, read color + layer and add the data chunk
                else
                {
                    //Get the IDs
                    int layer = int.Parse(row[2]);
                    int id = int.Parse(row[3]);

                    //Get the color as RGBA following the format (r,g,b,a)
                    if (!row[1].StartsWith("(") || !row[1].EndsWith(")"))
                        throw new Exception("One color entry is invalid");
                    String colorStr = row[1].Substring(1, row[1].Length - 2);
                    String[] colorArray = colorStr.Split(',');
                    if (colorArray.Length != 4)
                        throw new Exception("One color entry is invalid");
                    Color32 colorRGBA = new Color32(0, 0, 0, 0);
                    for (int j = 0; j < colorArray.Length; j++)
                        colorRGBA[j] = (byte)Math.Max(0, Math.Min(255, int.Parse(colorArray[j])));

                    //Save the content
                    AnnotationInfo annotationInfo = new AnnotationInfo(
                        colorRGBA,
                        img.GetRawTextureData(),//.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0),
                        img.width,
                        img.height,
                        textStream==null?"":textStream.text
                        );
                    dataManager.AddAnnotationInfo(annotationInfo);
                }
            }
        }

    }


}
