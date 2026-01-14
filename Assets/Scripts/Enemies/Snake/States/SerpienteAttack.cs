using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;
    private bool hasExited = false;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        hasExited = false;

        // Verificar si el jugador está muerto antes de atacar
        if (snake.CheckIfPlayerIsDead())
        {
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        // Orientar hacia el jugador
        if (snake.Player != null)
        {
            float dir = snake.Player.position.x - snake.transform.position.x;
            if (dir > 0 && !snake.facingRight) 
                snake.Flip();
            else if (dir < 0 && snake.facingRight) 
                snake.Flip();
        }

        snake.StartAttack();
        snake.StopHissSound();
    }

    public void Update()
    {
        if (hasExited) return;

        // Si el jugador muere durante el ataque, volver a patrullar
        if (snake.CheckIfPlayerIsDead())
        {
            ExitToPatrol();
            return;
        }

        // Si el jugador sale del rango de ataque, cambiar estado
        if (!snake.IsPlayerInAttackRange())
        {
            ExitToMovement();
            return;
        }

        // Verificar si la animación de ataque ha terminado
        AnimatorStateInfo stateInfo = snake.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("attack") && stateInfo.normalizedTime >= 0.95f)
        {
            // Si terminó la animación y aún puede atacar, reiniciar ataque
            if (snake.CanAttack() && snake.IsPlayerInAttackRange())
            {
                snake.StartAttack();
            }
        }
    }

    private void ExitToMovement()
    {
        if (hasExited) return;
        hasExited = true;

        // Resetear estado de ataque
        snake.OnAttackEnd();
        if (snake.biteCollider != null)
            snake.biteCollider.SetActive(false);

        // Limpiar triggers
        snake.animator.ResetTrigger("Attack");
        snake.animator.ResetTrigger("Damaged");

        // Decidir siguiente estado según si ve al jugador
        if (snake.CanSeePlayer())
        {
            snake.animator.SetBool("isMoving", false);
            snake.animator.SetBool("isChasing", true);
            snake.animator.Play("walk", 0, 0f);
            snake.StateMachine.ChangeState(new SerpienteChase(snake));
        }
        else
        {
            ExitToPatrol();
        }
    }

    private void ExitToPatrol()
    {
        if (hasExited) return;
        hasExited = true;

        snake.OnAttackEnd();
        if (snake.biteCollider != null)
            snake.biteCollider.SetActive(false);

        snake.animator.ResetTrigger("Attack");
        snake.animator.ResetTrigger("Damaged");
        snake.animator.SetBool("isChasing", false);
        snake.animator.SetBool("isMoving", true);
        snake.animator.Play("walk", 0, 0f);
        
        snake.StateMachine.ChangeState(new SerpientePatrol(snake));
    }

    public void Exit()
    {
        snake.animator.ResetTrigger("Attack");
    }
}