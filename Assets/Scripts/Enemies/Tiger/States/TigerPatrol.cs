using UnityEngine;

public class TigerPatrol : IState
{
    private EnemyTiger tiger;
    private bool hasInitialized = false;

    public TigerPatrol(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetBool("isWalking", true);
        tiger.animator.SetBool("isRunning", false);
        hasInitialized = false;
        
        Debug.Log("Tigre entra en PATROL - facingRight: " + tiger.facingRight + " Posición: " + tiger.transform.position.x);
    }

    public void Update()
    {
        if (tiger.CheckIfPlayerIsDead())
        {
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
            return;
        }
        
        // Si detecta al jugador, perseguirlo
        if (tiger.CanSeePlayer())
        {
            tiger.StateMachine.ChangeState(new TigerChase(tiger));
            return;
        }

        // CORRECCIÓN: En el primer frame de patrol, verificar orientación
        if (!hasInitialized)
        {
            hasInitialized = true;
            
            // Verificar si hay suelo delante
            Vector2 frontDirection = tiger.facingRight ? Vector2.right : Vector2.left;
            Vector2 frontGroundCheck = (Vector2)tiger.groundCheck.position + (frontDirection * 0.5f);
            RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, tiger.groundLayer);
            
            // Si no hay suelo o hay pared, girar inmediatamente
            Vector2 wallDirection = tiger.facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(tiger.wallCheck.position, wallDirection, tiger.wallCheckDistance, tiger.groundLayer);
            
            if (groundHit.collider == null || wallHit.collider != null)
            {
                Debug.Log("PATROL: Corrigiendo orientación al iniciar patrulla");
                tiger.Flip();
            }
        }

        // Continuar patrullando
        tiger.Patrol();
    }

    public void Exit()
    {
        tiger.StopMovement();
        Debug.Log("Tigre sale de PATROL");
    }
}