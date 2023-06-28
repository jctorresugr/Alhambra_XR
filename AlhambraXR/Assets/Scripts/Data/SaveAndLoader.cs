using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveAndLoader : MonoBehaviour
{

    public DataManager data;
    //new loading method
    public string jsonPath = "MiddleSave/fakeBigAnnotations.json";
    public string imagePath = "MiddleSave/";
    //old loading method, if this field is setted, then we use old method to load data.
    public string oldPath = "Database/";
    public bool debugSaveAsPNG = true;

    public void Save()
    {
        Debug.Log("Save at " + jsonPath);
        string s = JsonUtility.ToJson(data);
        File.WriteAllText(jsonPath, s);

        //save images
        foreach(Annotation annot in data.Annotations)
        {
            SaveImage(
                imagePath + AnnotationIDSuffix(annot.ID),
                annot.info.SnapshotRGBA,
                annot.info.SnapshotWidth,
                annot.info.SnapshotHeight
                );
        }
    }

    protected string AnnotationIDSuffix(AnnotationID id)
    {
        return $"{id.Layer}_{id.ID}";
    }

    protected void SaveImage(string path, byte[] data,int w, int h)
    {
        //debug visualization
        if(debugSaveAsPNG)
        {
            byte[] pngCodings = ImageConversion.EncodeArrayToPNG(
            data,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
            (uint)w, (uint)h);
            File.WriteAllBytes(path + ".png", pngCodings);
        }
        File.WriteAllBytes(path+".bin", data);

    }

    protected void LoadImage(string path, AnnotationInfo info)
    {
        byte[] codings = File.ReadAllBytes(path+".bin");
        info.SnapshotRGBA = codings;
    }

    public void Load()
    {
        if (oldPath != "" && oldPath != null)
        {
            Debug.Log("Load at " + oldPath);
            DataManager.LoadData(data, oldPath);
            //dirty code
            if (oldPath == "Database/")
            {
                foreach (Annotation annot in data.Annotations)
                {
                    annot.isLocalData = true;
                }
                //TODO: test code, commnet it for real dataset
                AnnotationJoint joint = data.AddAnnotationJoint("Room A");
                foreach (var annot in data.Annotations)
                {
                    joint.AddAnnotation(annot);
                }
            }
        }
        else
        {
            string s = File.ReadAllText(jsonPath);
            Debug.Log("Load at " + jsonPath);
            JsonUtility.FromJsonOverwrite(s, data);

            foreach (Annotation annot in data.Annotations)
            {
                LoadImage(
                    imagePath + AnnotationIDSuffix(annot.ID),
                    annot.info);
            }
        }

        
    }
}

[CustomEditor(typeof(SaveAndLoader))]

public class SaveAndLoaderEditor : Editor
{


    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();



        SaveAndLoader myScript = (SaveAndLoader)target;

        if(Application.isPlaying)
        {
            if (GUILayout.Button("Save"))
            {
                myScript.Save();
            }
            else if (GUILayout.Button("Load"))
            {
                myScript.Load();
            }
        }
        

    }

}