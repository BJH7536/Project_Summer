using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerB : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed = 5f; // 이동 속도 조절

    // Inspector에서 할당할 수 있는 토핑 프리팹 리스트
    [SerializeField] private List<GameObject> toppingPrefabsList;
    private Dictionary<string, GameObject> toppingPrefabs = new Dictionary<string, GameObject>();

    // 현재 게임에 생성되어 있는 토핑 오브젝트들을 관리하는 리스트
    private List<Holdable> activeToppings = new List<Holdable>();

    [SerializeField] private Transform holdPosition; // HoldPosition 참조 추가

    void Start()
    {
        targetPosition = transform.position; // 초기 위치 설정

        // Inspector에서 설정한 프리팹 리스트를 딕셔너리에 추가
        InitializeToppingPrefabs();
    }

    private void InitializeToppingPrefabs()
    {
        // toppingPrefabsList에 있는 프리팹들을 toppingPrefabs 딕셔너리에 추가합니다.
        foreach (var prefab in toppingPrefabsList)
        {
            Holdable holdable = prefab.GetComponent<Holdable>();
            if (holdable != null)
            {
                string toppingName = prefab.name.Replace(" ", string.Empty); // 공백 제거
                toppingPrefabs[toppingName] = prefab;
            }
            else
            {
                Debug.LogWarning($"The prefab {prefab.name} does not have a Holdable component.");
            }
        }
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
        // 전달된 toppingType을 기반으로 프리팹을 검색하여 생성
        string sanitizedToppingType = toppingType.Replace(" ", string.Empty); // 공백 제거

        foreach (var toppingPrefab in toppingPrefabs)
        {
            if (toppingPrefab.Key.StartsWith(sanitizedToppingType, StringComparison.OrdinalIgnoreCase))
            {
                // Instantiate를 사용하여 프리팹을 HoldPosition에서 생성
                GameObject toppingInstance = Instantiate(toppingPrefab.Value, holdPosition.position, Quaternion.identity);
                Holdable toppingHoldable = toppingInstance.GetComponent<Holdable>();

                if (toppingHoldable != null)
                {
                    toppingHoldable.transform.SetParent(holdPosition);
                    toppingHoldable.transform.localPosition = Vector3.zero; // HoldPosition에 정렬
                    Rigidbody toppingRigidbody = toppingInstance.GetComponent<Rigidbody>();
                    if (toppingRigidbody != null)
                    {
                        toppingRigidbody.isKinematic = true;
                    }
                    activeToppings.Add(toppingHoldable); // 활성화된 토핑 리스트에 추가

                    Debug.Log($"Holding topping: {toppingType}");
                    return; // 성공적으로 찾으면 종료
                }
                else
                {
                    Debug.LogWarning("The instantiated prefab does not have a Holdable component.");
                    return; // 문제가 있을 경우 종료
                }
            }
        }

        Debug.LogWarning($"Topping prefab not found for type: {toppingType}");
    }

    public void ReleaseTopping(Holdable topping)
    {
        if (activeToppings.Contains(topping))
        {
            // 토핑을 놓는 동작
            Rigidbody toppingRigidbody = topping.GetComponent<Rigidbody>();
            if (toppingRigidbody != null)
            {
                // Enable physics upon release
                toppingRigidbody.isKinematic = false;
            }
            topping.transform.SetParent(null); // 플레이어에서 분리
            activeToppings.Remove(topping); // 활성화된 토핑 리스트에서 제거
            Destroy(topping.gameObject); // 오브젝트를 파괴하여 제거

            Debug.Log($"Releasing topping: {topping.name}");
        }
        else
        {
            Debug.LogWarning($"Attempted to release a topping not currently held: {topping.name}");
        }
    }

    public Holdable GetToppingByType(string toppingType)
    {
        string sanitizedToppingType = toppingType.Replace(" ", string.Empty); // 공백 제거

        foreach (var topping in activeToppings)
        {
            if (topping.name.StartsWith(sanitizedToppingType, StringComparison.OrdinalIgnoreCase))
            {
                return topping;
            }
        }

        return null;
    }
}
