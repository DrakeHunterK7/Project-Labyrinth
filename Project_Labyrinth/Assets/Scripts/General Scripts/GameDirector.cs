using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public static GameDirector instance;
    private int gameLevel = 0;
    [SerializeField] private List<String> levelNames;
    [SerializeField] private GameObject player;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        MessageManager.instance.AddObjective("THE ESCAPE: Find a way to escape the lab", Color.yellow);
    }

    public void IncreaseLevel()
    {
        gameLevel++;
    }

    public void NextLevel()
    {
        IncreaseLevel();
        SceneManager.LoadScene(levelNames[gameLevel]);
    }

 
}

