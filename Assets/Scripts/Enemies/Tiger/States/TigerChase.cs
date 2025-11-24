public class TigerChase : IState
{
    private EnemyTiger tiger;

    public TigerChase(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetBool("isRunning", true);
        tiger.animator.SetBool("isWalking", false);
    }

    public void Update()
    {
        // Si está en rango de ataque y puede atacar, atacar
        if (tiger.IsPlayerInAttackRange() && tiger.CanAttack())
        {
            tiger.StateMachine.ChangeState(new TigerAttack(tiger));
            return;
        }

        // Si pierde de vista al jugador, volver a idle
        if (!tiger.CanSeePlayer())
        {
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            return;
        }

        // Continuar persiguiendo
        tiger.MoveTowardsPlayer();
    }

    public void Exit()
    {
        tiger.StopMovement();
    }
}
