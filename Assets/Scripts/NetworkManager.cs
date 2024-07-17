using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    //[SerializeField] private Player player;
    private Player_On_Network player;
    void Start()
    {
        ConnectToServer("203.255.57.136", 5555);
        player = new Player_On_Network(ref client, ref stream);
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        
        if(x != 0 || y != 0)
            player.moveEventSend($"(x : {x}, y : {y}) \n");
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
                    player.Move(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving data: " + e.Message);
                isConnected = false;
            }
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
}
