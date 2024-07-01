using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    #region Private Constants

    // PlayerPref 플레이어의 이름을 저장할 키
    private static string playerNamePrefKey = "PlayerName";
    
    #endregion

    #region Monobehaviour CallBacks
    
    private void Start()
    {
        string defaultName = string.Empty;
        TMP_InputField _inputField = GetComponent<TMP_InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 플레이어의 이름을 지정. 그리고 PlayerPrefs로 저장한다.
    /// </summary>
    /// <param name="value">플레이어의 이름</param>
    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        // 네트워크상의 플레이어 이름을 설정한다.
        PhotonNetwork.NickName = value;
        
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    #endregion
}
