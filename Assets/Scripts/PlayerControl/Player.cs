using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private float positionSendInterval = 0.5f;

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

        StartCoroutine(SendPositionRoutine());
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
            yield return new WaitForSeconds(positionSendInterval);
            SendPositionToServer();
        }
    }

    private void SendPositionToServer()
    {
        Vector3 position = transform.position;
        string positionMessage = $"Position:{position.x},{position.y},{position.z}\n";
        _networkManager.player_on_network.SendMessage(positionMessage);
        Debug.Log($"Sent position to server: {positionMessage}");
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        ChangeState(inputVector != Vector2.zero ? _runState : _idleState);
        SendPositionToServer();
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
