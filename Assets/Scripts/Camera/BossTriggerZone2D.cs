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
    public BossSpawnController bossSpawnController;

    public CinemachineCamera camBoss;
    public CinemachineCamera camNormal;
    public GameObject invisibleWalls; //parets invisibles que es activen quan el jugador entra a la zona del boss
    public PlayerStateMachine playerStateMachine;

    private bool triggered = false;

    private void Start()
    {
        camBoss.Priority = 0; //Ens assegurem que la camara del boss comenci desactivada
        invisibleWalls.SetActive(false); //Ens assegurem que les parets invisibles comencin desactivades
        PlayerStateMachine playerStateMachine = playerObject.GetComponent<PlayerStateMachine>();
        UpdateGameManagerReference();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (collision.gameObject == playerObject)
        {
            playerStateMachine = playerObject.GetComponent<PlayerStateMachine>();
            UpdateGameManagerReference();
            if(bossType == BossType.Gorila) //Si es el gorila
            {
                gorila.playerIsOnConfiner = true; //indiquem al monje que el jugador esta dins del confiner
                playerStateMachine.isPlayerOnGorilaBossZone = true; //indiquem al player state machine que el jugador esta en zona de boss
            }
            else if(bossType == BossType.Monje) //Si es el monje
            {
                monje.playerIsOnConfiner = true; //indiquem al monje que el jugador esta dins del confiner
                playerStateMachine.isPlayerOnMonjeBossZone = true; //indiquem al player state machine que el jugador esta en zona de boss
            }

            triggered = true;
           

            camBoss.Priority = 2; //Augmentem la prioritat de la camara del boss perque s'activi
            invisibleWalls.SetActive(true); //activem les parets invisibles
            gameManager.CanMoveParalax(false); //desactivem el paralax
        }
    }

    private void UpdateGameManagerReference()
    {
        if (gameManager != null)
        {
            if (bossType == BossType.Gorila)
            {
                gameManager.gorilaBossZone = this;
            }
            else if (bossType == BossType.Monje)
            {
                gameManager.monjeBossZone = this;
            }
        }
        else
        {
            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            UpdateGameManagerReference();
        }
    }


    public void OnBossDefeated() //s'ha de cridar quan mori el gorila
    {
        if(bossType == BossType.Gorila) //Si es el gorila
        {
            
            gorila.playerIsOnConfiner = false; //indiquem al monje que el jugador ja no esta dins del confiner
            playerStateMachine.isPlayerOnGorilaBossZone = false; //indiquem al player state machine que el jugador ja no esta en zona de boss
        }
        else if(bossType == BossType.Monje) //Si es el monje
        {
            
            monje.playerIsOnConfiner = false; //indiquem al monje que el jugador ja no esta dins del confiner
            playerStateMachine.isPlayerOnMonjeBossZone = false; //indiquem al player state machine que el jugador ja no esta en zona de boss
            //aqui activem booleano perque aparegui una llum i activi el dialag amb el buda de pedra
        }

        camBoss.Priority = 0; //Baixem la prioritat de la camara del boss perque es desactivi
        invisibleWalls.SetActive(false); //desactivem les parets invisibles
        gameManager.CanMoveParalax(true); //reactivem el paralax
    }

    public void OnPlayerDefeated()
    {
        camBoss.Priority = 0; //Baixem la prioritat de la camara del boss perque es desactivi
        camNormal.Priority = 2; //Augmentem la prioritat de la camara normal perque s'activi
        
        if(bossType == BossType.Gorila) //Si es el gorila
        {
            playerStateMachine.isPlayerOnGorilaBossZone = false; //indiquem al player state machine que el jugador ja no esta en zona de boss
            bossSpawnController.SpawnGorilaBossZone(); //Reapareix la zona del gorila
        }
        else if(bossType == BossType.Monje) //Si es el monje
        {
            playerStateMachine.isPlayerOnMonjeBossZone = false; //indiquem al player state machine que el jugador ja no esta en zona de boss
            bossSpawnController.SpawnMonjeBossZone(); //Reapareix la zona del monje
        }
    }
}
