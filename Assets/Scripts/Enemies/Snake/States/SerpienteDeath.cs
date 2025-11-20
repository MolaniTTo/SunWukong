using UnityEngine;

public class SerpienteDeath : IState
{
    private EnemySnake snake;

    public SerpienteDeath(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.lockFacing = true;
        snake.StopMovement();
        snake.animator.SetTrigger("Die");
    }

    public void Exit()
    {
        //no implementat ja que es l'ultim estat
    }

    public void Update()
    {
        //no implementat ja que es l'ultim estat
    }
}