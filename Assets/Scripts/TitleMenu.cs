using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    //M�todo para hacer que el bot�n de Play cambie de escena
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //M�todo para que el boton de Exit cierre el juego
    public void QuitGame()
    {
        Application.Quit();
    }
}
