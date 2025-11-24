using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    public float randomDuration = 1.2f; //temps que dura la fase de random final
    public float randomSpeed = 0.05f; //rapidesa del random durant la fase de random
    public float transitionSpeed = 1.8f; //rapidesa de la transició cap al valor final

    private bool revealing = false;


    public void StartReveal(GameManager stats) //la cridem des del GameManager quan canviem a l'escena de stats
    {
        if (revealing) return;

        revealing = true;

        StartCoroutine(RevealStats(stats));
    }

    IEnumerator RevealStats(GameManager stats)
    {

        TMP_Text[] fields = {
            attacksText, hitsText, damageDealtText, killsText, damageTakenText //crea un array amb totes les referencies als texts
        };


        float[] finalValues = { //crea un array amb els valors finals obtinguts del GameManager
            stats.totalAttacks,
            stats.totalHits,
            stats.totalDamageDealt,
            stats.totalKills,
            stats.totalDamageTaken

        };

        //comencen tots a fer el random
        bool[] revealed = new bool[fields.Length];
        Coroutine[] randomizers = new Coroutine[fields.Length];

        for (int i = 0; i < fields.Length; i++)
        {
            randomizers[i] = StartCoroutine(RandomizeField(fields[i], revealed, i)); //inicia el random per a cada camp
        }

        //va revelant un per un cada stat per ordre
        for (int i = 0; i < fields.Length; i++)
        {
            yield return StartCoroutine(RevealOneStat(fields[i], finalValues[i]));

            // marcar como revelado → ese ya no sigue randomizando
            revealed[i] = true;

            yield return new WaitForSeconds(0.3f); // pequeño delay entre stats
        }

        revealing = false;
    }


    IEnumerator RandomizeField(TMP_Text field, bool[] revealed, int index) //random que mante a els demes stats abans de ser revelats
    {
        while (!revealed[index]) //mentre no estigui revelat aquest stat
        {
            field.text = Random.Range(0, 999).ToString(); //assigna un valor random
            yield return new WaitForSeconds(randomSpeed); //espera un temps abans de canviar-lo de nou
        }
    }

    IEnumerator RevealOneStat(TMP_Text field, float finalValue) //revela un stat concret
    {
        float timer = 0;

        while (timer < randomDuration) //FASE DE RANDOM FINAL
        {
            field.text = Random.Range(0, 999).ToString();
            timer += Time.deltaTime;
            yield return null;
        }


        float current = Random.Range(0, 999);

        while (Mathf.Abs(current - finalValue) > 0.1f) //TRANSICIÓ CAP AL VALOR FINAL
        {
            current = Mathf.Lerp(current, finalValue, Time.deltaTime * transitionSpeed); //interpola cap al valor final
            field.text = Mathf.FloorToInt(current).ToString();
            yield return null;
        }

        //dona el valor final exacte
        field.text = finalValue.ToString();

    }
}
