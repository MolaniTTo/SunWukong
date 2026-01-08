using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;
using System.Collections;

public class FirstSequence : MonoBehaviour
{
    public ScreenFade screenFader;
    public PlayerStateMachine player; //per bloquejar input del jugador

    public void StartSequence()
    {
        StartCoroutine(FirstSequenceRoutine());
    }
    private IEnumerator FirstSequenceRoutine()
    {
        yield return new WaitForSeconds(0.5f); //esperem que faci el fade out
        player.animator.SetTrigger("WakeUp"); 
        Debug.Log("First Sequence started");
        player.EnterDialogueMode(); //posem el jugador en mode diàleg (bloqueja moviments i ataca)
        screenFader.FadeIn();
        AudioManager.Instance.PlayMusic("StartSequence", 1f); //posem musica de la sequencia d'inici
    }

    public void EndSequence() //cridat des de l'animacio quan acaba la sequencia
    {
        Debug.Log("First Sequence ended");
        AudioManager.Instance.PlayMusic("Base", 1f); //posem musica base
        player.ExitDialogueMode(); //el jugador ja pot moure's
        player.wakeUpFromSleep = true; //el jugador ja no està despertant-se
    }

}



