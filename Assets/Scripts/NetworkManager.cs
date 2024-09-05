using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;

    [SerializeField] private Player player;
    [SerializeField] private PlayerB PlayerB;
    public Player_On_Network player_on_network;

    private string clientId;

    public static NetworkManager instance;

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<NetworkManager>();
                    singletonObject.name = typeof(NetworkManager).ToString() + " (Singleton)";

                    DontDestroyOnLoad(singletonObject); // 씬이 바뀌어도 파괴되지 않도록 설정
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        clientId = Guid.NewGuid().ToString();  // 고유한 클라이언트 ID 생성
        ConnectToServer("117.16.154.225", 8000);
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    public async void ConnectToServer(string serverAddress, int port)
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverAddress, port);
            stream = client.GetStream();
            isConnected = true;

            player_on_network = new Player_On_Network(ref client, ref stream, clientId);  // ID 전달

            // 비동기 수신 시작
            ReceiveDataAsync().Forget();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect to server: " + e.Message);
        }
    }

    public void DisconnectFromServer()
    {
        if (isConnected)
        {
            player_on_network.NoticeServerThatImLeaving();
            stream.Close();
            client.Close();
            isConnected = false;
        }
    }

    async UniTaskVoid ReceiveDataAsync()
    {
        byte[] data = new byte[1024];

        while (isConnected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(data, 0, data.Length);

                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(data, 0, bytesRead);
                    ProcessMessageAsync(message).Forget();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error receiving data: " + e.Message);
                isConnected = false;
                break;
            }
        }
    }

    async UniTaskVoid ProcessMessageAsync(string message)
    {
        await UniTask.SwitchToMainThread();
        TryToMoveLocalPlayer(message);
    }

    void TryToMoveLocalPlayer(string message)
    {
        Debug.Log(message);
        if (!message.StartsWith("Position:")) return;

        string[] splitMessage = message.Split(':');
        if (splitMessage.Length < 2) return;

        string receivedClientId = splitMessage[1].Substring(0, splitMessage[1].IndexOf('('));
        if (receivedClientId == clientId) return;  // 자신이 보낸 메시지는 무시

        int startIndex = message.IndexOf('(');
        int endIndex = message.IndexOf(')');

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string positionString = message.Substring(startIndex + 1, endIndex - startIndex - 1);
            string[] str_arr = positionString.Split(',');

            if (str_arr.Length == 3)
            {
                float.TryParse(str_arr[0], out var x);
                float.TryParse(str_arr[1], out var y);
                float.TryParse(str_arr[2], out var z);

                PlayerB.MoveByNetworkManager(x, y, z);
            }
        }
        else
        {
            Debug.LogWarning("Invalid position format in message.");
        }
    }
}

public class Player_On_Network
{
    private TcpClient _client;
    private NetworkStream _stream;
    private string clientId;

    public Player_On_Network(ref TcpClient client, ref NetworkStream stream, string clientId)
    {
        this._client = client;
        this._stream = stream;
        this.clientId = clientId;
    }

    public void SendMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }

    public void NoticeServerThatImLeaving()
    {
        string message = $"{clientId}:Im Out!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }

    public string GetClientId()
    {
        return clientId;
    }
}
