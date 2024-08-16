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
        
        Instantiate(NetworkManager.instance.GetPlayer());
        // GameObject foundPlayerObject = GameObject.Find("Player");
        // if (foundPlayerObject != null)
        // {
        //     Player newPlayer = foundPlayerObject.GetComponent<Player>();
        //     if (newPlayer != null)
        //     {
        //         // NetworkManager 싱글톤 인스턴스를 통해 player 할당
        //         NetworkManager.Instance.SetPlayer(newPlayer);
        //     }
        //     else
        //     {
        //         Debug.LogError("Player component not found on the GameObject!");
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Player GameObject not found!");
        // }
    }
}
