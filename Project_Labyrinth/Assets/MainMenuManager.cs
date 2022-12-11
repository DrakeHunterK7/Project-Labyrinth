using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainMenuManager : MonoBehaviour
{

    public GameObject light1;
    public GameObject light2;
    public GameObject controlMenu;

    public String firstLevelName;

    public TextMeshProUGUI line1;
    public TextMeshProUGUI line2;
    public TextMeshProUGUI line3;
    public TextMeshProUGUI line4;
    public TextMeshProUGUI line5;
    
    private bool line1Fade;
    private bool line2Fade;
    private bool line3Fade;
    private bool line4Fade;
    private bool line5Fade;
    
    private float line1FadeTime = 2f;
    private float line2FadeTime = 2f;
    private float line3FadeTime = 2f;
    private float line4FadeTime = 2f;
    private float line5FadeTime = 2f;
    
    private float bgFadeTime = 3f;

    private float fadeDirection = 1;

    private int lineCount = 0;

    public Button skipButton;

    public Canvas mainMenuCanvas;
    public Canvas storyCanvas;
    public UnityEngine.UI.Image storyBackground;

    private bool gameStarted = false;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InvokeRepeating("FlickerLights", 0f, 0.1f);
    }

    void Update()
    {
        if (gameStarted)
        {
            bgFadeTime -= Time.deltaTime;
            storyBackground.color = Color.Lerp(Color.clear, Color.black, 1 - (bgFadeTime / 3f));
        }
        if (line1Fade)
        {
            line1FadeTime = Mathf.Clamp(line1FadeTime -= fadeDirection*Time.deltaTime, 0, 2);
            line1.color = Color.Lerp(Color.clear, Color.white, 1 - (line1FadeTime / 2f));
        }
        if (line2Fade)
        {
            line2FadeTime = Mathf.Clamp(line2FadeTime -= fadeDirection*Time.deltaTime, 0, 2);
            line2.color = Color.Lerp(Color.clear, Color.white, 1 - (line2FadeTime / 2f));
        }
        if (line3Fade)
        {
            line3FadeTime = Mathf.Clamp(line3FadeTime -= fadeDirection*Time.deltaTime, 0, 2);
            line3.color = Color.Lerp(Color.clear, Color.white, 1 - (line3FadeTime / 2f));
        }
        if (line4Fade)
        {
            line4FadeTime = Mathf.Clamp(line4FadeTime -= fadeDirection*Time.deltaTime, 0, 2);
            line4.color = Color.Lerp(Color.clear, Color.white, 1 - (line4FadeTime / 2f));
        }
        if (line5Fade)
        {
            line5FadeTime = Mathf.Clamp(line5FadeTime -= fadeDirection*Time.deltaTime, 0, 2);
            Mathf.Clamp(line5FadeTime, 0, 2);
            line5.color = Color.Lerp(Color.clear, Color.red, 1 - (line5FadeTime / 2f));
        }
    }

    
    void FlickerLights()
    {
        if (Random.Range(1, 10f) < 2f)
        {
            if (light1.activeSelf)
            {
                light1.SetActive(false);
            }
            else
            {
                light1.SetActive(true);
            }
        }
            
        
        if (Random.Range(1, 10f) < 2f)
        {
            if (light2.activeSelf)
            {
                light2.SetActive(false);
            }
            else
            {
                light2.SetActive(true);
            }
        }
    }

    public void BeginGame()
    {
        mainMenuCanvas.enabled = false;
        storyCanvas.gameObject.SetActive(true);
        gameStarted = true;
        StartCoroutine(StartShowingLines());
    }

    IEnumerator StartShowingLines()
    {
        yield return new WaitForSeconds(7f);
        line1Fade = true;
        skipButton.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(5f);
        line2Fade = true;
        
        yield return new WaitForSeconds(5f);
        line3Fade = true;
        
        yield return new WaitForSeconds(5f);
        line4Fade = true;
        
        yield return new WaitForSeconds(5f);
        line5Fade = true;
        
        yield return new WaitForSeconds(5f);
        fadeDirection = -1;
        skipButton.gameObject.SetActive(false);


        yield return new WaitForSeconds(5f);
        StartGame();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    public void showControls()
    {
        controlMenu.SetActive(true);
    }

    public void ExitControls()
    {
        controlMenu.SetActive(false);
    }



}
