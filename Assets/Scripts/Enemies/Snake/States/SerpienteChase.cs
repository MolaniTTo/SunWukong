using UnityEngine;

public class SerpienteChase : IState
{
    private EnemySnake snake;

    public SerpienteChase(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.animator.SetBool("isChasing", true);
        snake.animator.SetBool("isMoving", false);
    }

    public void Update()
    {
        if (snake.IsPlayerInAttackRange() && snake.CanAttack())
        {
            snake.StateMachine.ChangeState(new SerpienteAttack(snake));
            return;
        }

        if (!snake.CanSeePlayer())
        {
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        if (!snake.IsPlayerInAttackRange())
        {
            snake.MoveTowardsPlayer();
        }
        else
        {
            snake.StopMovement();
        }
    }

    public void Exit()
    {
        snake.StopMovement();
    }
}