using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationCacheInteraction : MonoBehaviour, PickPano.IPickPanoListener
{
    public NavigationCache navigationCache;
    public PickPano pickPano;
    public Camera userCamera;
    public ReferenceTransform referenceTransform;
    [Header("Settings")]
    public float nearbyThreshold = 1.0f;
    public float nearbyViewDegree = 30.0f;
    public float nearbyDisappearTime = 5.0f;

    protected struct NearbyAnnotationInfo
    {
        public AnnotationID id;
        public Vector3 position;

        public NearbyAnnotationInfo(AnnotationID id, Vector3 position)
        {
            this.id = id;
            this.position = position;
        }
    }
    private List<Annotation> oldList;
    private List<NearbyAnnotationInfo> annotPos = new List<NearbyAnnotationInfo>();

    void PickPano.IPickPanoListener.OnHover(PickPano pano, Color c)
    {
       
    }

    void PickPano.IPickPanoListener.OnSelection(PickPano pano, Color c)
    {
        AnnotationID id = new AnnotationID(c);
        navigationCache.RemoveNavigationToDestionation(id);

    }

    void PickPano.IPickPanoListener.OnSetTexture(PickPano pano, Texture2D newIndexTexture)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        pickPano.AddListener(this);
    }

    void OnDestroy()
    {
        pickPano.RemoveListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(navigationCache.IsCached)
        {
            VolumeNavigation.NavigationInfo cr = navigationCache.CacheResult;
            if(cr.annotations!=oldList)
            {
                oldList = cr.annotations;
                annotPos.Clear();
                foreach(Annotation annot in cr.annotations)
                {
                    //oldList.Add(new NearbyAnnotationInfo(annot.ID,))
                }
            }
        }
    }
}
