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
    public bool isDead = false;
    [SerializeField] private BloodDOT bloodDOT;

    //Tipos de daño
    public enum damageType { bleed, poision, generic }

    //Damage Multipliers
    public float bleedMultiplier = 1.5f; // Daño de sangrado aumenta en un 50%
    public float poisionMultiplier = 1.2f; // Daño de veneno
    public float genericMultiplier = 1.0f; // No hay modificador para daño genérico
    
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

    public void TakeDamage(int damage, damageType type)
    {
        if (isDead) return;
        
        bloodDOT = FindAnyObjectByType<BloodDOT>();
        int finalDamage = damage;

        //Aplicar modificadores de daño según el tipo
        switch (type)
        {
            case damageType.bleed:
                finalDamage = Mathf.RoundToInt(finalDamage * bleedMultiplier); // Daño de sangrado aumenta en un 50%
                break;
            case damageType.poision:
                finalDamage = Mathf.RoundToInt(finalDamage * poisionMultiplier); // Daño de veneno aumenta en un 20%
                break;
            case damageType.generic:
                // No hay modificador para daño genérico
                break;
        }
            
        if (bloodDOT && bloodDOT.isMarked)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * bloodDOT.damageAmplification);
            Debug.Log($"Marked! Base damage: {damage}, Type modified: {finalDamage}");
        }
        
        health -= finalDamage;
        Debug.Log($"Health remaining: {health}");

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("Enemy Dead");
        gameObject.SetActive(false);
        // Aquí puedes agregar lógica adicional para cuando el enemigo muere, como reproducir una animación, desactivar el objeto, etc.
    }
}
