using UnityEngine;
public class TigerAttack : IState
{
    private EnemyTiger tiger;
    private bool hasAttacked;

    public TigerAttack(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetTrigger("Attack");
        tiger.StopMovement();
        hasAttacked = false;
    }

    public void Update()
    {
        // El método Attack() se llama desde el AnimationEvent
        // Aquí solo esperamos a que termine la animación

        // Comprobar si la animación de ataque ha terminado
        AnimatorStateInfo stateInfo = tiger.animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 0.95f)
        {
            // Si el jugador sigue cerca, volver a atacar
            if (tiger.IsPlayerInAttackRange() && tiger.CanAttack())
            {
                tiger.StateMachine.ChangeState(new TigerAttack(tiger));
            }
            // Si el jugador está lejos pero visible, perseguir
            else if (tiger.CanSeePlayer() && tiger.GetDistanceToPlayer() <= tiger.detectionRange)
            {
                tiger.StateMachine.ChangeState(new TigerRun(tiger));
            }
            // Si no hay jugador, volver a idle
            else
            {
                tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            }
        }
    }

    public void Exit()
    {
    }
}