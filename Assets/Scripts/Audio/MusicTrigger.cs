using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string musicKey = "Boss"; //nom de la musica a reproduir
    [SerializeField] private float fadeTime = 1.5f;
    [SerializeField] private bool playOnce = false; //si es true nomes es reprodueix una vegada
    [SerializeField] private bool restorePreviousOnExit = false; //si es true restaura la musica anterior al sortir del trigger

    private bool hasPlayed = false;
    private string previousMusic = "";

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playOnce && hasPlayed) return; // Si ja s'ha reproduït i és només una vegada, sortir

            if (AudioManager.Instance != null)
            {
                //si cal restaurar la musica anterior al sortir, la guardem
                if (restorePreviousOnExit)
                {
                    previousMusic = AudioManager.Instance.GetCurrentMusic();
                }

                AudioManager.Instance.PlayMusic(musicKey, fadeTime);
                hasPlayed = true;
                Debug.Log($"Trigger activado: {musicKey}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && restorePreviousOnExit)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(previousMusic))
            {
                AudioManager.Instance.PlayMusic(previousMusic, fadeTime);
                Debug.Log($"Restaurando música anterior: {previousMusic}");
            }
        }
    }

    //per reiniciar el trigger des de altres scripts si es necessari
    public void ResetTrigger()
    {
        hasPlayed = false;
    }

}
