using UnityEngine;

public class CloudSignalReceiver : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject nube;
    public Transform personaje;
    
    [Header("Control del jugador")]
    public PlayerStateMachine PlayerStateMachine; 
  
    
    public void SepararNube()
    {
        if (nube != null)
        {
            nube.transform.SetParent(null);
        }
    }
    
    public void DesactivarNube()
    {
        if (nube != null)
        {
            nube.SetActive(false);
        }
    }
    
    
    public void DesactivarControlJugador()
    {
        Debug.Log("✓ Control del jugador desactivado");
        if (PlayerStateMachine != null)
        {
            PlayerStateMachine.enabled = false;
        }
    }
    
    
    public void ActivarControlJugador()
    {
        Debug.Log("✓ Control del jugador activado");
        if (PlayerStateMachine != null)
        {
            PlayerStateMachine.enabled = true;
        }
    }
}


