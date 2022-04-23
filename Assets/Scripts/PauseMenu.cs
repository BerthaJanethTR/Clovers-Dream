using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    //Variable de estado para el Pause
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    //Método que monitoriza cuando es presionado Escape para activar el Pause o resumir el juego
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    //Método que vuelve a la normalidad el juego
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    //Método que detiene el tiempo
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    //Método para cerrar el juego
    public void QuitGame()
    {
        Application.Quit();
    }

}
