using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private float speed = 15f;

    private PlayerInputActions _playerInputActions;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
        _playerInputActions.Game.Move.performed += OnMovePerformed;
        _playerInputActions.Game.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        _playerInputActions.Game.Move.performed -= OnMovePerformed;
        _playerInputActions.Game.Move.canceled -= OnMoveCanceled;
        _playerInputActions.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _animator.SetBool("isRun", true);
        inputVector = context.ReadValue<Vector2>();

    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        _animator.SetBool("isRun", false);
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y) * speed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + movement);

        transform.LookAt(transform.position+movement);
    }
}
