using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GameDirector : MonoBehaviour
{
    public static GameDirector instance;
    public int gameLevel = 0;
    [SerializeField] private List<String> levelNames;
    [SerializeField] private GameObject player;
    private PlayerMovement playerScript;
    public bool isLoadingNextLevel = false;
    public bool isPlayerDead = false;
    public bool isDoctorDead = false;

    public int playerChasers = 0;
    public AudioSource levelMusic;
    public AudioSource chaseMusic;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name is "Main_Menu" or "Bad Ending" or "Good Ending" or "Neutral Ending")
        {
            Destroy(player);
            Destroy(this.gameObject);
        }
        else
        {
            isLoadingNextLevel = false;
            if(playerScript == null)
                playerScript = player.GetComponent<PlayerMovement>();
            playerScript.fadeDirection = 1;
            playerScript.fadeTime = 0;
            MessageManager.instance.AddObjective("THE ESCAPE: Find a way to escape the lab", Color.yellow);
            levelMusic = GameObject.FindWithTag("LevelMusic").GetComponent<AudioSource>();
        }
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
        levelMusic.Stop();
        playerScript.statusText.text = "";
        playerScript.smelly = false;
        playerScript.fadeDirection = -1;
        playerScript.fadeTime = 2;
        IncreaseLevel();
        isLoadingNextLevel = true;
        InvokeRepeating("LoadLevel", 5, 0f);
    }

    void LoadLevel()
    {
        if (gameLevel > 2)
        {
            if (!isDoctorDead)
            {
                SceneManager.LoadScene("Neutral Ending");
            }
            else
            {
                SceneManager.LoadScene("Good Ending");
            }
        }
        else
        {
            SceneManager.LoadScene(levelNames[gameLevel]);
        }
    }

    public void AddChaser()
    {
        playerChasers++;
        HandleChaseMusic();
    }

    public void RemoveChaser()
    {
        playerChasers--;
        HandleChaseMusic();
    }


    void HandleChaseMusic()
    {
        if (isLoadingNextLevel) return;
        
        if (playerChasers == 0)
        {
            FadeOut(chaseMusic);
            FadeIn(levelMusic, 0.7f);
        }
        else
        {
            FadeOut(levelMusic);
            FadeIn(chaseMusic, 0.7f);
        }
    }
    
    
    public void FadeOut(AudioSource music)
    {
        float startVol = music.volume;
        while (music.volume > 0)
        {
            music.volume -= startVol * Time.deltaTime / 0.5f;
        }
    }

    public void FadeIn(AudioSource music, float targetVol)
    {
        while (music.volume < targetVol)
        {
            music.volume += targetVol * Time.deltaTime / 0.5f;
        }
    }
}

