using UnityEngine;

public class TigerPatrol : IState
{
    private EnemyTiger tiger;

    public TigerPatrol(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetBool("isWalking", true);
        tiger.animator.SetBool("isRunning", false);
        
        // SOLUCIÓN: Sincronizar la dirección de movimiento con la dirección visual
        tiger.SyncMovementDirection();
    }

    public void Update()
    {
        if (tiger.CheckIfPlayerIsDead())
        {
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            return;
        }
        
        // Si detecta al jugador, perseguirlo
        if (tiger.CanSeePlayer())
        {
            tiger.StateMachine.ChangeState(new TigerChase(tiger));
            return;
        }

        // Continuar patrullando
        tiger.Patrol();
    }

    public void Exit()
    {
        tiger.StopMovement();
    }
}