using UnityEngine;

public class TigerIdle : IState
{
    private EnemyTiger tiger;
    private float idleTimer = 0f;
    private float idleDuration = 2f; // Tiempo en idle antes de patrullar

    public TigerIdle(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetBool("isWalking", false);
        tiger.animator.SetBool("isRunning", false);
        tiger.StopMovement();
        idleTimer = 0f;
        
        Debug.Log("Tigre entra en IDLE - facingRight: " + tiger.facingRight);
    }

    public void Update()
    {
        if(tiger.CheckIfPlayerIsDead())
        {
            return; // No hacer nada si el jugador está muerto
        }
        
        // Si detecta al jugador, perseguirlo
        if (tiger.CanSeePlayer())
        {
            tiger.StateMachine.ChangeState(new TigerChase(tiger));
            return;
        }

        // Después de un tiempo en idle, empezar a patrullar
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            // CORRECCIÓN: Antes de patrullar, asegurarse de que está en una orientación válida
            // Verificar si hay suelo delante, si no, girar
            Vector2 frontDirection = tiger.facingRight ? Vector2.right : Vector2.left;
            Vector2 frontGroundCheck = (Vector2)tiger.groundCheck.position + (frontDirection * 0.5f);
            RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, tiger.groundLayer);
            
            // Si no hay suelo delante, girar antes de empezar a patrullar
            if (groundHit.collider == null)
            {
                Debug.Log("IDLE: No hay suelo delante, girando antes de patrullar");
                tiger.Flip();
            }
            
            tiger.StateMachine.ChangeState(new TigerPatrol(tiger));
        }
    }

    public void Exit()
    {
        Debug.Log("Tigre sale de IDLE - facingRight: " + tiger.facingRight);
    }
}