using System.Collections;
using UnityEngine;
using TMPro;

public class StatsRevealUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text attacksText;
    public TMP_Text hitsText;
    public TMP_Text damageDealtText;
    public TMP_Text killsText;
    public TMP_Text damageTakenText;

    [Header("Reveal Settings")]
    public float revealDelay = 0.3f;   // tiempo entre stats
    public float revealDuration = 3f;  // duración del count-up

    private bool revealing = false;

    private void Start()
    {
        StartReveal();
    }

    public void StartReveal()
    {
        if (revealing) return;
        revealing = true;
        StartCoroutine(RevealStats());
    }

    IEnumerator RevealStats()
    {
        TMP_Text[] fields = {
            attacksText, hitsText, damageDealtText, killsText, damageTakenText
        };

        float[] finalValues = {
            CombatStatsResult.totalAttacks,
            CombatStatsResult.totalHits,
            CombatStatsResult.totalDamageDealt,
            CombatStatsResult.totalKills,
            CombatStatsResult.totalDamageTaken,

        };

        for (int i = 0; i < fields.Length; i++)
        {
            yield return StartCoroutine(RevealOneStat(fields[i], finalValues[i]));
            yield return new WaitForSeconds(revealDelay);
        }

        revealing = false;
    }

    IEnumerator RevealOneStat(TMP_Text field, float finalValue)
    {
        float timer = 0f;
        float duration = revealDuration;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Ease-out cubic (rápido → lento)
            float t = timer / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            float current = Mathf.Lerp(0f, finalValue, t);
            field.text = Mathf.FloorToInt(current).ToString();

            yield return null;
        }

        field.text = ((int)finalValue).ToString();
    }
}
