using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    //Variable que equivale al delay para cambiar entre escenas y campo para el audio de fin de nivel
    [SerializeField] float levelLoadDelay = 1f;
    [SerializeField] AudioClip FinishSFX;

    //Trigger para activar el sonido despues de realizar una subrutina en este caso esperar un delay
    void OnTriggerEnter2D(Collider2D other)
    {
        AudioSource.PlayClipAtPoint(FinishSFX, Camera.main.transform.position);
        StartCoroutine(LoadNextLevel());
    }

    //Aplicacion del delay para el cambio de escena
    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(levelLoadDelay);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
