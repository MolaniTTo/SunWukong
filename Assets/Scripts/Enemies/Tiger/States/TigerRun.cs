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
        Debug.Log("TigerRun: Enter");
        tiger.animator.SetBool("IsRunning", true);
        tiger.animator.SetBool("IsWalking", false);
    }

    public void Update()
    {
        float distanceToPlayer = tiger.GetDistanceToPlayer();
        Debug.Log($"TigerRun: Distancia al jugador = {distanceToPlayer}, Attack Range = {tiger.attackRange}");
        
        // Si el jugador está en rango de ataque y puede atacar
        if (tiger.IsPlayerInAttackRange() && tiger.CanAttack())
        {
            Debug.Log("TigerRun: En rango de ataque, cambio a Attack");
            tiger.StateMachine.ChangeState(new TigerAttack(tiger));
            return;
        }

        // Si pierde de vista al jugador, volver a idle
        if (!tiger.CanSeePlayer() || tiger.GetDistanceToPlayer() > tiger.detectionRange)
        {
            Debug.Log("TigerRun: Perdí al jugador, vuelvo a Idle");
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