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
    }

    public void Update()
    {
        if (snake.CanSeePlayer())
        {
            snake.StateMachine.ChangeState(new SerpienteChase(snake));
            return;
        }

        snake.Patrol();
    }

    public void Exit()
    {
        snake.StopMovement();
    }
}