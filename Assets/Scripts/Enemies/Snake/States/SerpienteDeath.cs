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
        snake.StopMovement();
        snake.animator.SetTrigger("Die");

        // Destruir despu�s de 1 segundo
        snake.Invoke(nameof(DestroySelf), 1f);
    }

    public void Exit()
    {
        // No se usa porque es el �ltimo estado
    }

    public void Update()
    {
        // No se usa
    }

    private void DestroySelf()
    {
        GameObject.Destroy(snake.gameObject);
    }
}
