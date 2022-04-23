using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBC : MonoBehaviour
{
    //Método para cerrar el juego al presionar la barra espaciadora
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Application.Quit();
        }
    }
}
