using UnityEngine;
public class TigerWalk : IState
{
    private EnemyTiger tiger;

    public TigerWalk(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        Debug.Log("TigerWalk: Enter");
        tiger.animator.SetBool("IsWalking", true);
        tiger.animator.SetBool("IsRunning", false);
    }

    public void Update()
    {
        // Si ve al jugador, cambiar a correr
        if (tiger.CanSeePlayer() && tiger.GetDistanceToPlayer() <= tiger.detectionRange)
        {
            Debug.Log("TigerWalk: Veo al jugador, cambio a Run");
            tiger.StateMachine.ChangeState(new TigerRun(tiger));
            return;
        }

        // Moverse en la dirección actual
        tiger.MoveTowards(tiger.walkSpeed);

        // Comprobar si debe girar (temporalmente sin check de suelo)
        bool atBoundary = tiger.IsAtPatrolBoundary();
        bool wallAhead = tiger.IsWallAhead();
        
        if (atBoundary)
        {
            Debug.Log("TigerWalk: En límite de patrulla, vuelvo a Idle");
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
        }
        else if (wallAhead)
        {
            Debug.Log("TigerWalk: Pared detectada, vuelvo a Idle");
            tiger.StateMachine.ChangeState(new TigerIdle(tiger));
        }
        // Comentado temporalmente el check de suelo
        //else if (!grounded)
        //{
        //    Debug.Log("TigerWalk: No estoy en el suelo, vuelvo a Idle");
        //    tiger.StateMachine.ChangeState(new TigerIdle(tiger));
        //}
    }

    public void Exit()
    {
        tiger.animator.SetBool("IsWalking", false);
    }
}
