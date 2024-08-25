using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    
    [SerializeField] private Conveyor _conveyor;

    [SerializeField] private Define.PizzaType _dough;

    [SerializeField] private GameObject playerGameObject;
    
    [SerializeField] private Player ThisPlayer;
    [SerializeField] private Player OtherPlayer;
    
    private void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        SetDough();
        SpawnPlayers();
    }
    
    public void SetDough()
    {
        _dough = _conveyor.MakeRandomDough();
        
        Debug.Log($"Dough made! {_dough.ToString()} Pizza Ordered!");
    }

    public void SpawnPlayers()
    {
        ThisPlayer = Instantiate(playerGameObject, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        ThisPlayer.IsMine = true;
        ThisPlayer.networkManager = _networkManager;
        
        OtherPlayer = Instantiate(playerGameObject, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        OtherPlayer.IsMine = false;
        OtherPlayer.networkManager = _networkManager;

        _networkManager.ThisPlayer = ThisPlayer;
        _networkManager.OtherPlayer = OtherPlayer;

        CinemachineVirtualCameraBase camera =
            GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCameraBase>();
        camera.Follow = ThisPlayer.transform;
        camera.LookAt = ThisPlayer.transform.Find("CameraLook");
    }
    
}
