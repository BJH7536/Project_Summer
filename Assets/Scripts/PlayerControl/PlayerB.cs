using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerB : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed = 5f; // 이동 속도 조절

    // 토핑 프리팹들을 참조할 수 있는 딕셔너리
    private Dictionary<Define.Topping, GameObject> toppingPrefabs = new Dictionary<Define.Topping, GameObject>();
    // 현재 게임에 생성되어 있는 토핑 오브젝트들을 관리하는 리스트
    private List<GameObject> activeToppings = new List<GameObject>();

    void Start()
    {
        targetPosition = transform.position; // 초기 위치 설정

        // 토핑 프리팹을 로드하거나 초기화하는 코드 추가
        LoadToppingPrefabs();
    }

    private void LoadToppingPrefabs()
    {
        // Prefabs/Pizza Dough 경로에서 토핑 프리팹들을 로드합니다.
        toppingPrefabs[Define.Topping.Cheese] = Resources.Load<GameObject>("Prefabs/Pizza Dough/CheeseTopping");
        toppingPrefabs[Define.Topping.Pepperoni] = Resources.Load<GameObject>("Prefabs/Pizza Dough/PepperoniTopping");
        toppingPrefabs[Define.Topping.Bulgogi] = Resources.Load<GameObject>("Prefabs/Pizza Dough/BulgogiTopping");
        toppingPrefabs[Define.Topping.Pineapple] = Resources.Load<GameObject>("Prefabs/Pizza Dough/PineappleTopping");
        toppingPrefabs[Define.Topping.Shrimp] = Resources.Load<GameObject>("Prefabs/Pizza Dough/ShrimpTopping");
        toppingPrefabs[Define.Topping.Mushroom] = Resources.Load<GameObject>("Prefabs/Pizza Dough/MushroomTopping");
        toppingPrefabs[Define.Topping.Bacon] = Resources.Load<GameObject>("Prefabs/Pizza Dough/BaconTopping");
        // 필요시 다른 토핑도 추가
    }

    public void MoveByNetworkManager(float x, float y, float z)
    {
        targetPosition = new Vector3(x, y, z); // 목표 위치 설정
        StopAllCoroutines(); // 기존 코루틴을 멈추고
        StartCoroutine(MoveSmoothly()); // 새로운 위치로 부드럽게 이동
    }

    private IEnumerator MoveSmoothly()
    {
        while ((transform.position - targetPosition).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // 다음 프레임까지 대기
        }

        transform.position = targetPosition; // 최종 위치 보정
    }

    public void HoldTopping(string toppingType)
    {
        // toppingType을 Define.Topping enum으로 변환
        if (Enum.TryParse(toppingType, out Define.Topping toppingEnum))
        {
            if (toppingPrefabs.ContainsKey(toppingEnum))
            {
                // 토핑 프리팹을 생성하여 잡기 동작을 반영
                GameObject toppingInstance = Instantiate(toppingPrefabs[toppingEnum], transform.position, Quaternion.identity);
                toppingInstance.transform.SetParent(this.transform); // 플레이어에 붙이기
                activeToppings.Add(toppingInstance); // 활성화된 토핑 리스트에 추가

                Debug.Log($"Holding topping: {toppingType}");
            }
            else
            {
                Debug.LogWarning($"Topping prefab not found for type: {toppingType}");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid topping type: {toppingType}");
        }
    }

    public void ReleaseTopping(string toppingType)
    {
        // toppingType을 Define.Topping enum으로 변환
        if (Enum.TryParse(toppingType, out Define.Topping toppingEnum))
        {
            // 활성화된 토핑 리스트에서 해당 타입의 토핑을 찾음
            GameObject toppingToRemove = activeToppings.Find(topping => topping.name.Contains(toppingEnum.ToString()));

            if (toppingToRemove != null)
            {
                toppingToRemove.transform.SetParent(null); // 플레이어에서 분리
                Destroy(toppingToRemove); // 오브젝트를 파괴하여 제거
                activeToppings.Remove(toppingToRemove); // 활성화된 토핑 리스트에서 제거

                Debug.Log($"Releasing topping: {toppingType}");
            }
            else
            {
                Debug.LogWarning($"No active topping found for type: {toppingType}");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid topping type: {toppingType}");
        }
    }
}
