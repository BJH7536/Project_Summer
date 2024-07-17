using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Conveyor _conveyor;

    [SerializeField] private Define.PizzaType _dough;

    private void Start()
    {
        StartGame();
    }

    [Button]
    void StartGame()
    {
        _dough = _conveyor.MakeRandomDough();
        
        Debug.Log($"Dough made! {_dough.ToString()} Pizza Ordered!");
    }
    
    
    
}
