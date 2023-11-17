using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineManager : MonoBehaviour
{
    public CinemachineVirtualCamera _vcam;
    private CinemachineFramingTransposer _composer;

    void Start()
    {
        _composer = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }
}
