using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    public bool debugSaveAsPng = true;

    public void Save()
    {
        Debug.Log("Save at " + jsonPath);
        Utils.CreateFolder(jsonPath);
        if(!Directory.Exists(imagePath))
        {
            Directory.CreateDirectory(imagePath);
        }
        
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

        SaveImage(
            imagePath + "index",
            data.IndexTexture.GetPixelData<byte>(0).ToArray(),
            data.IndexTexture.width,
            data.IndexTexture.height,
            true
            );
    }

    protected string AnnotationIDSuffix(AnnotationID id)
    {
        return $"{id.Layer}_{id.ID}";
    }


    protected void SaveImage(string path, byte[] data,int w, int h,bool saveAsPNG=false)
    {
        //debug visualization
        if(debugSaveAsPng || saveAsPNG)
        {
            SaveImagePNG(path, data, w, h);
        }
        File.WriteAllBytes(path + ".bin", data);
    }

    protected void SaveImagePNG(string path, byte[] data, int w, int h)
    {
        byte[] pngCodings = ImageConversion.EncodeArrayToPNG(
            data,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
            (uint)w, (uint)h);
        File.WriteAllBytes(path + ".png", pngCodings);
    }

    protected byte[] LoadImage(string path)
    {
        return File.ReadAllBytes(path+".png");
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
            if(!File.Exists(jsonPath))
            {
                Debug.LogWarning("Cannot find file <" + jsonPath+">, ignore");
                return;
            }
            string s = File.ReadAllText(jsonPath);
            Debug.Log("Load at " + jsonPath);
            JsonUtility.FromJsonOverwrite(s, data);

            foreach (Annotation annot in data.Annotations)
            {

                annot.info.SnapshotRGBA = LoadImage(imagePath + AnnotationIDSuffix(annot.ID));
            }
            data.PostDeserialize();
            if(File.Exists(imagePath + "index.png"))
            {
                byte[] indexImageBytes = LoadImage(imagePath + "index");
                Texture2D texture2D = new Texture2D(2, 2);
                ImageConversion.LoadImage(texture2D, indexImageBytes);
                texture2D.filterMode = FilterMode.Point;
                texture2D.minimumMipmapLevel = 0;
                texture2D.requestedMipmapLevel = 0;
                texture2D.Apply();
                data.IndexTexture = texture2D;
                //debug code
                byte[] vs = texture2D.GetPixelData<byte>(0).ToArray();
            }
            else
            {
                Debug.LogWarning("Cannot find index texture: " + imagePath + "index");
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