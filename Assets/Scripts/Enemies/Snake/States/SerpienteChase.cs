using UnityEngine;

public class SerpienteChase : IState
{
    private EnemySnake snake;
    private float lastRangeCheck = 0f;
    private float rangeCheckInterval = 0.15f;

    public SerpienteChase(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        Debug.Log("[SNAKE CHASE] ENTER");
        snake.animator.SetBool("isChasing", true);
        snake.animator.SetBool("isMoving", false);
        snake.PlayHissSound();
        lastRangeCheck = Time.time;
    }

    public void Update()
    {
        if (snake.CheckIfPlayerIsDead())
        {
            Debug.Log("[SNAKE CHASE] Player died, returning to patrol");
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        if (Time.time - lastRangeCheck >= rangeCheckInterval)
        {
            lastRangeCheck = Time.time;
            
            if (snake.IsPlayerInAttackRange() && snake.CanAttack())
            {
                Debug.Log("[SNAKE CHASE] Player in attack range, changing to ATTACK");
                snake.StateMachine.ChangeState(new SerpienteAttack(snake));
                return;
            }
        }

        if (!snake.CanSeePlayer())
        {
            Debug.Log("[SNAKE CHASE] Lost sight of player, returning to patrol");
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
        Debug.Log("[SNAKE CHASE] EXIT");
        snake.StopMovement();
    }
}