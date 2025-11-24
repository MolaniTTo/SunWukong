using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;
    private bool attackExecuted = false;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.StartAttack();
        attackExecuted = false;
    }

    public void Update()
    {
        AnimatorStateInfo stateInfo = snake.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack") && stateInfo.normalizedTime >= 0.9f && !attackExecuted)
        {
            attackExecuted = true;

            if (snake.CanSeePlayer() && snake.IsPlayerInAttackRange() && snake.CanAttack())
                snake.StateMachine.ChangeState(new SerpienteAttack(snake));
            else if (snake.CanSeePlayer())
                snake.StateMachine.ChangeState(new SerpienteChase(snake));
            else
                snake.StateMachine.ChangeState(new SerpientePatrol(snake));
        }
    }

    public void Exit()
    {
        snake.animator.SetBool("isMoving", false);
        snake.animator.SetBool("isChasing", false);
        // No Flip aquí, la dirección se maneja en Patrol o Chase
    }
}
