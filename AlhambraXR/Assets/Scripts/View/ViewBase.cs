using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ViewBase<T> : MonoBehaviour
{
    public abstract void Init(T data);
}
