using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaunalHelpNodeList : IHelpNodeProvider
{
    [Header("Components")]
    public ReferenceTransform referenceTransform;
    [Header("Data")]
    public List<Transform> transforms = new List<Transform>();

    public override List<Vector3> GetHelpNodePositions()
    {
        return transforms.ConvertAll(
            c => 
            referenceTransform.InvMapPosition(c.position)
            );
    }
}
