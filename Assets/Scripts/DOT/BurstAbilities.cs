using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstAbilities : MonoBehaviour
{
    [Header("References")]
    public Health health;
    private AudioManager audioManager;

    [Header("Q — Blood Burst | Parámetros")]
    public KeyCode firstAbilityKey = KeyCode.Q;
    public int firstAbilityDamage = 50;
    public float qCooldown = 5f;
    public int numberOfTicks = 5;
    public float timeBetweenTicks = 0.2f;
    public float timeBeforeTickApplies = 0.5f;
    public bool isFirstAbilityUsed = false;
    public float tickDamage = 0f;
    public bool canUseQ = true;

    [Header("E — Mark | Parámetros")]
    public KeyCode secondAbilityKey = KeyCode.E;
    public float damageAmplification = 0.5f; // 50% más de daño 
    public float eCooldown = 10f;
    public float markDuration = 5f;
    public bool isMarked = false;
    public bool canUseE = true;


    [Header("R — Ultimate | Parámetros (por definir)")]
    public KeyCode ultAbilityKey = KeyCode.R;
    public bool canUseR = true;
    public float ultDamage = 200f;
    public float pushBackForce = 100f;
    public float ultBuildUpTime = 0.5f;
    public float rCooldown = 50f;
    public bool isUltActive = false;
    public GameObject target;


    /* Resumen de la habilidad:
    La primera habilidad contará con daño inicial, despues de aplicar la habilidad por primera vez, habra una espera de 
    .5 segundos para pasado este tiempo de "espera" varios ticks se aplicaran rapidamente haciendo de esto un burst de daño.
    La habilidad durará 5 segundos.
    */

    /*
    Para la segunda habilidad imagino lo siguiente. una marca que le aplicara al enemigo parecido a como  funciona la marca del cazador de Tyra(Paladins)
    el objetivo al tener esta marca aplicada el daño que recibirá de todas las fuentes será amplificado por 5seg y aplicará un slow del 20% basado en la velocidad
    del objetivo. Es una habilidad sencilla pero que hará relucir aun mas la primera y la ulti, es la parte de utilidad del personaje para su kit
    */

    /*
    Para la ulti busco algo sencillo, el enfoque a DOT ya no existe puesto que todo el kit es burst
    La ulti será de cooldown corto ya que es una habilidad que hará daño directo y empujará al jugador para atras ayudando a ser habilidad de escape,
    al en
    */
    // Start is called before the first frame update
    void Start()
    {
        //Calcular el daño por tick, que será un 5% de la vida maxima del enemigo
        tickDamage = health.maxHealth * 0.005f;

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        target = GameObject.FindGameObjectWithTag("Player");
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
        if (Input.GetKeyDown(ultAbilityKey) && canUseR == true)
        {
            StartCoroutine(UltimateBurst());
        }
    }


    public IEnumerator QCast()
    {
        // Deshabilita la habilidad para evitar múltiples activaciones
        canUseQ = false;

        // Aplica el daño inicial instantáneo como daño genérico
        health.TakeDamage(firstAbilityDamage, Health.damageType.generic);
        audioManager.PlaySFX(audioManager.QFirstSlash);

        // Espera a que termine el efecto de daño sobre tiempo (DOT)
        // ApplyDOT() aplica varios ticks de daño después de un delay inicial
        yield return StartCoroutine(ApplyQBurst());

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
        audioManager.PlaySFX(audioManager.EActive);
        // Duración de la marca
        yield return new WaitForSeconds(markDuration);
        
        // Remover marca
        isMarked = false;
        Debug.Log("Mark expired.");
        audioManager.PlaySFX(audioManager.EOff);
        // Iniciar cooldown
        yield return StartCoroutine(AbilityCooldown(eCooldown, () => canUseE = true));
    }

    public IEnumerator ApplyQBurst()
    {
        yield return new WaitForSeconds(timeBeforeTickApplies);
        for (int i = 0; i < numberOfTicks; i++)
        {
            health.TakeDamage((int)tickDamage, Health.damageType.bleed); // Cambiado a daño de sangrado para aplicar más daño
            yield return new WaitForSeconds(timeBetweenTicks);
            audioManager.PlaySFX(audioManager.QTicksBurst);
        }
    }

    public IEnumerator UltimateBurst()
    {
        yield return new WaitForSeconds(ultBuildUpTime);
        canUseR = false;
        health.TakeDamage((int)ultDamage, Health.damageType.generic);
        target.GetComponent<Rigidbody>().AddForce(-target.transform.forward * pushBackForce, ForceMode.Impulse);
        Debug.Log("Ultimate Activated!");
        isUltActive = true;
        yield return StartCoroutine(AbilityCooldown(rCooldown, () => canUseR = true));
    }
    
     private IEnumerator AbilityCooldown(float cooldownTime, System.Action onCooldownComplete)
    {
        yield return new WaitForSeconds(cooldownTime);
        onCooldownComplete?.Invoke();
    }
}
