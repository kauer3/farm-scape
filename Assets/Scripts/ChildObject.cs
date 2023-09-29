using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildObject : MonoBehaviour
{
    private void OnDestroy()
    {
        transform.parent.GetComponent<ParentObject>().CheckForDestroy();
        Debug.Log("Destroyed child object");
    }
}
