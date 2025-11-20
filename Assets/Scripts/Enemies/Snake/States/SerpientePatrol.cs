using UnityEngine;

public class SerpientePatrol : IState
{
    private EnemySnake snake;

    public SerpientePatrol(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.lockFacing = false;
        snake.animator.SetBool("isMoving", true);
    }

    public void Exit()
    {
        snake.animator.SetBool("isMoving", false);
        snake.StopMovement();
    }

    public void Update()
    {
        if (snake.CanSeePlayer())
        {
            snake.StateMachine.ChangeState(snake.ChaseState);
            return;
        }

        snake.PatrolMovement();
    }
}