using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;
    private bool attackExecuted = false;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        snake.StartAttack();
        attackExecuted = false;
    }

    public void Update()
    {
        // Seguir la direcci�n del jugador durante el ataque
        TrackPlayer();

        AnimatorStateInfo stateInfo = snake.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack") && stateInfo.normalizedTime >= 0.9f && !attackExecuted)
        {
            attackExecuted = true;

            if (snake.CanSeePlayer() && snake.IsPlayerInAttackRange() && snake.CanAttack())
                snake.StateMachine.ChangeState(new SerpienteAttack(snake));
            else if (snake.CanSeePlayer())
                snake.StateMachine.ChangeState(new SerpienteChase(snake));
            else
                snake.StateMachine.ChangeState(new SerpientePatrol(snake));
        }
    }

    private void TrackPlayer()
    {
        if (snake.Player == null) return;

        // Calcular la direcci�n hacia el jugador
        float directionToPlayer = snake.Player.position.x - snake.transform.position.x;

        /* Si el jugador est� a la derecha y la serpiente mira a la izquierda, voltear
        if (directionToPlayer > 0 && !snake.facingRight)
        {
            snake.Flip();
        }
        // Si el jugador est� a la izquierda y la serpiente mira a la derecha, voltear
        else if (directionToPlayer < 0 && snake.facingRight)
        {
            snake.Flip();
        }*/
    }

    public void Exit()
{
    // Evitar que la animación de walk se reproduzca al salir del ataque
    snake.animator.SetBool("isMoving", false);
    snake.animator.SetBool("isChasing", false);
}

}