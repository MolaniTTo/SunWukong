using UnityEngine;

public class RainZone : MonoBehaviour
{
    public float factorRalentizacion = 0.5f; // 50% más lento
    private float velocidadOriginal;
    private bool velocidadGuardada = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerCameraBounds pcb = col.GetComponent<PlayerCameraBounds>();

            if (pcb != null)
            {
                if (!velocidadGuardada)
                {
                    velocidadOriginal = pcb.velocidadMovimiento;
                    velocidadGuardada = true;
                }

                pcb.velocidadMovimiento *= factorRalentizacion;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerCameraBounds pcb = col.GetComponent<PlayerCameraBounds>();

            if (pcb != null && velocidadGuardada)
            {
                pcb.velocidadMovimiento = velocidadOriginal;
                velocidadGuardada = false;
            }
        }
    }
}
