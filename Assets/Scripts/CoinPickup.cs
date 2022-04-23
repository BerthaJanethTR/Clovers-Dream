using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    //Variable para el valor de cada moneda y campo para el sonido de la moneda
    [SerializeField] int pointsForCoinPickup = 1000;
    [SerializeField] AudioClip coinPickupSFX;

    bool wasCollected = false;
    
    //Método para que la moneda active el trigger al estar en contacto con Clover y desaparezca
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !wasCollected)
        {
            AudioSource.PlayClipAtPoint(coinPickupSFX, Camera.main.transform.position);
            wasCollected = true;
            FindObjectOfType<GameSession>().AddToScore(pointsForCoinPickup);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
