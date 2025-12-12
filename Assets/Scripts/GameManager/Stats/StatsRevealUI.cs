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

    [Header("Score & Rank UI")]
    public TMP_Text finalScoreText;
    public TMP_Text rankText;


    [Header("Reveal Settings")]
    public float revealDelay = 0.3f;  
    public float statRevealDuration = 1f;
    public float scoreRevealDuration = 2f;
    public float rankRevealDelay = 0.5f;

    private bool revealing = false;

    private void Start()
    {
        if (finalScoreText != null) finalScoreText.text = "0";
        if (rankText != null) rankText.text = "";
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
            if (fields[i] != null)
            {
                yield return StartCoroutine(RevealOneStat(fields[i], finalValues[i], statRevealDuration));
                yield return new WaitForSeconds(revealDelay);
            }
        }

        yield return new WaitForSeconds(revealDelay * 2);

        if (finalScoreText != null)
        {
            yield return StartCoroutine(RevealOneStat(finalScoreText, CombatStatsResult.finalScore, scoreRevealDuration));
        }

        yield return new WaitForSeconds(rankRevealDelay);

        if (rankText != null)
        {
            yield return StartCoroutine(RevealRank(CombatStatsResult.rank));
        }

        revealing = false;
    }

    IEnumerator RevealOneStat(TMP_Text field, float finalValue, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            float current = Mathf.Lerp(0f, finalValue, t);

            if (finalValue == CombatStatsResult.totalDamageDealt || finalValue == CombatStatsResult.totalDamageTaken)
            {
                field.text = current.ToString("F1");
            }
            else
            {
                field.text = Mathf.FloorToInt(current).ToString();
            }

            yield return null;
        }

        if (finalValue == CombatStatsResult.totalDamageDealt || finalValue == CombatStatsResult.totalDamageTaken)
        {
            field.text = finalValue.ToString("F1");
        }
        else
        {
            field.text = ((int)finalValue).ToString();
        }
    }


    IEnumerator RevealRank(string rank)
    {
        rankText.text = "";

        foreach (char letter in rank)
        {
            rankText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        if (rankText.transform.localScale != Vector3.one)
        {
            rankText.transform.localScale = Vector3.zero;
            float timer = 0f;
            float duration = 0.3f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                // Elastic bounce
                float scale = Mathf.Lerp(0f, 1.2f, t);
                rankText.transform.localScale = Vector3.one * scale;

                yield return null;
            }

            timer = 0f;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                float t = timer / 0.2f;
                float scale = Mathf.Lerp(1.2f, 1f, t);
                rankText.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            rankText.transform.localScale = Vector3.one;
        }
    }
}
