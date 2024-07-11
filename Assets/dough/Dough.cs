using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dough : MonoBehaviour
{
    private Stack<int> toppingsStack = new Stack<int>();

    // 재료를 추가하는 함수
    public void AddTopping(int topping)
    {
        toppingsStack.Push(topping); // 스택에 재료 추가
        Debug.Log("Added topping: " + topping);
    }

    // 최상위 재료를 제거하고 반환하는 함수
    public int RemoveTopping()
    {
        if (toppingsStack.Count == 0)
        {
            Debug.Log("No toppings to remove!");
            return -1; // 임의의 값 또는 예외 처리를 해줍니다.
        }

        int removedTopping = toppingsStack.Pop(); // 최상위 재료 제거
        Debug.Log("Removed topping: " + removedTopping);
        return removedTopping;
    }

    // 최상위 재료를 확인하는 함수
    public int PeekTopping()
    {
        if (toppingsStack.Count == 0)
        {
            Debug.Log("No toppings available!");
            return -1; // 임의의 값 또는 예외 처리를 해줍니다.
        }

        int topTopping = toppingsStack.Peek(); // 최상위 재료 확인
        Debug.Log("Top topping: " + topTopping);
        return topTopping;
    }

    // 현재 스택에 있는 모든 재료를 배열로 반환하는 함수 (디버깅 용도)
    public int[] GetAllToppings()
    {
        int[] toppingsArray = toppingsStack.ToArray();
        return toppingsArray;
    }

    // 스택을 비우는 함수 (초기화 등에 사용)
    public void ClearToppings()
    {
        toppingsStack.Clear();
        Debug.Log("Toppings cleared!");
    }
}