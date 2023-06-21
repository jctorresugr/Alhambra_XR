using UnityEngine;

/// <summary>
/// The purpose of this class is to have a steady UI canvas that follows the head of the user
/// </summary>
public class UICamera : MonoBehaviour
{
    void Start()
    {}

    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.z = 0;
        transform.rotation = Quaternion.Euler(euler);
    }
}