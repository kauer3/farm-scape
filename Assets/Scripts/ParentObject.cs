using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentObject : MonoBehaviour
{
    public void CheckForDestroy()
    {
        Debug.Log(transform.childCount + " Children");
        if (transform.childCount <= 1)
        {
            Debug.Log("Destroying parent object");
            Destroy(gameObject);
        }
    }
}
