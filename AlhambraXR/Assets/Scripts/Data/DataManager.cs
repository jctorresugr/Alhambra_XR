using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Data management, all annotation data stores here
/// </summary>
public class DataManager : MonoBehaviour
{
    public delegate void AnnotationChangeFunc(Annotation annotation);
    public delegate void AnnotationJointChangeFunc(AnnotationJoint annotationJoint);

    public event AnnotationChangeFunc OnAnnotationAddEvent;
    public event AnnotationChangeFunc OnAnnotationRemoveEvent;
    public event AnnotationJointChangeFunc OnAnnotationJointAddEvent;
    public event AnnotationJointChangeFunc OnAnnotationJointRemoveEvent;

    private List<Annotation> annotations;
    private List<AnnotationJoint> annotationJoints; 
    private bool isInited = false;

    public IReadOnlyList<Annotation> Annotations => annotations;  
    public IReadOnlyList<AnnotationJoint> AnnotationJoints => annotationJoints;

    public Annotation FindAnnotation(Predicate<Annotation> predicate)
    {
        return annotations.Find(predicate);
    }

    public bool ExistAnnotation(Predicate<Annotation> predicate)
    {
        return FindAnnotation(predicate)!=null;
    }

    public void Init()
    {
        if(isInited)
        {
            return;
        }
        isInited = true;
        annotationJoints = new List<AnnotationJoint>();
        annotations = new List<Annotation>();
        LoadData(this);

        //TODO: test code, commnet it for real dataset
        AnnotationJoint joint = AddAnnotationJoint("Test Joint 1");
        joint.AddAnnotation(annotations[2]);
        joint.AddAnnotation(annotations[3]);
        joint.AddAnnotation(annotations[4]);

        AnnotationJoint joint2 = AddAnnotationJoint("Test Joint 2");
        joint2.AddAnnotation(annotations[5]);
        joint2.AddAnnotation(annotations[6]);
        joint2.AddAnnotation(annotations[7]);
    }

    public void Awake()
    {
        Init();
    }

    public Annotation FindID(AnnotationID id)
    {
        return annotations.Find(x => x.ID == id);
    }

    public AnnotationJoint FindJointID(int id)
    {
        return annotationJoints.Find(x => x.ID == id);
    }

    public Annotation AddAnnotation(AnnotationID id)
    {
        Annotation annot = FindID(id);
        if (annot==null)
        {
            annot = new Annotation(id);
            annotations.Add(annot);
            OnAnnotationAddEvent?.Invoke(annot);
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
            OnAnnotationAddEvent?.Invoke(newAnnotation);
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
            OnAnnotationAddEvent?.Invoke(newAnnotation);
            Debug.Log("Add annotation (info) " + id);
        }
        else
        {
            annot.info = info;
            Debug.Log("Fill annotation (info) " + id);
        }
    }


    //Annotation Joint operations
    public AnnotationJoint AddAnnotationJoint(string name)
    {
        AnnotationJoint aj;
        int id = annotationJoints.Count - 1;
        do
        {
            id++;
            aj = FindJointID(id);
        } while (aj != null);

        aj = new AnnotationJoint(id, name);
        OnAnnotationJointAddEvent?.Invoke(aj);
        return aj;
    }

    public AnnotationJoint RemoveAnnotationJoint(int id)
    {
        AnnotationJoint aj = annotationJoints.Find(x => x.ID == id);
        if(aj!=null)
        {
            _ = annotationJoints.Remove(aj);
            RemoveJointInformation(aj);
            OnAnnotationJointRemoveEvent?.Invoke(aj);
            return aj;
        }
        return null;
    }

    public bool RemoveAnnotationJoint(AnnotationJoint aj)
    {
        bool result = annotationJoints.Remove(aj);
        if(result)
        {
            RemoveJointInformation(aj);
            OnAnnotationJointRemoveEvent?.Invoke(aj);
        }
        return result;
    }

    protected void RemoveJointInformation(AnnotationJoint aj)
    {
        foreach(Annotation a in aj.Annotations)
        {
            a.RemoveJoint(aj);
        }
    }

    public Annotation RemoveAnnotation(AnnotationID id)
    {
        Annotation a = FindID(id);
        if(RemoveAnnotation(a))
        {
            return a;
        }
        else
        {
            return null;
        }
    }

    public bool RemoveAnnotation(Annotation a)
    {
        bool result = annotations.Remove(a);
        if(result)
        {
            RemoveAnnotationInfomation(a);
            OnAnnotationRemoveEvent?.Invoke(a);
            return true;
        }
        return false;
    }

    protected void RemoveAnnotationInfomation(Annotation a)
    {
        foreach(AnnotationJoint aj in a.Joints)
        {
            aj.RemoveAnnotation(a);
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
                //...and read the image associated to it
                Texture2D img = Resources.Load<Texture2D>(folder + "img" + row[0]);
                img = Utils.MakeTextureReadable(img);
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
