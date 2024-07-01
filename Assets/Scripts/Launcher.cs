using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region private Serizalizable Fields

    [Tooltip("방의 최대 인원 수. 방이 가득 차있는 경우, 새로운 플레이어는 들어올 수 없다. 새로운 방이 만들어져야 할 것")] [SerializeField]
    private byte maxPlayerPerRoom = 4;
    
    #endregion
    
    #region Private Fields
    
    /// <summary>
    /// 클라이언트의 버전 넘버.
    /// 유저는 이 gameVersion에 따라 분리된다.
    /// </summary>
    private string gameVersion = "1";

    /// <summary>
    /// 연결을 시도 중임을 나타내는 플래그
    /// </summary>
    private bool isConnecting;
    
    [Tooltip("사용자의 이름을 설정하고 서버에 연결할 UI 패널")] [SerializeField]
    private GameObject controlPanel;
    
    [Tooltip("연결이 진행 중임을 사용자에게 알릴 UI 레이블")] [SerializeField]
    private GameObject progressLabel;
    
    #endregion
    
    #region MonoBehaviour CallBacks

    private void Awake()
    {
        // 마스터 클라이언트에서 PhotonNetwork.LoadLevel()을 사용해서,
        // 같은 룸에 있는 모든 클라이언트가 Scene을 동기화 할 수 있도록
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // 기본 UI
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    #endregion
    
    #region Public Methods

    /// <summary>
    /// 연결 프로세스를 시작한다.
    /// 이미 연결되었다면, 무작위 방에 진입을 시도하고
    /// 아직 연결되지 않았다면, 이 앱 인스턴스를 Photon Cloud Network에 연결한다.
    /// </summary>
    public void Connect()
    {
        // UI 변경
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        // 연결을 시도 중임을 나타내는 플래그
        isConnecting = true;
        
        if (PhotonNetwork.IsConnected)
            // 여기서 무작위 방에 진입을 시도하는데, 실패할 경우 OnJoinRandomFailed() 에서 알림을 받고, 방 하나를 새로 생성한다.
            PhotonNetwork.JoinRandomRoom();
        else
        {
            // Photon Online Server에 연결하기
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    #endregion
    
    #region MonoBehaviourPunCallBacks callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Pun Basics Tutotrial/Launcher : OnConnectedToMaster() was called by PUN");
        Debug.Log($"Pun Basics Tutotrial/Launcher : Current Clients Name is {PhotonNetwork.NickName}");
        Debug.Log($"Pun Basics Tutotrial/Launcher : Current Connecting Flag is {isConnecting}");

        // 연결을 시도 중일 때만 방에 진입을 시도.
        // 위 스크립트가 동작하는 Scene이 로드될 때, 서버에 연결되어있기만 한다면 OnConnectedToMaster()가 호출되기 때문에
        // 이를 구분한다.
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // 무작위 방에 진입하는걸 실패하면, 방이 없거나 이미 가득 차있는 경우일 것. 새로 만들것이니까 상관없다.
        PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers = maxPlayerPerRoom});
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

        // 현재 로컬 플레이어가, 이 방의 첫번째 플레이어일때만 로드한다.
        // 그렇지 않을때는 Photon의 자동 Scene 동기에 맡긴다.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("we load 'Room for 1'");
            
            // Room Level을 로드
            PhotonNetwork.LoadLevel("Room for 1");
        }
    }
    
    #endregion
    
}
