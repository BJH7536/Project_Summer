using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private float speed = 15f;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float interactDistance = 3f;

    private PlayerInputActions _playerInputActions;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Holdable heldObject;

    private IPlayerState _currentState;
    private PlayerIdleState _idleState;
    private PlayerRunState _runState;

    private int interactableLayer;

    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int Holding = Animator.StringToHash("Holding");

    [SerializeField] private NetworkManager _networkManager;
    private float positionSendInterval = .5f;

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
        _playerInputActions.Game.Move.performed += OnMovePerformed;
        _playerInputActions.Game.Move.canceled += OnMoveCanceled;
        _playerInputActions.Game.Interact.performed += OnInteractPerformed;

        StartCoroutine(SendPositionRoutine()); // 코루틴 시작
    }

    private void OnDisable()
    {
        _playerInputActions.Game.Move.performed -= OnMovePerformed;
        _playerInputActions.Game.Move.canceled -= OnMoveCanceled;
        _playerInputActions.Game.Interact.performed -= OnInteractPerformed;
        _playerInputActions.Disable();
    }

    private IEnumerator SendPositionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(positionSendInterval); // 주기적으로 대기
            SendPositionToServer(); // 위치 전송
        }
    }

    private void SendPositionToServer()
    {
        Vector3 position = transform.position;
        string positionMessage = $"Position:{_networkManager.player_on_network.GetClientId()}({position.x},{position.y},{position.z})\n";
        _networkManager.player_on_network.SendMessage(positionMessage);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        ChangeState(inputVector != Vector2.zero ? _runState : _idleState);
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

    private void SendInteractionToServer(Holdable holdable)
    {
        Topping ToppingInfo = holdable.GetComponent<Topping>();
        if (ToppingInfo != null)
        {
            Debug.Log(ToppingInfo.GetTopping().ToString());
            string ToppingnMessage = $"Topping:{ToppingInfo.GetTopping()}\n";
            _networkManager.player_on_network.SendMessage(ToppingnMessage);
        }else
            Debug.Log("없음 ");
    }

    public void HoldTopping(Holdable topping)
    {
        heldObject = topping;
        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.GetComponent<Rigidbody>().isKinematic = true;
        heldObject.GetComponent<Collider>().isTrigger = true;
        _animator.SetBool(Holding, true);
        SendInteractionToServer(topping);
    }

    public void ReleaseTopping(Holdable topping)
    {
        topping.transform.SetParent(null);
        topping.GetComponent<Rigidbody>().isKinematic = false;
        topping.GetComponent<Collider>().isTrigger = false;
        _animator.SetBool(Holding, false);

        // 서버로 토핑을 놓았다는 메시지 전송
        SendToppingReleaseToServer(topping);
        heldObject = null;
    }

    private void SendToppingReleaseToServer(Holdable topping)
    {
        Topping ToppingInfo = topping.GetComponent<Topping>();
        if (ToppingInfo != null)
        {
            string ToppingnMessage = $"ToppingReleased:{ToppingInfo.GetTopping()}\n";
            _networkManager.player_on_network.SendMessage(ToppingnMessage);
        }
        else
        {
            Debug.Log("토핑 정보가 없습니다.");
        }
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

        public void Execute() { }

        public void Exit() { }
    }

    private class PlayerRunState : IPlayerState
    {
        private Player _player;
        public void Enter(Player player)
        {
            _player = player;
            _player._animator.SetBool(IsRun, true);
        }

        public void Execute()
        {
            Vector3 movement = new Vector3(_player.inputVector.x, 0, _player.inputVector.y).normalized * _player.speed;

            if (movement != Vector3.zero)
            {
                _player._rigidbody.velocity = movement;
                _player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, Quaternion.LookRotation(movement), 10 * Time.deltaTime);
            }
        }

        public void Exit() { }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastOrigin.position, interactDistance);
    }
}
