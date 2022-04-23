using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSession : MonoBehaviour
{
    //Variables para la interfaz de muertes y score
    [SerializeField] int playerDeathCount = 0;
    [SerializeField] int score = 0;

    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;

    //Método para conservar datos de la interfaz entre escenas
    void Awake()
    {
        int numGameSessions = FindObjectsOfType<GameSession>().Length;
        if (numGameSessions > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    //Conversión de datos int a string
    void Start()
    {
        livesText.text = playerDeathCount.ToString();
        scoreText.text = score.ToString();
    }

    //Método que procesa el estado de muerte y resetea el score
    public void ProcessPlayerDeath()
    {
        TakeLife();
        score = 0;
        scoreText.text = "0";
    }

    //Método que hace la suma del score
    public void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        scoreText.text = score.ToString();
    }

    //Método que hace el conteo de muertes y coloca a Clover en la posicion inicial
    void TakeLife()
    {
        playerDeathCount++;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        livesText.text = playerDeathCount.ToString();
    }
}
