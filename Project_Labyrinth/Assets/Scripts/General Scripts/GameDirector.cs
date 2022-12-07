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
    private PlayerMovement playerScript;
    public bool isLoadingNextLevel = false;
    
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
        
    }
    
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
 
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingNextLevel = false;
        if(playerScript == null)
            playerScript = player.GetComponent<PlayerMovement>();
        playerScript.fadeDirection = 1;
        playerScript.fadeTime = 0;
        MessageManager.instance.AddObjective("THE ESCAPE: Find a way to escape the lab", Color.yellow);
    }
 
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void IncreaseLevel()
    {
        gameLevel++;
    }

    public void NextLevel()
    {
        playerScript.fadeDirection = -1;
        playerScript.fadeTime = 2;
        IncreaseLevel();
        isLoadingNextLevel = true;
        InvokeRepeating("LoadLevel", 5, 0f);
    }

    void LoadLevel()
    {
        SceneManager.LoadScene(levelNames[gameLevel]);
    }

 
}

