using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private float speed = 15f;
    [SerializeField] private Transform holdPosition; // 플레이어 앞에 오브젝트를 들 위치
    [SerializeField] private Transform raycastOrigin; // 레이캐스트 시작 위치
    [SerializeField] private float interactDistance = 5f; // 상호작용 거리 (기본값을 5로 증가)

    private PlayerInputActions _playerInputActions;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private GameObject heldObject; // 들고 있는 오브젝트

    private IPlayerState _currentState;
    private PlayerIdleState _idleState;
    private PlayerRunState _runState;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();

        _idleState = new PlayerIdleState();
        _runState = new PlayerRunState();

        ChangeState(_idleState);
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
        _playerInputActions.Game.Move.performed += OnMovePerformed;
        _playerInputActions.Game.Move.canceled += OnMoveCanceled;
        _playerInputActions.Game.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _playerInputActions.Game.Move.performed -= OnMovePerformed;
        _playerInputActions.Game.Move.canceled -= OnMoveCanceled;
        _playerInputActions.Game.Interact.performed -= OnInteractPerformed;
        _playerInputActions.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        ChangeState(_runState);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        ChangeState(_idleState);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (heldObject == null)
        {
            TryPickUpObject();
        }
        else
        {
            DropObject();
        }
    }

    private void TryPickUpObject()
    {
        // => 기존에 tag를 통해서 물체를 감지하던 코드를 Layer기반으로 수정.
        int interactableLayer = LayerMask.GetMask("Interactable");
        
        // 최대 감지할 수 있는 충돌체 수를 정의 (예: 10)
        Collider[] colliders = new Collider[10];
        
        // 충돌체 감지
        int numColliders = Physics.OverlapSphereNonAlloc(raycastOrigin.position, interactDistance, colliders, interactableLayer);

        for (int i = 0; i < numColliders; i++)
        {
            Collider collider = colliders[i];
            heldObject = collider.gameObject;
            heldObject.transform.SetParent(holdPosition);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
            heldObject.GetComponent<Collider>().isTrigger = true;       // 플레이어 캐릭터와 충돌하지 않도록 수정
            Debug.Log("Picked up " + heldObject.name);
            break;
        }
    }

    private void DropObject()
    {
        Debug.Log("Dropped "+heldObject.name);
        heldObject.transform.SetParent(null);
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        heldObject.GetComponent<Collider>().isTrigger = false;
        heldObject = null;
    }

    private void FixedUpdate()
    {
        _currentState?.Execute();
    }

    private void ChangeState(IPlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this);
    }

    private interface IPlayerState
    {
        void Enter(Player player);
        void Execute();
        void Exit();
    }

    private class PlayerIdleState : IPlayerState
    {
        private Player _player;
        public void Enter(Player player)
        {
            _player = player;
            _player._animator.SetBool("isRun", false);
            _player._rigidbody.velocity = Vector3.zero;
            _player._rigidbody.angularVelocity = Vector3.zero;
        }

        public void Execute()
        {
            // Idle 상태에서는 아무것도 하지 않음
        }

        public void Exit() { }
    }

    private class PlayerRunState : IPlayerState
    {
        private Player _player;
        public void Enter(Player player)
        {
            _player = player;
            _player._animator.SetBool("isRun", true);
        }

        public void Execute()
        {
            Vector3 movement = new Vector3(_player.inputVector.x, 0, _player.inputVector.y).normalized * _player.speed;

            if (movement != Vector3.zero)
            {
                // 플레이어 캐릭터의 Rigidbody
                _player._rigidbody.velocity = movement;
                //_player._rigidbody.MovePosition(_player._rigidbody.position + movement * Time.fixedDeltaTime);
                
                //_player.transform.LookAt(_player.transform.position + movement);
                // 바라보는 방향이 좀 더 부드럽게 변하도록 보간을 추가
                _player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, Quaternion.LookRotation(movement), 10*Time.deltaTime);
            }
        }

        public void Exit() { }
    }

    // Gizmos를 사용하여 상호작용 범위를 시각적으로 표시합니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastOrigin.position, interactDistance);
    }
}
