using UnityEngine;
public class TigerIdle : IState
{
    private EnemyTiger tiger;
    private float idleTimer;
    private float idleTime = 2f;

    public TigerIdle(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        Debug.Log("TigerIdle: Enter");
        tiger.animator.SetBool("IsWalking", false);
        tiger.animator.SetBool("IsRunning", false);
        tiger.StopMovement();
        idleTimer = 0f;
    }

    public void Update()
    {
        // Si ve al jugador, cambiar a correr
        if (tiger.CanSeePlayer() && tiger.GetDistanceToPlayer() <= tiger.detectionRange)
        {
            Debug.Log("TigerIdle: Veo al jugador, cambio a Run");
            tiger.StateMachine.ChangeState(new TigerRun(tiger));
            return;
        }

        // Después de un tiempo idle, empezar a patrullar
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            Debug.Log("TigerIdle: Tiempo cumplido, cambio a Walk");
            tiger.StateMachine.ChangeState(new TigerWalk(tiger));
        }
    }

    public void Exit()
    {
    }
}