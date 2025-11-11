using UnityEngine;
public class TigerRun : IState
{
    private EnemyTiger tiger;

    public TigerRun(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetBool("IsRunning", true);
        tiger.animator.SetBool("IsWalking", false);
    }

    public void Update()
    {
        // Si el jugador está en rango de ataque y puede atacar
        if (tiger.IsPlayerInAttackRange() && tiger.CanAttack())
        {
            tiger.StateMachine.ChangeState(new TigerAttack(tiger));
            return;
        }

        // Si pierde de vista al jugador, volver a idle
        if (!tiger.CanSeePlayer() || tiger.GetDistanceToPlayer() > tiger.detectionRange)
        {
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            return;
        }

        // Perseguir al jugador
        tiger.MoveTowards(tiger.runSpeed);
    }

    public void Exit()
    {
        tiger.animator.SetBool("IsRunning", false);
    }
}