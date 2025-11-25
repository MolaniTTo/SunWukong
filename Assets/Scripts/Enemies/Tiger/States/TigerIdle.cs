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
    }

    public void Update()
    {
        if(tiger.CheckIfPlayerIsDeath()) //si el jugador esta mort
        {
            return;
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
            tiger.StateMachine.ChangeState(new TigerPatrol(tiger));
        }
    }

    public void Exit()
    {
        // Nada especial al salir
    }
}