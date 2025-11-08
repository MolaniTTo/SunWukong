using UnityEngine;

public class GameManager : MonoBehaviour
{

    //#############################################################

    
    [Header("CombatStatsTracker")]     //PARTE DE RECOPILACION DE STATS DE COMBATE DEL JUGADOR
    public int totalAttacks;
    public int totalHits;
    public int totalKills;
    public float totalDamageTaken;

    private void OnEnable()
    {
        CombatEvents.OnPlayerAttack += OnAttack;
        CombatEvents.OnHit += OnHit;
        CombatEvents.OnEnemyKilled += OnKill;
        CombatEvents.OnPlayerDamaged += OnPlayerDamaged;
    }

    private void OnDisable()
    {
        CombatEvents.OnPlayerAttack -= OnAttack;
        CombatEvents.OnHit -= OnHit;
        CombatEvents.OnEnemyKilled -= OnKill;
        CombatEvents.OnPlayerDamaged -= OnPlayerDamaged;
    }

    private void OnAttack() => totalAttacks++;
    private void OnHit(GameObject attacker, GameObject receiver) => totalHits++;
    private void OnKill(GameObject enemy) => totalKills++;
    private void OnPlayerDamaged(float damage) => totalDamageTaken += damage;

    //#############################################################

}
