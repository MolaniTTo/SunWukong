using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.lockFacing = true;
        snake.StopMovement();
        snake.animator.SetTrigger("Attack");
        snake.animationFinished = false;
    }

    public void Exit()
    {

    }

    public void Update()
    {
        if (snake.animationFinished)
        {
            if (snake.CanSeePlayer())
            {
                snake.StateMachine.ChangeState(snake.ChaseState);
            }
            else
            {
                snake.StateMachine.ChangeState(snake.PatrolState);
            }
            snake.animationFinished = false;
            return;
        }
    }
}