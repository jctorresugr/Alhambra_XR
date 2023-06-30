using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualObjectGroupHelpNode : IHelpNodeProvider
{
    [Header("Components")]
    public ReferenceTransform referenceTransform;
    [Header("Data")]
    public GameObject listObject;
    [Header("Settings")]
    public bool removeAfterCollectInfo = true;
    public List<Vector3> results;
    public override List<Vector3> GetHelpNodePositions()
    {
        return results;
    }

    // Start is called before the first frame update
    void Start()
    {
        results = new List<Vector3>();
        if(listObject==null)
        {
            return;
        }

        Transform[] transforms = listObject.GetComponentsInChildren<Transform>();
        foreach(Transform t in transforms)
        {
            results.Add(referenceTransform.InvMapPosition(t.position));
        }

        if(removeAfterCollectInfo)
        {
            foreach (Transform t in transforms)
            {
                Destroy(t.gameObject);
            }
            Destroy(listObject);
        }
    }
}
