using System.Collections;
using UnityEngine;

public class PlayerB : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed = 5f; // 이동 속도 조절

    void Start()
    {
        targetPosition = transform.position; // 초기 위치 설정
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
}
