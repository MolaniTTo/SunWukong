using System;
using UnityEngine;

public static class CombatEvents //no posem monobehaviour perque no cal que estigui assignat a cap GameObject
{
    public static event Action OnPlayerAttack; //es crida quan el jugador ataca
    public static event Action<GameObject, GameObject> OnHit; //es crida quan algu es colpejat, envia el GameObject de l'atacant i el del que rep el cop
    public static event Action<float> OnDamageDealt; //es crida quan es causa danys als enemics, envia la quantitat de danys causats
    public static event Action<GameObject> OnEnemyKilled; //es crida quan un enemic es mort, envia el GameObject de l'enemic mort
    public static event Action<float> OnPlayerDamaged; //es crida quan el jugador rep danys, envia la quantitat de danys rebuts
    public static event Action<bool> OnPlayerDeath; //es crida quan el jugador mor, envia true si mor, false si respawna
    public static event Action<bool> OnPlayerWin;


    public static void PlayerAttack() => OnPlayerAttack?.Invoke(); //es crida quan el jugador ataca
    public static void Hit(GameObject attacker, GameObject receiver) => OnHit?.Invoke(attacker, receiver); //es crida quan algu es colpejat
    public static void DamageDealt(float damage) => OnDamageDealt?.Invoke(damage); //es crida quan es causa danys als enemics
    public static void EnemyKilled(GameObject enemy) => OnEnemyKilled?.Invoke(enemy); //es crida quan un enemic es mort
    public static void PlayerDamaged(float damage) => OnPlayerDamaged?.Invoke(damage); //es crida quan el jugador rep danys
    public static void PlayerDeath(bool isDead) => OnPlayerDeath?.Invoke(isDead); //es crida quan el jugador mor o respawna
    public static void PlayerWin(bool hasWon) => OnPlayerWin?.Invoke(hasWon); //es crida quan el jugador guanya




}
