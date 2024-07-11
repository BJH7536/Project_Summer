using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; // ���� ��� (�÷��̾�)
    public Vector3 offset = new Vector3(0, 10, -10); // ī�޶� ������
    public float smoothSpeed = 0.125f; // �ε巯�� �̵� �ӵ�

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}