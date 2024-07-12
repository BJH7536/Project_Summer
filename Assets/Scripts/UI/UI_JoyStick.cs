using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform joystickHandle;
    public Canvas canvas;

    private Vector2 joystickStartPos;
    private bool isJoystickActive = false;

    private void Start()
    {
        joystickHandle.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ActivateJoystick(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DeactivateJoystick();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - joystickStartPos;
        float maxDistance = joystickHandle.sizeDelta.x / 2f;
        Vector2 clampedDirection = Vector2.ClampMagnitude(direction, maxDistance);
        joystickHandle.position = joystickStartPos + clampedDirection;
    }

    private void ActivateJoystick(Vector2 position)
    {
        joystickHandle.position = position;
        joystickHandle.gameObject.SetActive(true);
        joystickStartPos = position;
        isJoystickActive = true;
    }

    private void DeactivateJoystick()
    {
        joystickHandle.gameObject.SetActive(false);
        isJoystickActive = false;
    }
}
