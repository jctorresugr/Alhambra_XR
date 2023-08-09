using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
    public bool useAsset = true;

    //????
    //if this is assigned, we will not load from Resources
    public Texture2D preAssignedIndexTexture = null;

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
        File.WriteAllBytes(path + ".bytes", data);
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
        return ReadAllBytes(path+".png");
    }

    protected byte[] LoadImageBin(string path)
    {
        return ReadAllBytes(path + ".bytes");
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
                if (preAssignedIndexTexture != null)
                {
                    data.IndexTexture = preAssignedIndexTexture;
                }
            }
        }
        else
        {
            if(!Exists(jsonPath))
            {
                Debug.LogWarning("Cannot find file <" + jsonPath+">, ignore");
                return;
            }
            string s = ReadAllText(jsonPath);
            Debug.Log("Load at " + jsonPath);
            JsonUtility.FromJsonOverwrite(s, data);

            foreach (Annotation annot in data.Annotations)
            {

                annot.info.SnapshotRGBA = LoadImageBin(imagePath + AnnotationIDSuffix(annot.ID));
            }
            data.PostDeserialize();
            Texture2D finalTexture = null;
            if(preAssignedIndexTexture!=null)
            {
                finalTexture = preAssignedIndexTexture;
                Debug.Log("Load predefined");
            }
            else if(Exists(imagePath + "index.png"))
            {
                Texture2D texture2D;
                if (!useAsset)
                {
                    byte[] indexImageBytes = LoadImage(imagePath + "index");
                    texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    ImageConversion.LoadImage(texture2D, indexImageBytes, false);

                    
                }
                else
                {
                    texture2D = Resources.Load<Texture2D>(imagePath + "index");
                }
                texture2D.filterMode = FilterMode.Point;
                texture2D.minimumMipmapLevel = 0;
                texture2D.requestedMipmapLevel = 0;
                finalTexture = texture2D;

            }
            else
            {
                Debug.LogWarning("Cannot find index texture: " + imagePath + "index");
            }
            data.IndexTexture = finalTexture;
            /*
            if (finalTexture != null)
            {
                //data.IndexTexture.Resize(2048, 2048);
                Texture2D resizedTexture = new Texture2D(2048,2048, finalTexture.format,false,false);
                Graphics.ConvertTexture(finalTexture, resizedTexture);
                data.IndexTexture = resizedTexture;
            }*/
            

        }

        
    }

    protected byte[] ReadAllBytes(string path)
    {
        if(useAsset)
        {
            path = Path.ChangeExtension(path, null);
            return Resources.Load<TextAsset>(path).bytes;
        }
        else
        {
            return File.ReadAllBytes(path);
        }
    }

    protected string ReadAllText(string path)
    {
        if (useAsset)
        {
            path = Path.ChangeExtension(path, null);
            return Resources.Load<TextAsset>(path).text;
        }
        else
        {
            return File.ReadAllText(path);
        }
    }
    protected bool Exists(string path)
    {
        if(useAsset)
        {
            return true;
        }
        else
        {
            return File.Exists(path);
        }
    }
}
#if UNITY_EDITOR
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
#endif