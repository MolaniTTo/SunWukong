using UnityEngine;
using System.Collections;

public class PlayerTemporaryEffects : MonoBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private BarraDeKi barraKi;
    
    private bool hasInfiniteKi = false;
    private Coroutine infiniteKiCoroutine;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject infiniteKiAuraPrefab; // Aura opcional adicional para ki infinito
    private GameObject currentInfiniteKiAura;

    private void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        barraKi = FindFirstObjectByType<BarraDeKi>();
    }
    
    public void ActivateInfiniteKi(float duration)
    {
        // Si ya hay un efecto activo, cancelarlo primero
        if (infiniteKiCoroutine != null)
        {
            StopCoroutine(infiniteKiCoroutine);
            CleanupInfiniteKiEffect();
        }
        
        infiniteKiCoroutine = StartCoroutine(InfiniteKiCoroutine(duration));
    }
    
    private IEnumerator InfiniteKiCoroutine(float duration)
    {
        hasInfiniteKi = true;
        
        // Cambiar color de la barra a azul
        if (barraKi != null)
        {
            barraKi.SetBarColor(Color.cyan);
        }
        
        // Activar aura de ki infinito si existe el prefab
        if (infiniteKiAuraPrefab != null && currentInfiniteKiAura == null)
        {
            currentInfiniteKiAura = Instantiate(infiniteKiAuraPrefab, transform.position, Quaternion.identity);
            
            AuraFollower follower = currentInfiniteKiAura.AddComponent<AuraFollower>();
            follower.target = transform;
            follower.offset = new Vector3(0f, 1f, 0f);
            
            currentInfiniteKiAura.transform.localScale = new Vector3(2f, 2f, 2f);
        }
        
        Debug.Log($"Efecto de Ki ilimitado activado durante {duration} segundos");
        
        // Mantener Ki al máximo durante la duración
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (playerStateMachine != null)
            {
                playerStateMachine.RestoreFullKi();
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Limpiar efectos
        CleanupInfiniteKiEffect();
        
        hasInfiniteKi = false;
        Debug.Log("Efecto de Ki ilimitado terminado");
    }
    
    private void CleanupInfiniteKiEffect()
    {
        // Restaurar color original de la barra
        if (barraKi != null)
        {
            barraKi.RestoreOriginalColor();
        }
        
        // Destruir aura de ki infinito si existe
        if (currentInfiniteKiAura != null)
        {
            Destroy(currentInfiniteKiAura);
            currentInfiniteKiAura = null;
        }
    }
    
    public bool HasInfiniteKi()
    {
        return hasInfiniteKi;
    }
    
    private void OnDestroy()
    {
        // Limpiar al destruir el componente
        if (currentInfiniteKiAura != null)
        {
            Destroy(currentInfiniteKiAura);
        }
    }
}