using UnityEngine;

public class TigerAttack : IState
{
    private EnemyTiger tiger;
    private bool attackExecuted = false;

    public TigerAttack(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.StartAttack();
        attackExecuted = false;
    }

    public void Update()
    {
        if (tiger.CheckIfPlayerIsDead())
        {
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            return;
        }
        
        // Esperar a que termine la animación de ataque
        AnimatorStateInfo stateInfo = tiger.animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Attack"))
        {
            // Si la animación está casi completa, volver a perseguir o idle
            if (stateInfo.normalizedTime >= 0.9f && !attackExecuted)
            {
                attackExecuted = true;
                
                // Decidir siguiente estado
                if (tiger.CanSeePlayer() && tiger.IsPlayerInAttackRange())
                {
                    // Seguir atacando si el jugador sigue cerca
                    if (tiger.CanAttack())
                    {
                        tiger.StateMachine.ChangeState(new TigerAttack(tiger));
                    }
                    else
                    {
                        tiger.StateMachine.ChangeState(new TigerChase(tiger));
                    }
                }
                else if (tiger.CanSeePlayer())
                {
                    // Si ve al jugador pero está lejos, perseguir
                    tiger.StateMachine.ChangeState(new TigerChase(tiger));
                }
                else
                {
                    // Si no ve al jugador, volver a idle
                    tiger.StateMachine.ChangeState(new TigerIdle(tiger));
                }
            }
        }
    }

    public void Exit()
    {
        // Nada especial al salir
    }
}