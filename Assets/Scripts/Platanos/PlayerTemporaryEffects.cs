using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerTemporaryEffects : MonoBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private BarraDeKi barraKi;
    
    private bool hasInfiniteKi = false;
  
    


    
    private Coroutine infiniteKiCoroutine;

    
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
    
   
    
    public bool HasInfiniteKi()
    {
        return hasInfiniteKi;
    }
    
}