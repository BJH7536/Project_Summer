using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Dough : MonoBehaviour
{
    [SerializeField] public Define.PizzaType Type;
    
    private Stack<Define.Topping> toppingsStack = new Stack<Define.Topping>();

    // 재료를 추가하는 함수
    public void AddTopping(Define.Topping topping)
    {
        toppingsStack.Push(topping); // 스택에 재료 추가
        Debug.Log("Added topping: " + topping);
        SendDoughInfoToServer();
    }

    private void SendDoughInfoToServer()
    {
        string toppingsInfo = GenerateToppingsInfo();
        //Debug.Log(toppingsInfo);
        NetworkManager.instance.player_on_network.SendMessage(toppingsInfo);
    }

    // Dough의 스택에 있는 모든 토핑 정보를 문자열로 변환
    private string GenerateToppingsInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Dough:{Type}:Toppings[");

        foreach (var topping in toppingsStack)
        {
            sb.Append(topping.ToString());
            sb.Append(",");
        }

        if (toppingsStack.Count > 0)
            sb.Remove(sb.Length - 1, 1);  // 마지막 콤마 제거

        sb.Append("]\n");
        return sb.ToString();
    }

    // 최상위 재료를 제거하고 반환하는 함수
    public Define.Topping RemoveTopping()
    {
        if (toppingsStack.Count == 0)
        {
            Debug.Log("No toppings to remove!");
            return Define.Topping.Unknown; // 임의의 값 또는 예외 처리를 해줍니다.
        }

        Define.Topping removedTopping = toppingsStack.Pop(); // 최상위 재료 제거
        Debug.Log("Removed topping: " + removedTopping);
        return removedTopping;
    }

    // 최상위 재료를 확인하는 함수
    public Define.Topping PeekTopping()
    {
        if (toppingsStack.Count == 0)
        {
            Debug.Log("No toppings available!");
            return Define.Topping.Unknown; // 임의의 값 또는 예외 처리를 해줍니다.
        }

        Define.Topping topTopping = toppingsStack.Peek(); // 최상위 재료 확인
        Debug.Log("Top topping: " + topTopping);
        return topTopping;
    }

    // 현재 스택에 있는 모든 재료를 배열로 반환하는 함수
    public Define.Topping[] GetAllToppings()
    {
        Define.Topping[] toppingsArray = toppingsStack.ToArray();
        return toppingsArray;
    }

    // 스택을 비우는 함수 (초기화 등에 사용)
    public void ClearToppings()
    {
        toppingsStack.Clear();
        Debug.Log("Toppings cleared!");
    }
}