using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BananaPickup : MonoBehaviour
{
    [Header("Banana Type")]
    public BananaType bananaType;
    
    [Header("Temporary Effects Duration")]
    [SerializeField] private float blueEffectDuration = 60f; // Duración del Ki ilimitado
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject pickupParticles; // Partículas que se instancian al recoger
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject visualContainer; // Contenedor con sprite y partículas idle
    
    [Header("Player Aura Effects")]
    [SerializeField] private GameObject yellowAuraPrefab; // Aura para plátano amarillo (Ki)
    [SerializeField] private GameObject redAuraPrefab;    // Aura para plátano rojo (Vida)
    [SerializeField] private GameObject blueAuraPrefab;   // Aura para plátano azul (Ki infinito)
    [SerializeField] private float shortAuraDuration = 1.5f; // Duración para auras amarilla y roja
    
    private SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    
    public enum BananaType
    {
        Yellow,  // Restaura Ki al máximo
        Red,     // Restaura vida al máximo
        Blue     // Ki ilimitado durante 60 segundos (barra azul)
    }
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Si no tiene AudioSource, añadirlo automáticamente
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger detectado con: {collision.gameObject.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            PlayerStateMachine playerController = collision.GetComponent<PlayerStateMachine>();
            if (playerController != null)
            {
                Debug.Log("¡Jugador detectado! Recogiendo plátano automáticamente...");
                CollectBanana(playerController);
            }
            else
            {
                Debug.LogWarning("El objeto tiene tag 'Player' pero no tiene PlayerStateMachine!");
            }
        }
    }
    
    private void CollectBanana(PlayerStateMachine playerController)
    {
        if (playerController == null) return;

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterBananaCollected(gameObject);
        }

        // Aplicar efecto según el tipo de plátano
        switch (bananaType)
        {
            case BananaType.Yellow:
                ApplyYellowBananaEffect(playerController);
                break;
                
            case BananaType.Red:
                ApplyRedBananaEffect(playerController);
                break;
                
            case BananaType.Blue:
                ApplyBlueBananaEffect(playerController);
                break;
        }
        
        // Efectos visuales y sonoros
        SpawnPickupEffects();
        
        // Desactivar el collider para que no se pueda recoger otra vez
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Ocultar todo el contenido visual (sprite + particle systems hijos)
        if (visualContainer != null)
        {
            visualContainer.SetActive(false);
        }
        else
        {
            // Si no hay contenedor, ocultar solo el sprite
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            
            // Y detener todos los Particle Systems hijos
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        
        // Destruir el plátano después de que el sonido termine
        float soundLength = pickupSound != null ? pickupSound.length : 0.5f;
        Destroy(gameObject, soundLength + 0.1f);
    }
    
    private void ApplyYellowBananaEffect(PlayerStateMachine playerController)
    {
        // Restaurar Ki al máximo
        playerController.RestoreFullKi();
        
        // Activar aura amarilla temporal
        if (yellowAuraPrefab != null)
        {
            SpawnPlayerAura(playerController.transform, yellowAuraPrefab, shortAuraDuration);
        }
        
        Debug.Log("¡Plátano Amarillo recogido! Ki restaurado al máximo");
    }
    
    private void ApplyRedBananaEffect(PlayerStateMachine playerController)
    {
        // Restaurar vida al máximo
        CharacterHealth health = playerController.characterHealth;
        if (health != null)
        {
            health.currentHealth = health.maxHealth;
            health.ForceHealthUpdate();
        }
        
        // Activar aura roja temporal
        if (redAuraPrefab != null)
        {
            SpawnPlayerAura(playerController.transform, redAuraPrefab, shortAuraDuration);
        }
        
        Debug.Log("¡Plátano Rojo recogido! Vida restaurada al máximo");
    }
    
    private void ApplyBlueBananaEffect(PlayerStateMachine playerController)
    {
        // Activar Ki ilimitado con barra azul
        PlayerTemporaryEffects tempEffects = playerController.GetComponent<PlayerTemporaryEffects>();
        if (tempEffects == null)
        {
            tempEffects = playerController.gameObject.AddComponent<PlayerTemporaryEffects>();
        }
        
        tempEffects.ActivateInfiniteKi(blueEffectDuration);
        
        // Activar aura azul durante toda la duración del efecto
        if (blueAuraPrefab != null)
        {
            SpawnPlayerAura(playerController.transform, blueAuraPrefab, blueEffectDuration);
        }
        
        Debug.Log($"¡Plátano Azul recogido! Ki ilimitado durante {blueEffectDuration} segundos");
    }
    
    private void SpawnPickupEffects()
    {
        // Partículas de recolección
        if (pickupParticles != null)
        {
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        }
        
        // Reproducir sonido con el AudioSource del plátano
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    private void SpawnPlayerAura(Transform playerTransform, GameObject auraPrefab, float duration)
    {
        // Instanciar el aura como hijo del jugador
        GameObject aura = Instantiate(auraPrefab, playerTransform.position, Quaternion.identity, playerTransform);
        
        // Posicionar el aura en el centro del jugador (ajusta Y para centrarlo verticalmente)
        aura.transform.localPosition = new Vector3(0f, 1f, 0f); // Ajusta el 1f según la altura de tu personaje
        
        // Escalar el aura para que sea visible (ajusta según necesites)
        aura.transform.localScale = new Vector3(2f, 2f, 2f); // Multiplica el tamaño por 2
        
        // Destruir el aura después de la duración especificada
        Destroy(aura, duration);
        
        Debug.Log($"Aura activada durante {duration} segundos en posición: {aura.transform.localPosition} con escala: {aura.transform.localScale}");
    }
}