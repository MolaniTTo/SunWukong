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
        snake.lockFacing = false;
        snake.animator.SetBool("isChasing", true);
    }

    public void Exit()
    {
        snake.animator.SetBool("isChasing", false);
        snake.StopMovement();
        snake.lockFacing = true;
    }

    public void Update()
    {
        if (snake.HasLostPlayer())
        {
            snake.StateMachine.ChangeState(snake.PatrolState);
            return;
        }

        snake.ChaseMovement();
        snake.Flip();

        if (snake.IsInAttackRange() && snake.CanAttack())
        {
            snake.StateMachine.ChangeState(snake.AttackState);
            return;
        }
    }
}