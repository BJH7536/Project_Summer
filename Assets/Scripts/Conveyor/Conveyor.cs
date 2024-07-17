using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;

public class Conveyor : MonoBehaviour
{
    public float speed;                     //속도
    public Vector3 direction;               //이동방향
    public GameObject[] doughPrefabs;                //피자도우 프리팹
    public Transform doughSpawnPosition;    // 피자 도우가 스폰될 장소
    
    //벨트 위의 물체 리스트
    public List<Rigidbody> onBelt = new List<Rigidbody>();  
    
    [Button]
    public Define.PizzaType MakeRandomDough()
    {
        GameObject dough = Instantiate<GameObject>(doughPrefabs[Random.Range(0, doughPrefabs.Length)]);
        dough.transform.position = doughSpawnPosition.position;

        Define.PizzaType doughType = GetRandomDough();
        dough.GetComponent<Dough>().Type = doughType;
        return doughType;
    }
   
    public Define.PizzaType GetRandomDough()
    {
        // Unknown을 제외한 값을 가져오기 위해 Length - 1을 사용
        Array values = Enum.GetValues(typeof(Define.PizzaType));
        System.Random random = new System.Random();
        return (Define.PizzaType)values.GetValue(random.Next(1, values.Length));
    }
    
    //fixedUpdate를 사용해 물체를 이동
    void FixedUpdate()
    {
        foreach (var rb in onBelt)
        {
            rb.velocity = speed * direction.normalized;
        }
    }

    //물체가 들어왔을 때 
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !onBelt.Contains(rb) && collision.gameObject.CompareTag("Dough"))
        {
            onBelt.Add(rb);
            var position = rb.position;
            position = new Vector3(position.x, position.y, transform.position.z);
            rb.position = position;
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
