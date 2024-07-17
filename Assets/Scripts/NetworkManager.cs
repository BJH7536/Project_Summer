using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
    public Player_On_Network player_on_network;
    
    void Start()
    {
        ConnectToServer("203.255.57.136", 5555);
        player_on_network = new Player_On_Network(ref client, ref stream);
    }

    // void Update()
    // {
    //     float x = Input.GetAxis("Horizontal");
    //     float y = Input.GetAxis("Vertical");
    //     
    //     if(x != 0 || y != 0)
    //         player_on_network.moveEventSend($"(x : {x}, y : {y}) \n");
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
                    Debug.Log("Received from server: " + message);

                    // 여기서 서버로부터 받은 메시지에 따른 동작을 수행할 수 있습니다.
                    //TryToMoveLocalPlayer(message);
                    //player_on_network.Move(message);
                    
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
        if (message == "hello") return;
        
        Debug.Log($"message : {message}");
        string str = message.Split(":")[1];
        Debug.Log($"str : {str}");
        // str.Substring(1, str.Length - 2).Split(",");

        string[] str_arr =  str.Split("(")[1].Split(")")[0].Split(", ");
        
        float.TryParse(str_arr[0], out var x);
        float.TryParse(str_arr[1], out var y);
        Debug.Log($"(str_arr[0], str_arr[1]) : ({str_arr[0]}, {str_arr[1]})");
        
        Debug.Log($"(x, y) : ({x}, {y})");
        
        var movingVector = new Vector2(x, y);
        player.MoveByNetworkManager(movingVector);
        Debug.LogWarning($"{movingVector}");
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

    public void moveEventSend(string keyInfo)
    {
        string message = "RPC_MOVE CALLER_" + new string(keyInfo);
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }

    public void Move(string received_data)
    {
        Debug.Log("RPC_MOVE CALLER_" + received_data);
    }
    
    public void NoticeServerThatImLeaving()
    {
        string message = "Im Out!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        _stream.Write(data, 0, data.Length);
    }
}
