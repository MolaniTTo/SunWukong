using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerTemporaryEffects : MonoBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private BarraDeKi barraKi;
    
    private bool hasInfiniteKi = false;
    private bool hasSpeedBoost = false;
    
    private float originalSpeed;
    private float speedMultiplier = 1.5f; // 50% más rápido
    
    private Coroutine infiniteKiCoroutine;
    private Coroutine speedBoostCoroutine;
    
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
        }
        
        infiniteKiCoroutine = StartCoroutine(InfiniteKiCoroutine(duration));
    }
    
    public void ActivateSpeedBoost(float duration)
    {
        // Si ya hay un efecto activo, cancelarlo primero
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
            // Restaurar velocidad antes de aplicar nuevo boost
            if (hasSpeedBoost && playerStateMachine != null)
            {
                playerStateMachine.speed = originalSpeed;
            }
        }
        
        speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(duration));
    }
    
    private IEnumerator InfiniteKiCoroutine(float duration)
    {
        hasInfiniteKi = true;
        
        // Cambiar color de la barra a azul
        if (barraKi != null)
        {
            barraKi.SetBarColor(Color.cyan);
        }
        
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
        
        // Restaurar color original de la barra
        if (barraKi != null)
        {
            barraKi.RestoreOriginalColor();
        }
        
        hasInfiniteKi = false;
        Debug.Log("Efecto de Ki ilimitado terminado");
    }
    
    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        hasSpeedBoost = true;
        
        if (playerStateMachine != null)
        {
            // Guardar velocidad original
            originalSpeed = playerStateMachine.speed;
            
            // Aplicar boost
            playerStateMachine.speed *= speedMultiplier;
            Debug.Log($"Velocidad aumentada de {originalSpeed} a {playerStateMachine.speed}");
        }
        
        // Esperar duración
        yield return new WaitForSeconds(duration);
        
        // Restaurar velocidad original
        if (playerStateMachine != null)
        {
            playerStateMachine.speed = originalSpeed;
            Debug.Log($"Velocidad restaurada a {originalSpeed}");
        }
        
        hasSpeedBoost = false;
        Debug.Log("Efecto de velocidad aumentada terminado");
    }
    
    public bool HasInfiniteKi()
    {
        return hasInfiniteKi;
    }
    
    public bool HasSpeedBoost()
    {
        return hasSpeedBoost; // Added 'return' keyword
    }
}