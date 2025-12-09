using Unity.Cinemachine;

using UnityEngine;

public enum BossType { Gorila, Monje }
public class BossTriggerZone2D : MonoBehaviour
{
    public BossType bossType;
    public Gorila gorila;
    public Monje monje;
    public GameObject playerObject;
    public GameManager gameManager;

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
            if(bossType == BossType.Gorila) //Si es el monje
            {
                gorila.playerIsOnConfiner = true; //indiquem al monje que el jugador esta dins del confiner
            }
            else if(bossType == BossType.Monje) //Si es el gorila
            {
                monje.playerIsOnConfiner = true; //indiquem al monje que el jugador esta dins del confiner
            }

            triggered = true;
            camBoss.Priority = 2; //Augmentem la prioritat de la camara del boss perque s'activi
            invisibleWalls.SetActive(true); //activem les parets invisibles
            gorila.playerIsOnConfiner = true; //indiquem al gorila que el jugador esta dins del confiner
            gameManager.CanMoveParalax(false); //desactivem el paralax
        }
    }


    public void OnBossDefeated() //s'ha de cridar quan mori el gorila
    {
        if(bossType == BossType.Gorila) //Si es el monje
        {
            gorila.playerIsOnConfiner = false; //indiquem al monje que el jugador ja no esta dins del confiner
        }
        else if(bossType == BossType.Monje) //Si es el gorila
        {
            monje.playerIsOnConfiner = false; //indiquem al monje que el jugador ja no esta dins del confiner
            //aqui activem booleano perque aparegui una llum i activi el dialag amb el buda de pedra
        }

        camBoss.Priority = 0; //Baixem la prioritat de la camara del boss perque es desactivi
        invisibleWalls.SetActive(false); //desactivem les parets invisibles
        gameManager.CanMoveParalax(true); //reactivem el paralax
    }
}
