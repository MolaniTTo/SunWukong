using System.Collections;
using UnityEngine;

/// <summary>
/// Maneja los efectos visuales y de audio al morir un enemigo.
/// Puede ser usado por cualquier enemigo o jefe.
/// </summary>
public class DeathEffectHandler : MonoBehaviour
{
    [Header("Death Effect Settings")]
    [Tooltip("Prefab del efecto de humo/partículas que aparecerá al morir")]
    public GameObject deathEffectPrefab;
    
    [Tooltip("Posición offset del efecto respecto al enemigo")]
    public Vector3 effectOffset = Vector3.zero;
    
    [Tooltip("Tiempo que tarda en destruirse el efecto (si es 0, se autodestruye según el Animator/Particles)")]
    public float effectDuration = 2f;
    
    [Header("Audio Settings")]
    [Tooltip("Clip de audio que se reproducirá al morir")]
    public AudioClip deathSound;
    
    [Tooltip("Volumen del sonido de muerte (0 a 1)")]
    [Range(0f, 1f)]
    public float deathSoundVolume = 1f;
    
  
    
    [Header("Destruction Settings")]
    [Tooltip("Tiempo de espera después de la animación de muerte antes de destruir el enemigo")]
    public float destroyDelay = 1.5f;
    
    [Tooltip("¿Destruir el enemigo después de la animación?")]
    public bool destroyAfterAnimation = true;
    
    [Header("Optional Settings")]
    [Tooltip("¿Hacer fade out del sprite antes de destruir?")]
    public bool fadeOutSprite = false;
    
    [Tooltip("Duración del fade out")]
    public float fadeDuration = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = true;

    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Inicia la secuencia de muerte con efectos
    /// </summary>
    public void TriggerDeathSequence()
    {
        if (isDying) return; // Evitar múltiples llamadas
        
        isDying = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"[DeathEffectHandler] Iniciando secuencia de muerte para {gameObject.name}");
        }
        
        StartCoroutine(DeathSequence());
    }

    /// <summary>
    /// Reproduce el sonido de muerte
    /// </summary>
    private void PlayDeathSound()
    {
        if (deathSound == null)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[DeathEffectHandler] No hay AudioClip asignado para {gameObject.name}");
            }
            return;
        }

       
        else
        {
            // Audio 2D (se escucha igual desde cualquier distancia)
            // Crear un GameObject temporal para reproducir el audio
            GameObject audioObject = new GameObject("DeathSound_Temp");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            
            audioSource.clip = deathSound;
            audioSource.volume = deathSoundVolume;
            audioSource.spatialBlend = 0f; // 2D audio
            audioSource.Play();
            
            // Destruir el objeto temporal cuando termine el audio
            Destroy(audioObject, deathSound.length);
            
            if (showDebugLogs)
            {
                Debug.Log($"[DeathEffectHandler] Reproduciendo sonido 2D: {deathSound.name}");
            }
        }
    }

    private IEnumerator DeathSequence()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[DeathEffectHandler] Esperando {destroyDelay} segundos...");
        }
        
        // 1. Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(destroyDelay);

        // 2. Spawn del efecto de humo/partículas
        if (deathEffectPrefab != null)
        {
            Vector3 effectPosition = transform.position + effectOffset;
            
            if (showDebugLogs)
            {
                Debug.Log($"[DeathEffectHandler] Spawneando efecto en posición: {effectPosition}");
            }
            
            // REPRODUCIR EL SONIDO JUSTO ANTES DE SPAWNEAR EL EFECTO
            PlayDeathSound();
            
            GameObject effect = Instantiate(deathEffectPrefab, effectPosition, Quaternion.identity);
            
            // Verificar que el efecto tiene Particle System
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[DeathEffectHandler] Particle System encontrado. Playing: {ps.isPlaying}");
                }
                
                // Asegurarse de que el Particle System está reproduciéndose
                if (!ps.isPlaying)
                {
                    ps.Play();
                }
            }
            else
            {
                // Buscar en hijos
                ps = effect.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DeathEffectHandler] Particle System encontrado en hijo. Playing: {ps.isPlaying}");
                    }
                    if (!ps.isPlaying)
                    {
                        ps.Play();
                    }
                }
                else
                {
                    Debug.LogWarning($"[DeathEffectHandler] No se encontró Particle System en el prefab {deathEffectPrefab.name}");
                }
            }
            
            // Destruir el efecto automáticamente si tiene duración
            if (effectDuration > 0)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[DeathEffectHandler] Efecto se destruirá en {effectDuration} segundos");
                }
                Destroy(effect, effectDuration);
            }
        }
        else
        {
            Debug.LogWarning($"[DeathEffectHandler] deathEffectPrefab es NULL en {gameObject.name}");
        }

        // 3. Opcional: Fade out del sprite
        if (fadeOutSprite && spriteRenderer != null)
        {
            yield return StartCoroutine(FadeOutSprite());
        }

        // 4. Destruir el objeto del enemigo
        if (destroyAfterAnimation)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[DeathEffectHandler] Destruyendo enemigo {gameObject.name}");
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeOutSprite()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    // Método para llamar desde Animation Event si prefieres ese enfoque
    public void OnDeathAnimationComplete()
    {
        TriggerDeathSequence();
    }
    
    // Método para testear en el editor
    [ContextMenu("Test Death Effect")]
    private void TestDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            Vector3 effectPosition = transform.position + effectOffset;
            GameObject effect = Instantiate(deathEffectPrefab, effectPosition, Quaternion.identity);
            Debug.Log($"Efecto spawneado en: {effectPosition}");
            
            ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Debug.Log("Particle System reproducido");
            }
        }
        else
        {
            Debug.LogError("deathEffectPrefab es NULL!");
        }
        
        // Testear también el sonido
        PlayDeathSound();
    }
}