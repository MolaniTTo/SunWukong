using UnityEngine;
using UnityEngine.InputSystem;

public class BananaPickup : MonoBehaviour
{
    [Header("Banana Type")]
    public BananaType bananaType;
    
    [Header("Stats Bonuses")]
    [SerializeField] private float yellowMaxKiBonus = 50f;
    [SerializeField] private float greenMaxHealthBonus = 75f;
    [SerializeField] private float redDamageBonus = 7.5f;
    [SerializeField] private float blueKiReductionPercent = 25f; // 25%
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject pickupParticles; // Partículas que se instancian al recoger
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject visualContainer; // Contenedor con sprite y partículas idle
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    
    private bool playerInRange = false;
    private PlayerStateMachine playerController;
    private InputSystem_Actions inputActions;
    private SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    
    public enum BananaType
    {
        Yellow,  // Aumenta Ki máximo
        Green,   // Aumenta vida máxima
        Red,     // Aumenta daño de ataques
        Blue     // Reduce consumo de Ki
    }
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        inputActions = new InputSystem_Actions();
        
        // Si no tiene AudioSource, añadirlo automáticamente
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    private void Update()
    {
        if (playerInRange && playerController != null)
        {
            Debug.Log($"Jugador en rango, esperando input... (dialogueLocked: {playerController.dialogueLocked})");
            
            // Leer directamente del Input System sin pasar por el player controller
            if (inputActions.Player.AttackPunch.triggered)
            {
                Debug.Log("¡Botón A detectado! Recogiendo plátano...");
                CollectBanana();
            }
            
            // También permitir con el botón de salto como alternativa
            if (inputActions.Player.Jump.triggered)
            {
                Debug.Log("¡Botón de salto detectado! Recogiendo plátano...");
                CollectBanana();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger detectado con: {collision.gameObject.name}, Tag: {collision.tag}");
        
        if (collision.CompareTag("Player"))
        {
            playerController = collision.GetComponent<PlayerStateMachine>();
            if (playerController != null)
            {
                playerInRange = true;
                Debug.Log("¡Jugador en rango del plátano! Presiona A/X para recoger.");
            }
            else
            {
                Debug.LogWarning("El objeto tiene tag 'Player' pero no tiene PlayerStateMachine!");
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Jugador salió del rango del plátano");
            playerInRange = false;
            playerController = null;
        }
    }
    
    private void CollectBanana()
    {
        if (playerController == null) return;
        
        // Aplicar efecto según el tipo de plátano
        switch (bananaType)
        {
            case BananaType.Yellow:
                ApplyYellowBananaEffect();
                break;
                
            case BananaType.Green:
                ApplyGreenBananaEffect();
                break;
                
            case BananaType.Red:
                ApplyRedBananaEffect();
                break;
                
            case BananaType.Blue:
                ApplyBlueBananaEffect();
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
    
    private void ApplyYellowBananaEffect()
    {
        // Aumentar Ki máximo
        playerController.maxKi += yellowMaxKiBonus;
        playerController.RestoreFullKi(); // Restaurar Ki al máximo nuevo
        
        // Notificar a la barra de Ki que actualice su máximo
        BarraDeKi barraKi = FindFirstObjectByType<BarraDeKi>();
        if (barraKi != null)
        {
            barraKi.ActualizarMaxKi(playerController.maxKi);
        }
        
        Debug.Log($"¡Plátano Amarillo recogido! Ki máximo aumentado a {playerController.maxKi}");
    }
    
    private void ApplyGreenBananaEffect()
    {
        // Aumentar vida máxima
        CharacterHealth health = playerController.characterHealth;
        if (health != null)
        {
            health.maxHealth += greenMaxHealthBonus;
            health.currentHealth += greenMaxHealthBonus; // Añadir la vida extra también al current
            health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
            health.ForceHealthUpdate(); // Actualizar la UI de salud
            
            // Notificar a la barra de vida que actualice su máximo
            BarraDeVida barraVida = FindFirstObjectByType<BarraDeVida>();
            if (barraVida != null)
            {
                barraVida.ActualizarMaxVida(health.maxHealth);
            }
        }
        
        Debug.Log($"¡Plátano Verde recogido! Vida máxima aumentada en {greenMaxHealthBonus}");
    }
    
    private void ApplyRedBananaEffect()
    {
        // Aumentar daño de ataques
        PlayerDamageModifier damageModifier = playerController.GetComponent<PlayerDamageModifier>();
        if (damageModifier == null)
        {
            damageModifier = playerController.gameObject.AddComponent<PlayerDamageModifier>();
        }
        
        damageModifier.AddDamageBonus(redDamageBonus);
        
        Debug.Log($"¡Plátano Rojo recogido! Daño aumentado en {redDamageBonus}");
    }
    
    private void ApplyBlueBananaEffect()
    {
        // Reducir consumo de Ki
        float reductionFactor = 1f - (blueKiReductionPercent / 100f);
        
        playerController.specialAttackPunchCost *= reductionFactor;
        playerController.specialAttackStaffCost *= reductionFactor;
        playerController.healingKiCostPerSecond *= reductionFactor;
        
        Debug.Log($"¡Plátano Azul recogido! Consumo de Ki reducido en {blueKiReductionPercent}%");
    }
    
    private void SpawnPickupEffects()
    {
        // Partículas
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
    
    // Visualización del rango de interacción en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

// Script adicional para manejar el modificador de daño
public class PlayerDamageModifier : MonoBehaviour
{
    [HideInInspector] public float totalDamageBonus = 0f;
    
    public void AddDamageBonus(float bonus)
    {
        totalDamageBonus += bonus;
    }
    
    public float GetTotalDamage(float baseDamage)
    {
        return baseDamage + totalDamageBonus;
    }
}