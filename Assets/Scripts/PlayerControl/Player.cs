using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private float speed = 15f;
    [SerializeField] private Transform holdPosition; // 플레이어 앞에 오브젝트를 들 위치
    [SerializeField] private Transform raycastOrigin; // 레이캐스트 시작 위치
    [SerializeField] private float interactDistance = 3f; // 상호작용 거리 (기본값을 5로 증가)
    
    private Vector3 destination;
    private PlayerInputActions _playerInputActions;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Holdable heldObject; // 들고 있는 오브젝트
    private IPlayerState _currentState;
    private PlayerIdleState _idleState;
    private PlayerRunState _runState;
    private string _currentStateName;
    private int interactableLayer;
    
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int Holding = Animator.StringToHash("Holding");

    [SerializeField] public NetworkManager networkManager;

    // isMine 변수 추가
    [SerializeField] private bool isMine;

    // Getter와 Setter 메서드 추가
    public bool IsMine
    {
        get => isMine;
        set => isMine = value;
    }
    
    private void Awake()
    {
        interactableLayer = LayerMask.GetMask("Interactable");
        
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

        // 입력 이벤트 연결 (isMine 체크 추가)
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

    public void MoveByNetworkManager(Vector2 vector2)
    {
        inputVector = vector2;
        if (inputVector != Vector2.zero)
        {
            destination = new Vector3(vector2.x, 0, vector2.y);
            ChangeState(_runState);
        }
        else
        {
            ChangeState(_idleState);
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!isMine) return; // isMine이 false이면 입력 무시
        // Move 관련 로직
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!isMine) return; // isMine이 false이면 입력 무시
        networkManager.networkClient.TrySendMoveEvent($"{_currentStateName}:{context.ReadValue<Vector2>()}\n", true);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isMine) return; // isMine이 false이면 입력 무시

        if (heldObject == null)
        {
            TryInteractSomething();
        }
        else
        {
            heldObject.Release(this);
        }
    }

    private void TryInteractSomething()
    {
        Collider[] colliders = new Collider[10];
        Physics.OverlapSphereNonAlloc(raycastOrigin.position, interactDistance, colliders, interactableLayer);

        foreach (var col in colliders)
        {
            if (col == null) continue;
            
            var holdable = col.GetComponent<Holdable>();
            if (holdable != null)
            {
                holdable.Hold(this);
                break;
            }
        }
    }

    public void HoldTopping(Holdable topping)
    {
        heldObject = topping;
        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.GetComponent<Rigidbody>().isKinematic = true;
        heldObject.GetComponent<Collider>().isTrigger = true;
        _animator.SetBool(Holding, true);
    }

    public void ReleaseTopping(Holdable topping)
    {
        topping.transform.SetParent(null);
        topping.GetComponent<Rigidbody>().isKinematic = false;
        topping.GetComponent<Collider>().isTrigger = false;
        _animator.SetBool(Holding, false);
        heldObject = null;
    }

    private void FixedUpdate()
    {
        _currentState?.Execute();
        
        if (!isMine) return;
        // 현재 입력 상태를 계속 확인 
        Vector2 currentInputVector = _playerInputActions.Game.Move.ReadValue<Vector2>();
        networkManager.networkClient.TrySendMoveEvent($"{_currentStateName}:{currentInputVector}\n");
    }
    
    private void ChangeState(IPlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this);

        #region About State Name
        
        _currentStateName = newState.GetType().Name;
        if (_currentStateName.StartsWith("Player") && _currentStateName.EndsWith("State"))
        {
            _currentStateName = _currentStateName.Substring("Player".Length);
            _currentStateName = _currentStateName.Substring(0, _currentStateName.Length - "State".Length);
        }
        #endregion
    }

    // Gizmos를 사용하여 상호작용 범위를 시각적으로 표시합니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastOrigin.position, interactDistance);
    }
    
    #region State
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
            _player._animator.SetBool(IsRun, false);
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
        private Vector3 _targetPosition;
        
        public void Enter(Player player)
        {
            _player = player;
            _player._animator.SetBool(IsRun, true);

            _targetPosition = _player.destination;
        }

        public void Execute()
        {
            Vector3 vec = (_targetPosition - _player.transform.position);
            Vector3 newVec = new Vector3(vec.x, 0, vec.y);
            Vector3 movement = newVec.normalized * _player.speed;

            if (movement != Vector3.zero)
            {
                _player._rigidbody.MovePosition(Vector3.Lerp(_player.transform.position, _targetPosition, _player.networkManager.networkClient.inputDetectionInterval / Time.fixedDeltaTime));
                _player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, Quaternion.LookRotation(movement), 10 * Time.deltaTime);
            }
        }

        public void Exit() { }
    }
    
    #endregion
}
