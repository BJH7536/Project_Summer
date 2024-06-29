using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Vector3 followOffset = new Vector3(0, 10, -10);
    public Vector3 lookAtOffset = new Vector3(0, 2, 0);

    private void Start()
    {
        if (virtualCamera != null)
        {
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = followOffset;
            }

            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_TrackedObjectOffset = lookAtOffset;
            }
        }
    }
}
