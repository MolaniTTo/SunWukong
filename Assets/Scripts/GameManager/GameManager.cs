using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    public PlayerStateMachine player;
    public bool isOneHitMode = false; //Mode onetap per al jugador

    [Header("Fade Settings")]
    public ScreenFade screenFade;

    [Header("First Sequence")]
    public FirstSequence firstSequence;

    [Header("Paralax Settings")]
    public ParallaxStatic[] parallaxLayers;

    [Header("CombatStatsTracker")]     //PARTE DE RECOPILACION DE STATS DE COMBATE DEL JUGADOR
    public int totalAttacks;
    public int totalHits;
    public float totalDamageDealt;
    public int totalKills;
    public float totalDamageTaken;
    public bool playerDead;

    [Header("BossSettings")]
    public BossTriggerZone2D gorilaBossZone;
    public BossTriggerZone2D monjeBossZone;

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
                SaveCombatStats();     
                ChangeToStatsScene();
            }
            else
            {
                if (player.isPlayerOnGorilaBossZone)
                {
                    monjeBossZone.OnPlayerDefeated(); //cridem a la funcio perque el jugador surti de la zona del boss
                }
                else if (player.isPlayerOnMonjeBossZone)
                {
                    monjeBossZone.OnPlayerDefeated(); //cridem a la funcio perque el jugador surti de la zona del boss
                }

                screenFade.FadeOut(); //fem fade out
                StartCoroutine(RespawnPlayer()); //respawnejem el jugador

            }
        }

    }

    private IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawning player...");
        yield return new WaitForSeconds(3.5f); //esperem 1 segon abans de respawnejar

        Transform checkPoint = player.lastCheckPoint;
        if (checkPoint != null)
        {
            Debug.Log("Player respawned at checkpoint: " + checkPoint.position);
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            playerRb.simulated = false; 
            playerRb.linearVelocity = Vector2.zero;
            player.transform.position = checkPoint.position;


            if (player.characterHealth != null)
            {
                player.characterHealth.RestoreFullHealth();
            }

            playerRb.simulated = true;
            player.ForceNewState(PlayerStateMachine.PlayerState.Idle);
            player.animator.SetTrigger("Respawn");
            player.isDead = false;
            CombatEvents.PlayerDeath(false); // notificar que ya no está muerto
            screenFade.FadeIn();
        }
    }





    //#############################################################

    private void SaveCombatStats()
    {
        CombatStatsResult.totalAttacks = totalAttacks;
        CombatStatsResult.totalHits = totalHits;
        CombatStatsResult.totalDamageDealt = totalDamageDealt;
        CombatStatsResult.totalKills = totalKills;
        CombatStatsResult.totalDamageTaken = totalDamageTaken;

        float finalScore = CalculateFinalScore();
        CombatStatsResult.finalScore = finalScore;
        CombatStatsResult.rank = GetRank(finalScore);
    }


    private float CalculateFinalScore()
    {
        //Puntuacio basada en precisio, atacs tirats i atacs donats
        float accuracy = totalAttacks > 0 ? (float)totalHits / totalAttacks : 0f;
        float accuracyScore = Mathf.Lerp(0f, 30f, accuracy);

        //Puntuacio basada en danys causats i rebuts
        float ratio = totalDamageDealt / (totalDamageTaken + 1f);
        float damageScore = Mathf.Clamp(ratio * 5f, 0f, 30f);

        //Nombre de kills
        float killScore = Mathf.Clamp(totalKills * 3f, 0f, 25f);

        //Puntuacio basada en agressivitat (atacs totals)
        float aggressionScore = Mathf.Clamp(totalAttacks * 0.05f, 0f, 15f);

        return accuracyScore + damageScore + killScore + aggressionScore;
    }

    private string GetRank(float score) //retorna el rang segons la puntuacio
    {
        if (score < 20f) return "Muy bajo";
        if (score < 40f) return "Mediocre";
        if (score < 60f) return "Normal";
        if (score < 75f) return "Bueno";
        if (score < 90f) return "Muy bueno";
        return "Impresionante";
    }

    private void ChangeToStatsScene()
    {
        if (screenFade != null)
        {
            screenFade.FadeOut();
        }
        //esperem una mica per fer el fade out
        UnityEngine.SceneManagement.SceneManager.LoadScene("StatsScene");

    }

    public void CanMoveParalax(bool canMove) //activa o desactiva el paralax
    {
        foreach (ParallaxStatic layer in parallaxLayers)
        {
            layer.stopPararallax = !canMove; //Si canMove es true, stopPararallax es false i viceversa
        }

    }


}
