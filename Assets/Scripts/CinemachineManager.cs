using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineManager : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer composer;

    void Start()
    {
        composer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void UpdateDeadZone(float width, float height)
    {
        composer.m_DeadZoneWidth = width;
        composer.m_DeadZoneHeight = height;
    }
}
