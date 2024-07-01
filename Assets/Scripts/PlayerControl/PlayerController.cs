using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPun
{
    [Header("Information")]
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private float speed = 15f;

    private PlayerInputActions _playerInputActions;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private IPlayerState _currentState;
    private PlayerIdleState _idleState;
    private PlayerRunState _runState;

    /// <summary>
    /// 로컬 플레이어의 인스턴스.
    /// 이를 사용해 로컬 플레이어가 Scene에 보이는지 확인한다.
    /// </summary>
    public static GameObject LocalPlayerInstance;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();

        _idleState = new PlayerIdleState();
        _runState = new PlayerRunState();

        ChangeState(_idleState);
        
        // 
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (photonView.IsMine && PhotonNetwork.IsConnected)
        {
            _playerInputActions.Enable();
            _playerInputActions.Game.Move.performed += OnMovePerformed;
            _playerInputActions.Game.Move.canceled += OnMoveCanceled;
        }
    }

    private void OnDisable()
    {
        if (photonView.IsMine && PhotonNetwork.IsConnected)
        {
            _playerInputActions.Game.Move.performed -= OnMovePerformed;
            _playerInputActions.Game.Move.canceled -= OnMoveCanceled;
            _playerInputActions.Disable();
        }
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
        void Enter(PlayerController player);
        void Execute();
        void Exit();
    }

    private class PlayerIdleState : IPlayerState
    {
        private PlayerController _player;
        public void Enter(PlayerController player)
        {
            _player = player;
            _player._animator.SetBool("isRun", false);
        }

        public void Execute()
        {
            // Idle 상태에서는 아무것도 하지 않음
        }

        public void Exit() { }
    }

    private class PlayerRunState : IPlayerState
    {
        private PlayerController _player;
        public void Enter(PlayerController player)
        {
            _player = player;
            _player._animator.SetBool("isRun", true);
        }

        public void Execute()
        {
            Vector3 movement = new Vector3(_player.inputVector.x, 0, _player.inputVector.y) * _player.speed * Time.fixedDeltaTime;
            _player._rigidbody.MovePosition(_player._rigidbody.position + movement);

            if (movement != Vector3.zero)
            {
                _player.transform.LookAt(_player.transform.position + movement);
            }
        }

        public void Exit() { }
    }
}
