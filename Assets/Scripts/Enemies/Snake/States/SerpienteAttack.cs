using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;
    private bool stateExiting = false;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        stateExiting = false;
        // Mirar al jugador al empezar
        if (snake.Player != null)
        {
            float dir = snake.Player.position.x - snake.transform.position.x;
            if (dir > 0 && !snake.facingRight) snake.Flip();
            else if (dir < 0 && snake.facingRight) snake.Flip();
        }

        snake.StartAttack();
        snake.StopHissSound();
    }

    public void Update()
    {
        if (stateExiting) return;

        // Si el mono sale del CÍRCULO ROJO, cortamos el ataque al instante
        if (!snake.IsPlayerInAttackRange())
        {
            ExitToMovement();
            return;
        }

        AnimatorStateInfo stateInfo = snake.animator.GetCurrentAnimatorStateInfo(0);

        // Si sigue en el círculo y terminó la animación, atacamos de nuevo
        if (stateInfo.IsName("attack") && stateInfo.normalizedTime >= 0.95f)
        {
            if (snake.CanAttack())
            {
                snake.StartAttack();
            }
        }
    }

private void ExitToMovement()
{
    stateExiting = true;
    
    // Resetear estado de ataque
    snake.OnAttackEnd();
    if (snake.biteCollider != null) 
        snake.biteCollider.SetActive(false);
    
    // Limpiar todos los triggers y forzar salida
    snake.animator.ResetTrigger("Attack");
    snake.animator.ResetTrigger("Damaged");
    
    // CLAVE: Forzar la reproducción de otro estado inmediatamente
    if (snake.CanSeePlayer())
    {
        snake.animator.SetBool("isMoving", false);
        snake.animator.SetBool("isChasing", true);
        // Forzar la transición inmediatamente
        snake.animator.Play("walk", 0, 0f); // Reproduce walk desde el inicio
        snake.StateMachine.ChangeState(new SerpienteChase(snake));
    }
    else
    {
        snake.animator.SetBool("isChasing", false);
        snake.animator.SetBool("isMoving", true);
        // Forzar la transición inmediatamente
        snake.animator.Play("walk", 0, 0f); // Reproduce walk desde el inicio
        snake.StateMachine.ChangeState(new SerpientePatrol(snake));
    }
}

    public void Exit()
    {
        snake.animator.ResetTrigger("Attack");
    }
}