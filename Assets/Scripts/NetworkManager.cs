using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    [SerializeField] public Player ThisPlayer;
    [SerializeField] public Player OtherPlayer;
    public Client networkClient;
    
    // 첫 번째 패킷인지 여부를 확인하는 플래그
    private bool isFirstPacket = true;
    
    void Start()
    {
        ConnectToServer("183.103.222.240", 8000);
        networkClient = new Client(ref client, ref stream);
    }

    // void Update()
    // {
    //     float x = Input.GetAxis("Horizontal");
    //     float y = Input.GetAxis("Vertical");
    //     
    //     if(x != 0 || y != 0)
    //         networkClient.moveEventSend($"(x : {x}, y : {y}) \n");
    // }

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
            networkClient.NoticeServerThatImLeaving();
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
                    Debug.Log($"Packet received! Message context is : {message}");

                    // 여기서 서버로부터 받은 메시지에 따른 동작을 수행할 수 있음.
                    
                    // 첫 번째 패킷에 대한 특별한 처리
                    if (isFirstPacket)
                    {
                        SetClientIdentifierByServer(message);
                        isFirstPacket = false; // 이후로는 기본 처리로 전환
                    }
                    else
                    {
                        // 서버로부터 받은 메시지를 메인 스레드에서 처리
                        ProcessMessageAsync(message).Forget();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error receiving data: " + e.Message);
                isConnected = false;
            }
        }
    }

    // 서버와 연결한 직후에 받는 패킷은
    // 서버에서 클라이언트에게 할당한 식별자가 날라온다.
    void SetClientIdentifierByServer(string message)
    {
        networkClient.MyIdentifier = message[0];

        Debug.Log($"My Identifier is {networkClient.MyIdentifier}");
    }
    
    async UniTaskVoid ProcessMessageAsync(string message)
    {
        await UniTask.SwitchToMainThread();
        TryToMoveLocalPlayer(message);
    }
    
    void TryToMoveLocalPlayer(string message)
    {
        if (message == "hello") return;
        
        float startTime = Time.time;
        Debug.Log($"Parse Start at {startTime} =============");
        Debug.Log($"message : {message}");
        // string str = message.Split(":")[1];
        // Debug.Log($"str : {str}");
        // // str.Substring(1, str.Length - 2).Split(",");
        //
        // string[] str_arr =  str.Split("(")[1].Split(")")[0].Split(", ");
        //
        // float.TryParse(str_arr[0], out var x);
        // float.TryParse(str_arr[1], out var y);
        // Debug.Log($"(str_arr[0], str_arr[1]) : ({str_arr[0]}, {str_arr[1]})");

        // 모든 플레이어에 대한 정보를 받는다 
        // playerDataArray e.g.) 0:RUN:(0.00,1.00)|1:RUN:(1.00,1.00)
        string[] playerDataArray = message.Split("|");
        foreach (string playerData in playerDataArray)
        {
            // 그 중 나의 정보를 선별해, 이 정보만을 내 클라이언트에 반영한다.
            // 한 플레이어의 정보 / e.g.) 0:RUN:(0.00,0.00)
            string[] data = playerData.Split(":");
            char id = data[0][0];
            if (id == networkClient.MyIdentifier)       // 내 캐릭터에 대한 정보일 때
            {
                // 조이스틱 좌표 정보
                data[2] = data[2].Trim('(', ')');
                string[] vectorString = data[2].Split(",");
                float.TryParse(vectorString[0], out var x);
                float.TryParse(vectorString[1], out var y);
                
                Debug.Log($"(x, y) : ({x}, {y})");
                
                // 내 클라이언트 캐릭터에 반영
                var movingVector = new Vector2(x, y);
                ThisPlayer.MoveByNetworkManager(movingVector);
            }
            else                                         // 상대 캐릭터에 대한 정보일 때
            {
                // 조이스틱 좌표 정보
                data[2] = data[2].Trim('(', ')');
                string[] vectorString = data[2].Split(",");
                float.TryParse(vectorString[0], out var x);
                float.TryParse(vectorString[1], out var y);
                
                Debug.Log($"(x, y) : ({x}, {y})");
                
                // 내 클라이언트 캐릭터에 반영
                var movingVector = new Vector2(x, y);
                OtherPlayer.MoveByNetworkManager(movingVector);
            }
        }
        
        Debug.Log($"Parse End for {Time.time - startTime :F3} =============");
    }
}

public class Client
{
    private TcpClient _client;
    private NetworkStream _stream;
    
    // 서버에서 클라이언트를 식별하기 위한 식별자
    private char myIdentifier;

    // 입력 샘플링 시간
    public float inputDetectionInterval = 0.001f; // 1ms 간격
    
    // 가장 마지막으로 패킷을 보낸 시간
    private float lastPacketSentTime = 0f;
    
    public Client(ref TcpClient client, ref NetworkStream stream)
    {
        this._client = client;
        this._stream = stream;
        
        Time.fixedDeltaTime = inputDetectionInterval;
    }
    
    public char MyIdentifier
    {
        get => myIdentifier;
        set => myIdentifier = value;
    }

    public void TrySendMoveEvent(string keyInfo, bool forceSend = false)
    {
        float currentTime = Time.time;
    
        // forceSend가 true이거나 지정된 간격이 지난 경우에만 패킷을 보냄
        if (forceSend || (currentTime - lastPacketSentTime >= inputDetectionInterval))
        {
            string message = myIdentifier + ":" + keyInfo;
            byte[] data = Encoding.ASCII.GetBytes(message);
            _stream.Write(data, 0, data.Length);

            float timeSinceLastPacket = currentTime - lastPacketSentTime;
            lastPacketSentTime = currentTime;

            // 현재 시간을 로그에 출력
            Debug.Log($"Packet sent! Message context is : {message}");
            Debug.Log($"Packet sent at {currentTime:F3} seconds, {timeSinceLastPacket:F3} seconds since last packet");
        }
    }
    
    // public void moveEventSend(string keyInfo)
    // {
    //     string message = "RPC_MOVE CALLER_" + new string(keyInfo);
    //     byte[] data = Encoding.ASCII.GetBytes(message);
    //     _stream.Write(data, 0, data.Length);
    // }
    //
    // public void Move(string received_data)
    // {
    //     Debug.Log("RPC_MOVE CALLER_" + received_data);
    // }
    
    public void NoticeServerThatImLeaving()
    {
        string message = "Im Out!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }
}