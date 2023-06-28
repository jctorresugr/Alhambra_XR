using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IHelpNodeProvider : MonoBehaviour
{
    public abstract List<Vector3> GetHelpNodePositions();
}
