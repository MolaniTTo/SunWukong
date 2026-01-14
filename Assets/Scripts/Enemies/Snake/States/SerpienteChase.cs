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
        snake.PlayHissSound();
    }

    public void Update()
    {
        // Si el jugador muere, volver a patrullar
        if (snake.CheckIfPlayerIsDead())
        {
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        // Si el jugador está en rango de ataque y puede atacar, atacar
        if (snake.IsPlayerInAttackRange() && snake.CanAttack())
        {
            snake.StateMachine.ChangeState(new SerpienteAttack(snake));
            return;
        }

        // Si pierde de vista al jugador, volver a patrullar
        if (!snake.CanSeePlayer())
        {
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        // Si no está en rango de ataque, seguir moviéndose hacia el jugador
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