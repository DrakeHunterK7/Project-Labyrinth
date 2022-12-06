using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
    [SerializeField]
    GameObject pauseMenuPanel;

    [SerializeField]
    GameObject HUDCanvas;

    public static bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0.0f;

        pauseMenuPanel.SetActive(true);
        HUDCanvas.SetActive(false);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1.0f;
        pauseMenuPanel.SetActive(false);
        HUDCanvas.SetActive(true);
    }

    public void ReturnToMenu()
    {
        isPaused = false;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Main_Menu");
    }

    public void ControlsMenu()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
