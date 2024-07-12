using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed; //속도
    public Vector3 direction;//이동방향
    public List<Rigidbody> onBelt = new List<Rigidbody>();//벨트 위의 물체 리스트

//fixedUpdate를 사용해 물체를 이동
    void FixedUpdate()
    {
        foreach (var rb in onBelt)
        {
            rb.velocity = speed * direction;
        }
    }

//물체가 들어왔을 때 
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !onBelt.Contains(rb))
        {
            onBelt.Add(rb);
        }
    }

//물체가 나갈 때
    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && onBelt.Contains(rb))
        {
            rb.velocity = Vector3.zero;
            onBelt.Remove(rb);
        }
    }
}
