using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerB : MonoBehaviour
{
    public void MoveByNetworkManager(float x,float y,float z)
    {
        //Debug.LogWarning($"(x, y, z) : ({x}, {y},{z})");
        Vector3 v3 = new Vector3(x,y,z);

        transform.position=v3;

    }
}
