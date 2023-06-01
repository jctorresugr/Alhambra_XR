
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class Utils
{
    public static T EnsureComponent<T>(MonoBehaviour obj, ref T comp)
    {
        if(comp==null)
        {
            comp = obj.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogWarning("Cannot init component for " + obj.ToString() + ", expect component: " + comp.GetType().Name);
            }
        }
        return comp;
    }

    public static T EnsureComponent<T>(GameObject obj, ref T comp)
    {
        if (comp == null)
        {
            comp = obj.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogWarning("Cannot init component for " + obj.ToString() + ", expect component: " + comp.GetType().Name);
            }
        }
        return comp;
    }

    // get component, if not exists, add the compoennt
    public static T ForceToGetComponent<T>(GameObject obj, ref T comp) where T:Component
    {
        if(comp==null)
        {
            comp = obj.GetComponent<T>();
            if (comp == null)
            {
                comp = obj.GetComponentInChildren<T>();
                if(comp==null)
                {
                    comp = obj.gameObject.AddComponent<T>();
                }
            }
        }
        return comp;
    }

    //Slow conversion, you can use C# unsafe function to convert it forcely.
    //In c you can directly cast the pointer :(
    public static byte[] ArrayColor32ToByte(Color32[] c)
    {
        byte[] bytes = new byte[c.Length * 4];
        for(int i=0;i<c.Length;i++)
        {
            int i2 = i << 2;
            bytes[i2] = c[i].r;
            bytes[i2+1] = c[i].g;
            bytes[i2+2] = c[i].b;
            bytes[i2+3] = c[i].a;
        }
        return bytes;
    }

    //Force the texture to be readable
    //Consuming but at least we can read texture!
    //why we cannot read image directly to cpu memory in the unity???????????????????????????????
    //ImageConversion only support compressed byte array flow??? (where is rgba???)
    public static Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    public static Vector3 MulVector3(Vector3 a, Vector3 b)
    {
        return new Vector3
            (
            a.x * b.x,
            a.y * b.y,
            a.z * b.z
            );
    }

    public static void SaveFile(string fileName, string data)
    {
        if (File.Exists(fileName))
        {
            Debug.Log("Already Exists " + fileName);
        }
        var sr = File.CreateText(fileName);
        sr.WriteLine(data);
        sr.Close();
    }

    public static string ReadFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            return File.ReadAllText(fileName);
        }else
        {
            return null;
        }
    }

    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(
            Mathf.Abs(v.x),
            Mathf.Abs(v.y),
            Mathf.Abs(v.z)
            ) ;
    }

    public static float Sqr(float a)
    {
        return a * a;
    }
    public static float BoundsDistance(Bounds a, Bounds b)
    {
        /*
        if(a.Intersects(b))
        {
            return 0.0f;
        }
        Bounds bound = a.Intersects(b);*/
        float distance = 0.0f;
        for(int i=0;i<3;i++)
        {
            float amin = a.min[i];
            float amax = a.max[i];
            float bmax = b.max[i];
            float bmin = b.min[i];
            if(amax>bmin && amin<bmax)
            {
                continue;
            }
            else
            {
                if(amin>bmax)
                {
                    distance += Sqr(amin - bmax);
                }else if (amax<bmin)
                {
                    distance += Sqr(bmin - amax);
                }
            }
        }
        return Mathf.Sqrt(distance);
        //Vector3 r = Vector3.Min(Abs(a.max - b.min),Abs(b.max-a.min));
        //return Mathf.Min(r.x, r.y, r.z);

    }

    public static float BoundsDistance(Bounds a, Vector3 b)
    {
        float distance = 0.0f;
        for (int i = 0; i < 3; i++)
        {
            float amin = a.min[i];
            float amax = a.max[i];
            float bi = b[i];
            if (amax > bi && amin < bi)
            {
                continue;
            }
            else
            {
                if (amin > bi)
                {
                    distance += Sqr(amin - bi);
                }
                else if (amax < bi)
                {
                    distance += Sqr(bi - amax);
                }
            }
        }
        return Mathf.Sqrt(distance);
    }

    public static float BoundsDistance(Vector3 a, Bounds b)
    {
        return BoundsDistance(b, a);
    }

    public static float BoundsDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a,b);
    }

    public static float Volume(Vector3 size)
    {
        return size.x * size.y * size.z;
    }


}
