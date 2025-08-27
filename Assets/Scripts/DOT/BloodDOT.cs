using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDOT : MonoBehaviour
{
    [Header("References")]
    public Health health;

    [Header("Keybinds")]


    public KeyCode ultAbilityKey = KeyCode.R;

    [Header("Q — Blood Burst | Parámetros")]
    public int firstAbilityDamage = 50;
    public float qCooldown = 5f;
    public int numberOfTicks = 5;
    public float timeBetweenTicks = 0.2f;
    public float timeBeforeTickApplies = 0.5f;
    public bool isFirstAbilityUsed = false;
    public float tickDamage = 0f;
    public KeyCode firstAbilityKey = KeyCode.Q;
    public bool canUseQ = true;

    [Header("E — Mark | Parámetros")]
    public float damageAmplification = 0.5f; // 50% más de daño 
    public float eCooldown = 10f;
    public float markDuration = 5f;
    public bool isMarked = false;
    public bool canUseE = true;
    public KeyCode secondAbilityKey = KeyCode.E;
    

    [Header("R — Ultimate | Parámetros (por definir)")]
    public bool isUltActive = false;
    // Agrega aquí los parámetros de tu ulti cuando la diseñes.
    // public float ultDamage = 200f;
    // public float ultDuration = 2f;
    // public bool  isUltActive = false;


    /* Resumen de la habilidad:
    La primera habilidad contará con daño inicial, despues de aplicar la habilidad por primera vez, habra una espera de 
    .5 segundos para pasado este tiempo de "espera" varios ticks se aplicaran rapidamente haciendo de esto un burst de daño.
    La habilidad durará 5 segundos, y el debuff de vulnerabilidad durará 3 segundos.
    El debuff de vulnerabilidad hará que el enemigo reciba un 50% mas de daño de todas las fuentes.
    Al chile que primera habilidad tan rota xd
    */

    /*
    Para la segunda habilidad imagino lo siguiente. una marca que le aplicara al enemigo parecido a como  funciona la marca del cazador de Tyra(Paladins)
    el objetivo al tener esta marca aplicada el daño que recibirá de todas las fuentes será amplificado por 5seg y aplicará un slow del 20% basado en la velocidad
    del objetivo. Es una habilidad sencilla pero que hará relucir aun mas la primera y la ulti, es la parte de utilidad del personaje para su kit
    */

    // Start is called before the first frame update
    void Start()
    {
        //Calcular el daño por tick, que será un 5% de la vida maxima del enemigo
        tickDamage = health.maxHealth * 0.005f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(firstAbilityKey) && canUseQ == true)
        {
            StartCoroutine(QCast());
        }
        
        if (Input.GetKeyDown(secondAbilityKey) && canUseE == true)
        {
            StartCoroutine(CastE());
        }
    }


    public IEnumerator QCast()
    {
        // Deshabilita la habilidad para evitar múltiples activaciones
        canUseQ = false;

        // Aplica el daño inicial instantáneo como daño genérico
        health.TakeDamage(firstAbilityDamage, Health.damageType.generic);

        // Espera a que termine el efecto de daño sobre tiempo (DOT)
        // ApplyDOT() aplica varios ticks de daño después de un delay inicial
        yield return StartCoroutine(ApplyDOT());

        // Inicia el cooldown de la habilidad
        // Cuando termine el cooldown, la función lambda () => canUseQ = true rehabilitará la habilidad
        yield return StartCoroutine(AbilityCooldown(qCooldown, () => canUseQ = true));
    }

    private IEnumerator CastE()
    {
        // Deshabilitar la habilidad
        canUseE = false;

        // Aplicar marca
        isMarked = true;
        Debug.Log("Marked!");
        // Duración de la marca
        yield return new WaitForSeconds(markDuration);
        
        // Remover marca
        isMarked = false;
        Debug.Log("Mark expired.");
        // Iniciar cooldown
        yield return StartCoroutine(AbilityCooldown(eCooldown, () => canUseE = true));
    }

    public IEnumerator ApplyDOT()
    {
        yield return new WaitForSeconds(timeBeforeTickApplies);
        for (int i = 0; i < numberOfTicks; i++)
        {
            health.TakeDamage((int)tickDamage, Health.damageType.bleed); // Cambiado a daño de sangrado para aplicar más daño
            yield return new WaitForSeconds(timeBetweenTicks);
        }
    }
    
     private IEnumerator AbilityCooldown(float cooldownTime, System.Action onCooldownComplete)
    {
        yield return new WaitForSeconds(cooldownTime);
        onCooldownComplete?.Invoke();
    }
}
