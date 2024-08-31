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
        ConnectToServer("127.0.0.1", 8080);
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

            player_on_network = new Player_On_Network(ref client, ref stream);  // 클라이언트 ID 없이 초기화

            // 비동기 수신 시작
            ReceiveDataAsync().Forget();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect to server: " + e.Message);
            isConnected = false;  // 연결 실패 시 isConnected를 false로 설정
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
                    Debug.Log(message);
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

        if (message.StartsWith("Position:"))
        {
            //TryToMoveLocalPlayer(message);
        }
        else if (message.StartsWith("Topping:"))
        {
            ProcessToppingMessage(message, isReleased: false);
        }
        else if (message.StartsWith("ToppingReleased:"))
        {
            ProcessToppingMessage(message, isReleased: true);
        }
    }

    void ProcessToppingMessage(string message, bool isReleased)
    {
        string[] splitMessage = message.Split(':');
        if (splitMessage.Length < 2) return;

        string toppingType = splitMessage[1].Trim(); // Topping type (e.g., "Cheese", "Pepperoni")

        if (!isReleased)
        {
            // 상대방이 토핑을 잡는 것을 반영
            PlayerB.HoldTopping(toppingType);
        }
        else
        {
            // 상대방이 토핑을 놓는 것을 반영
            Holdable toppingToRelease = PlayerB.GetToppingByType(toppingType);
            if (toppingToRelease != null)
            {
                PlayerB.ReleaseTopping(toppingToRelease);
            }
            else
            {
                Debug.LogWarning($"No active topping found for type: {toppingType}");
            }
        }
    }

    void TryToMoveLocalPlayer(string message)
    {
        Debug.Log(message);
        if (!message.StartsWith("Position:")) return;

        string[] splitMessage = message.Split(':');
        if (splitMessage.Length < 2) return;

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

    public Player_On_Network(ref TcpClient client, ref NetworkStream stream)
    {
        this._client = client;
        this._stream = stream;
    }

    public void SendMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }

    public void NoticeServerThatImLeaving()
    {
        string message = "Im Out!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }
}
