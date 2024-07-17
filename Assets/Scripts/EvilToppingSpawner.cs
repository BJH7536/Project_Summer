using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilToppingSpawner : MonoBehaviour
{
    [SerializeField] private GameObject EvilTopping;
    
    public float spawnInterval = 10.0f;     // 스폰 간격
    public float spawnChance = 0.05f;       // 스폰 확률 
    public Vector3 spawnAreaSize = new Vector3(1, 1, 1); // 충돌 검사 영역 크기

    void Start()
    {
        // 스폰 코루틴 시작
        StartCoroutine(SpawnObject());
    }

    IEnumerator SpawnObject()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (Random.value < spawnChance)
            {
                // 충돌체 검사
                if (!Physics.CheckBox(transform.position, spawnAreaSize / 2, Quaternion.identity))
                {
                    Instantiate(EvilTopping, transform.position, transform.rotation);
                    //Debug.Log("Object Spawned!");
                }
                else
                {
                    //Debug.Log("Spawn location is occupied.");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // 스폰 위치와 크기를 Gizmo로 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}