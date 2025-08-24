using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public Slider healthBar;

    //Quiero crear un sistema de vida para enemigos, que tenga un valor alto para poder probar DOT de manera mas eficaz
    //Necesito un valor de vida
    public int health = 4500;
    public int maxHealth = 4500;

    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = health;
    }
}
