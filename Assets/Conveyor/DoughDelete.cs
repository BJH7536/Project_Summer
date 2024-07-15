using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoughDelete : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dough"))
        {
            Destroy(other.gameObject);
        }
    }
}
