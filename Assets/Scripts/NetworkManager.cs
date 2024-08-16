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
    private Thread receiveThread;
    private bool isConnected = false;

    [SerializeField] private Player player;
    [SerializeField] private PlayerB PlayerB;
    public Player_On_Network player_on_network;

    private string clientId;

    void Start()
    {
        clientId = Guid.NewGuid().ToString();  // 고유한 클라이언트 ID 생성
        ConnectToServer("203.255.57.136", 5555);
        player_on_network = new Player_On_Network(ref client, ref stream, clientId);  // ID 전달
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    void ConnectToServer(string serverAddress, int port)
    {
        try
        {
            client = new TcpClient(serverAddress, port);
            stream = client.GetStream();
            isConnected = true;

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect to server: " + e.Message);
        }
    }

    void DisconnectFromServer()
    {
        if (isConnected)
        {
            player_on_network.NoticeServerThatImLeaving();
            receiveThread.Abort();
            stream.Close();
            client.Close();
            isConnected = false;
        }
    }

    void ReceiveData()
    {
        while (isConnected)
        {
            try
            {
                byte[] data = new byte[1024];
                int bytesRead = stream.Read(data, 0, data.Length);

                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(data, 0, bytesRead);
                    //Debug.Log("Received from server: " + message);

                    // 서버로부터 받은 메시지를 메인 스레드에서 처리
                    ProcessMessageAsync(message).Forget();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error receiving data: " + e.Message);
                isConnected = false;
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
        if (!message.StartsWith("Position:")) return;

        // 클라이언트 ID 추출
        int colonIndex = message.IndexOf(':');
        if (colonIndex == -1) return;

        string receivedClientId = message.Substring(9, colonIndex - 9);
        if (receivedClientId == clientId) return;  // 자신이 보낸 메시지는 무시

        // "Position:" 이후의 문자열에서 첫 번째로 등장하는 괄호 안의 값을 추출
        int startIndex = message.IndexOf('(');
        int endIndex = message.IndexOf(')');

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            // 괄호 안의 내용을 추출
            string positionString = message.Substring(startIndex + 1, endIndex - startIndex - 1);
            string[] str_arr = positionString.Split(',');

            // 좌표 값을 파싱
            if (str_arr.Length == 3)
            {
                float.TryParse(str_arr[0], out var x);
                float.TryParse(str_arr[1], out var y);
                float.TryParse(str_arr[2], out var z);

                // 이제 파싱된 좌표 값들을 사용할 수 있습니다.
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

