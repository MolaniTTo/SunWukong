using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvincible = false;
    public bool isLowHealth = false;

    [Header("Identification")]
    public string characterName = "Unnamed";
    public bool isPlayer = false;


    //Events
    public event Action<float> OnHealthChanged; //event per notificar canvis en la vida (passa la vida actual)
    public event Action OnDeath; //event per notificar la mort del personatge
    public event Action<float, GameObject> OnTakeDamage; //event per notificar que ha rebut danys (passa la vida actual i el gameobject que l'ha causat)

    private bool isDead = false;
    private PlayerStateMachine playerStateMachine;

    private void Awake() //ho fem virtual perque els fills puguin sobreescriure-ho i cridar al base.awake()
    {
        currentHealth = maxHealth;
        if (isPlayer)
        {
            playerStateMachine = GetComponent<PlayerStateMachine>();
        }
    }

    public void TakeDamage(float amount, GameObject attacker)
    {
        if (isDead) { return; }

        if (isPlayer && playerStateMachine != null && playerStateMachine.currentState == PlayerStateMachine.PlayerState.Block)
        {
            amount *= 0.5f; //si el jugador esta bloquejant, redueixim el dany a la meitat
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); //Assegurem que la vida no baixi de 0 ni superi la vida maxima

        OnTakeDamage?.Invoke(currentHealth, attacker); //notifiquem que ha rebut danys
        OnHealthChanged?.Invoke(currentHealth); //notifiquem el canvi de vida

        if (isPlayer)
        {
            CombatEvents.PlayerDamaged(amount); //notifiquem als subscrits que el jugador ha rebut danys
        }

        else
        {
            CombatEvents.DamageDealt(amount); //notifiquem als subscrits que un enemic ha rebut danys
            CombatEvents.Hit(attacker, gameObject); //notifiquem als subscrits que un enemic ha estat colpejat
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (isPlayer && playerStateMachine != null && playerStateMachine.currentState != PlayerStateMachine.PlayerState.Block)
        {
            playerStateMachine.animator.SetTrigger("BeingHit"); //activem la animacio de ser colpejat
            playerStateMachine.audioSource.PlayOneShot(playerStateMachine.hurtSound); //reproduim el so de ser colpejat
            playerStateMachine.ForceNewState(PlayerStateMachine.PlayerState.BeingHit); //canviem l'estat del jugador a BeingHit
        }

        if(isPlayer && playerStateMachine != null && playerStateMachine.currentState == PlayerStateMachine.PlayerState.Block && playerStateMachine.hasStaff)
        {
            playerStateMachine.animator.SetTrigger("Parry"); //activem la animacio de ser colpejat mentre bloqueja amb el basto
            playerStateMachine.audioSource.PlayOneShot(playerStateMachine.hurtSound); //reproduim el so de ser colpejat
        }
    }

    public void Heal(float amount) //funcio en cas de que el personatge es curi
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); //Assegurem que la vida no superi la vida maxima
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ForceHealthUpdate()
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        if(isDead) { return; }
        isDead = true;

        OnDeath?.Invoke(); //notifiquem la mort del personatge

        if (!isPlayer) //si no es el jugador, notifiquem que un enemic ha mort
        {
            if(ProgressManager.Instance != null)
            {
                ProgressManager.Instance.RegisterEnemyDefeated(gameObject);
            }
            CombatEvents.EnemyKilled(gameObject);
            return;
        }

        else
        {
            if (playerStateMachine != null)
            {
                playerStateMachine.animator.SetTrigger("Death");
                playerStateMachine.audioSource.Stop();
                playerStateMachine.audioSource.PlayOneShot(playerStateMachine.deathSound);

                playerStateMachine.ForceNewState(PlayerStateMachine.PlayerState.Death);
            }

            CombatEvents.PlayerDeath(true);
        }

    }

    public void RestoreFullHealth()
    {
        isDead = false;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth);
    }

}
