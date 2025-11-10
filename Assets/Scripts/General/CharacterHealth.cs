using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Identification")]
    public string characterName = "Unnamed";
    public bool isPlayer = false;

    //Events
    public event Action<float> OnHealthChanged; //event per notificar canvis en la vida (passa la vida actual)
    public event Action OnDeath; //event per notificar la mort del personatge
    public event Action<float, GameObject> OnTakeDamage; //event per notificar que ha rebut danys (passa la vida actual i el gameobject que l'ha causat)

    private bool isDead = false;

    private void Awake() //ho fem virtual perque els fills puguin sobreescriure-ho i cridar al base.awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (isDead) { return; }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); //Assegurem que la vida no baixi de 0 ni superi la vida maxima

        OnTakeDamage?.Invoke(currentHealth, attacker); //notifiquem que ha rebut danys
        OnHealthChanged?.Invoke(currentHealth); //notifiquem el canvi de vida

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount) //funcio en cas de que el personatge es curi
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        if(isDead) { return; }
        isDead = true;

        OnDeath?.Invoke(); //notifiquem la mort del personatge

        if (isPlayer)
        {
            //gestionar spawnPoints
            //gestionar contador de morts per les estadistiques
        }
        else
        {
            //gestionar barra de ki pel jugador
            //gestionar numero de morts per les estadistiques
        }
    }



}
