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
        snake.animator.SetBool("isMoving", true);
        snake.animator.SetBool("isChasing", false);
        snake.StopHissSound();
    }

    public void Update()
    {
        // Si detecta al jugador y no está muerto, perseguirlo
        if (snake.CanSeePlayer() && !snake.CheckIfPlayerIsDead())
        {
            snake.StateMachine.ChangeState(new SerpienteChase(snake));
            return;
        }

        // Continuar patrullando
        snake.Patrol();
    }

    public void Exit()
    {
        snake.StopMovement();
    }
}