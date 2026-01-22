using UnityEngine;

public class BossMusicController : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private string bossMusicKey = "Boss";
    [SerializeField] private string returnMusicKey = "Base"; //musica a la que ha de tornar un cop el boss es derrotat
    [SerializeField] private float fadeTime = 2f;
    [SerializeField] private bool startOnAwake = false; //comencar musica al iniciar
    [SerializeField] private bool useTransitionMusic = false; //utilitzar musica de transicio/victoria
    [SerializeField] private string transitionMusicKey = "Base"; //musica de transicio/victoria

    [Header("Refs")]
    [SerializeField] private CharacterHealth bossHealth; //component de salut del boss

    private bool musicStarted = false;
    private bool bossDefeated = false;

    void Start()
    {
        if (bossHealth == null)
        {
            bossHealth = GetComponent<CharacterHealth>();
        }

        //se suscriu a l'event de mort del boss
        if (bossHealth != null)
        {
            bossHealth.OnDeath += OnBossDefeated;
        }

        if (startOnAwake)
        {
            StartBossMusic();
        }
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath -= OnBossDefeated;
        }
    }

    public void StartBossMusic() //ho cridarem desde el gorila quan acaba WakeUp i desde el monje quan li tira el primer raig
    {
        if (musicStarted || bossDefeated) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(bossMusicKey, fadeTime);
            musicStarted = true;
            Debug.Log($"Música de boss iniciada: {bossMusicKey}");
        }
    }

    private void OnBossDefeated()
    {
        if (bossDefeated) return;
        bossDefeated = true;

        if (AudioManager.Instance != null)
        {
            if (useTransitionMusic)
            {
                //reproducir musica de transicio/victoria
                AudioManager.Instance.PlayMusic(transitionMusicKey, fadeTime);

                //torna a la musica normal despres de 5 segons
                Invoke(nameof(ReturnToNormalMusic), 5f);
            }
            else
            {
                //torna directament a la musica normal
                ReturnToNormalMusic();
            }
        }

        Debug.Log("Boss derrotado, cambiando música");
    }

    public void ReturnToNormalMusic() //torna a la musica normal després del boss
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(returnMusicKey, fadeTime);
        }
    }

    //per si vulguessim acrivarho desde un trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !musicStarted)
        {
            StartBossMusic();
        }
    }
    
}
