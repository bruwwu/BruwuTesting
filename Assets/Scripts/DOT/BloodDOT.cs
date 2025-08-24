using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDOT : MonoBehaviour
{
    public Health health;

        [Header("Ability Damage")]
    public int firstAbilityDamage = 50;
    public bool isFirstAbilityUsed = false;
    public float abilityCooldown = 5f;

        [Header("DOT Settings")]
    public int numberOfTicks = 5;
    public float tickDamage = 0f;
    public float timeBetweenTicks = 0.2f;
    public float timeBeforeTickApplies = 0.5f;

        [Header("Debuff Settings")]
    public float durationOfDebuff = 3f;
    public int vulnerableMultiplier = 2;
    public bool isVulnerable = false;

    /* Resumen de la habilidad:
    La primera habilidad contará con daño inicial, despues de aplicar la habilidad por primera vez, habra una espera de 
    .5 segundos para pasado este tiempo de "espera" varios ticks se aplicaran rapidamente haciendo de esto un burst de daño.
    La habilidad durará 5 segundos, y el debuff de vulnerabilidad durará 3 segundos.
    El debuff de vulnerabilidad hará que el enemigo reciba un 50% mas de daño de todas las fuentes.
    Al chile que primera habilidad tan rota xd
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
        if (isFirstAbilityUsed == true)
        {
            abilityCooldown -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Q) && abilityCooldown > 0 && isFirstAbilityUsed == false)
            {
                isFirstAbilityUsed = true;
                health.health -= firstAbilityDamage;
                isVulnerable = true;
                StartCoroutine(ApplyDOT());
            }
            else if (abilityCooldown <= 0)
            {
                isFirstAbilityUsed = false;
                abilityCooldown = 5f;
                isVulnerable = false;
            }
    }

    public IEnumerator ApplyDOT()
    {
        if (isFirstAbilityUsed == true)
        {
            yield return new WaitForSeconds(timeBeforeTickApplies);
            for (int i = 0; i < numberOfTicks; i++)
            {
                health.health -= (int)tickDamage;
                yield return new WaitForSeconds(timeBetweenTicks);
            }
        }
        yield return null;
    }
}
