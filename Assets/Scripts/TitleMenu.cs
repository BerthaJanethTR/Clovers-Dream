using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    //Método para hacer que el botón de Play cambie de escena
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Método para que el boton de Exit cierre el juego
    public void QuitGame()
    {
        Application.Quit();
    }
}
