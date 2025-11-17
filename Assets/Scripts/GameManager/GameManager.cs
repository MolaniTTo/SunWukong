using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    public PlayerStateMachine player;
    public bool isOneHitMode = false; //Mode onetap per al jugador

    [Header("Fade Settings")]
    [SerializeField] private ScreenFade screenFade;


    //#############################################################


    [Header("CombatStatsTracker")]     //PARTE DE RECOPILACION DE STATS DE COMBATE DEL JUGADOR
    public int totalAttacks;
    public int totalHits;
    public float totalDamageDealt;
    public int totalKills;
    public float totalDamageTaken;
    public bool playerDead;

    private void OnEnable()
    {
        CombatEvents.OnPlayerAttack += OnAttack;
        CombatEvents.OnHit += OnHit;
        CombatEvents.OnDamageDealt += OnDamageDealt;
        CombatEvents.OnEnemyKilled += OnKill;
        CombatEvents.OnPlayerDamaged += OnPlayerDamaged;
        CombatEvents.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        CombatEvents.OnPlayerAttack -= OnAttack;
        CombatEvents.OnHit -= OnHit;
        CombatEvents.OnDamageDealt -= OnDamageDealt;
        CombatEvents.OnEnemyKilled -= OnKill;
        CombatEvents.OnPlayerDamaged -= OnPlayerDamaged;
        CombatEvents.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnAttack() => totalAttacks++;
    private void OnHit(GameObject attacker, GameObject receiver) => totalHits++;
    private void OnDamageDealt(float damage) => totalDamageDealt += damage;
    private void OnKill(GameObject enemy) => totalKills++;
    private void OnPlayerDamaged(float damage) => totalDamageTaken += damage;

    private void OnPlayerDeath(bool isDead)
    {
        playerDead = isDead;
        if (isDead)
        {
            if(isOneHitMode) //si estem en mode onetap s'acaba la partida i mostrem stats
            {
                //aqui canviem de escena i mostrem estadistiques
            }
            else
            {
                RespawnPlayer();
            }
        }

    }

    private void RespawnPlayer()
    {
        if (player == null)
        {
            return;
        }

        Transform checkPoint = player.lastCheckPoint;
        if (checkPoint != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            playerRb.simulated = false; 
            playerRb.linearVelocity = Vector2.zero;
            transform.position = checkPoint.position;

            if (player.characterHealth != null)
            {
                player.characterHealth.RestoreFullHealth();
            }

            playerRb.simulated = true;
            player.ForceNewState(PlayerStateMachine.PlayerState.Idle);
            CombatEvents.PlayerDeath(false); // notificar que ya no está muerto
        }
    }





    //#############################################################

}
