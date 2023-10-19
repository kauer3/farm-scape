using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentObject : MonoBehaviour
{
    public void CheckForDestroy()
    {
        if (transform.childCount <= 1)
        {
            Destroy(gameObject);
        }
    }
}
