using Unity.Cinemachine;

using UnityEngine;

public class BossTriggerZone2D : MonoBehaviour
{
    public Gorila gorila;
    public Monje monje;
    public GameObject playerObject;

    public CinemachineCamera camBoss;
    public CinemachineCamera camNormal;
    public GameObject invisibleWalls; //parets invisibles que es activen quan el jugador entra a la zona del boss

    private bool triggered = false;

    private void Start()
    {
        camBoss.Priority = 0; //Ens assegurem que la camara del boss comenci desactivada
        invisibleWalls.SetActive(false); //Ens assegurem que les parets invisibles comencin desactivades
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.gameObject == playerObject)
        {
            triggered = true;
            camBoss.Priority = 2; //Augmentem la prioritat de la camara del boss perque s'activi
            invisibleWalls.SetActive(true); //activem les parets invisibles
            gorila.playerIsOnConfiner = true; //indiquem al gorila que el jugador esta dins del confiner
        }
    }


    public void OnBossDefeated() //s'ha de cridar quan mori el gorila
    {
        camBoss.Priority = 0; //Baixem la prioritat de la camara del boss perque es desactivi
        gorila.playerIsOnConfiner = false; //indiquem al gorila que el jugador ja no esta dins del confiner
        invisibleWalls.SetActive(false); //desactivem les parets invisibles
    }
}
