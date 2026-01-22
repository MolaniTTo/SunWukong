using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

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
    public GameObject invisibleWalls;
    public PlayerStateMachine playerStateMachine;

    private bool triggered = false;

    private void Start()
    {
        camBoss.Priority = 0;
        invisibleWalls.SetActive(false);

        // Buscar referencias si no est�n asignadas
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null)
        {
            playerStateMachine = playerObject.GetComponent<PlayerStateMachine>();
        }

        // Buscar BossSpawnController si no est� asignado
        if (bossSpawnController == null)
        {
            bossSpawnController = FindFirstObjectByType<BossSpawnController>();
            if (bossSpawnController == null)
            {
                Debug.LogWarning("BossSpawnController no encontrado! OnPlayerDefeated no funcionar� correctamente.");
            }
        }

        if (camNormal == null)
        {
            camNormal = GameObject.FindGameObjectWithTag("CinemachineMainCam").GetComponent<CinemachineCamera>();
        }


        UpdateGameManagerReference();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (collision.gameObject == playerObject)
        {
            // Actualizar referencia del player por si acaso
            if (playerStateMachine == null)
            {
                playerStateMachine = playerObject.GetComponent<PlayerStateMachine>();
            }

            UpdateGameManagerReference();

            if (bossType == BossType.Gorila)
            {
                gorila.playerIsOnConfiner = true;
                playerStateMachine.isPlayerOnGorilaBossZone = true;
            }
            else if (bossType == BossType.Monje)
            {
                monje.playerIsOnConfiner = true;
                playerStateMachine.isPlayerOnMonjeBossZone = true;
            }

            triggered = true;
            camBoss.Priority = 2;
            invisibleWalls.SetActive(true);
            gameManager.CanMoveParalax(false);
        }
    }

    private void UpdateGameManagerReference()
    {
        if (gameManager == null)
        {
            GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
            if (gameController != null)
            {
                gameManager = gameController.GetComponent<GameManager>();
            }
            else
            {
                Debug.LogError("GameController con tag 'GameController' no encontrado!");
                return;
            }
        }

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
    }

    public void OnBossDefeated()
    {
        if (bossType == BossType.Gorila)
        {
            gorila.playerIsOnConfiner = false;
            if (playerStateMachine != null)
            {
                playerStateMachine.isPlayerOnGorilaBossZone = false;
            }
        }
        else if (bossType == BossType.Monje)
        {
            monje.playerIsOnConfiner = false;
            if (playerStateMachine != null)
            {
                playerStateMachine.isPlayerOnMonjeBossZone = false;
            }
            CombatEvents.PlayerWin(true);
        }

        camBoss.Priority = 0;
        invisibleWalls.SetActive(false);

        if (gameManager != null)
        {
            gameManager.CanMoveParalax(true);
        }
    }

    public void OnPlayerDefeated()
    {
        camBoss.Priority = 0;
        camNormal.Priority = 2;

        if (bossType == BossType.Gorila)
        {
            if (playerStateMachine != null)
            {
                playerStateMachine.isPlayerOnGorilaBossZone = false;
            }
            if (bossSpawnController != null)
            {
                bossSpawnController.StartCoroutine(bossSpawnController.SpawnGorilaBossZone());
            }
            else
            {
                Debug.LogError("BossSpawnController es null! No se puede respawnear la zona del Gorila.");
                // Intentar encontrarlo de nuevo como �ltimo recurso
                bossSpawnController = FindFirstObjectByType<BossSpawnController>();
                if (bossSpawnController != null)
                {
                    bossSpawnController.StartCoroutine(bossSpawnController.SpawnGorilaBossZone());
                }
            }
        }
        else if (bossType == BossType.Monje)
        {
            if(ProgressManager.Instance != null)
            {
                Debug.Log("Reseteando di�logos del Monje en ProgressManager.");
                ProgressManager.Instance.ResetBossDialogues();
            }
            if (playerStateMachine != null)
            {
                playerStateMachine.isPlayerOnMonjeBossZone = false;
            }
            if (bossSpawnController != null)
            {
                bossSpawnController.StartCoroutine(bossSpawnController.SpawnMonjeBossZone());
            }
            else
            {
                Debug.LogError("BossSpawnController es null! No se puede respawnear la zona del Monje.");
                // Intentar encontrarlo de nuevo como �ltimo recurso
                bossSpawnController = FindFirstObjectByType<BossSpawnController>();
                if (bossSpawnController != null)
                {
                    bossSpawnController.StartCoroutine(bossSpawnController.SpawnMonjeBossZone());
                }
            }
        }
    }
}