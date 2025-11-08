using System;
using UnityEngine;

public static class CombatEvents //no posem monobehaviour perque no cal que estigui assignat a cap GameObject
{
    public static event Action OnPlayerAttack; //es crida quan el jugador ataca
    public static event Action<GameObject, GameObject> OnHit; //es crida quan algu es colpejat, envia el GameObject de l'atacant i el del que rep el cop
    public static event Action<GameObject> OnEnemyKilled; //es crida quan un enemic es mort, envia el GameObject de l'enemic mort
    public static event Action<float> OnPlayerDamaged; //es crida quan el jugador rep danys, envia la quantitat de danys rebuts


    public static void PlayerAttack() => OnPlayerAttack?.Invoke(); //es crida quan el jugador ataca
    public static void Hit(GameObject attacker, GameObject receiver) => OnHit?.Invoke(attacker, receiver); //es crida quan algu es colpejat
    public static void EnemyKilled(GameObject enemy) => OnEnemyKilled?.Invoke(enemy); //es crida quan un enemic es mort
    public static void PlayerDamaged(float damage) => OnPlayerDamaged?.Invoke(damage); //es crida quan el jugador rep danys

}
