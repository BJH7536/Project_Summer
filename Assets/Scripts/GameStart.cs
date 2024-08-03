using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{

    public void GameStartButton()
    {
        Debug.Log("connecting....");
        NetworkManager.instance.ConnectToServer("203.255.57.136", 5555);
        SceneManager.LoadScene("Migration");
    }
}
