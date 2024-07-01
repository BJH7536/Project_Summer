using System;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    public static GameManager Instance;

    [Tooltip("플레이어를 나타내는 데 사용될 프리팹")] public GameObject playerPrefab;

    public SmoothCameraFollow SmoothCameraFollow;

    public CinemachineVirtualCamera CinemachineVirtualCamera;
    
    #endregion
    
    
    #region MonoBehaviour CallBacks


    private void Start()
    {
        Instance = this;
        
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        }
        else
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);

                // Photon의 방 안에서, 로컬 플레이어의 캐릭터를 스폰한다. PhotonNetwork.Instantiate로 동기화된다.
                Transform instantiatedPlayer = PhotonNetwork
                    .Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0).transform;
                SmoothCameraFollow.SetTarget(instantiatedPlayer.GetComponent<PlayerController>());
                SmoothCameraFollow.target = instantiatedPlayer;
                CinemachineVirtualCamera.Follow = instantiatedPlayer;
                CinemachineVirtualCamera.LookAt = instantiatedPlayer;
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManager.GetActiveScene().name);
            }
        }
    }

    #endregion
    
    
    #region Photon CallBacks

    /// <summary>
    /// 로컬 플레이어가 방을 나갈때 호출된다. 로비 Scene으로 돌아간다.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
            
            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
            
            LoadArena();
        }
    }
    
    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    #endregion

    #region Private Methods

    /// <summary>
    /// 현재 클라이언트가 마스터 클라이언트인지를 검사하고
    /// 방 인원수에 맞는 Scene으로 넘어가는 기능
    /// </summary>
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            return;
        }
        
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    #endregion
}
