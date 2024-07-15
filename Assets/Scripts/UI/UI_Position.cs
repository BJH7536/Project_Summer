using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Position : MonoBehaviour
{
    public RectTransform joystick;
    public RectTransform actionButton;

    void Start()
    {
        // 조이스틱 위치 설정
        joystick.anchorMin = new Vector2(0, 0);
        joystick.anchorMax = new Vector2(0, 0);
        joystick.pivot = new Vector2(0.5f, 0.5f);
        joystick.anchoredPosition = new Vector2(200, 200);

        // 상호작용 버튼 위치 설정
        actionButton.anchorMin = new Vector2(1, 0);
        actionButton.anchorMax = new Vector2(1, 0);
        actionButton.pivot = new Vector2(0.5f, 0.5f);
        actionButton.anchoredPosition = new Vector2(-200, 212);
    }
}
