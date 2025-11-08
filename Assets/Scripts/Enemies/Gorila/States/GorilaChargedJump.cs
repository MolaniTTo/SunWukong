using UnityEngine;

public class GorilaChargedJump : IState
{
    private Gorila gorila;

    public GorilaChargedJump(Gorila gorila)
    {
        this.gorila = gorila;
    }

    public void Enter()
    {
        gorila.lockFacing = true;
        gorila.StopMovement();
        gorila.animator.SetTrigger("ChargedJump");
        gorila.animationFinished = false;
    }
   
    public void Exit()
    {

    }

    public void Update()
    {
        if (gorila.health <= 0)
        {
            gorila.StateMachine.ChangeState(gorila.DeathState);
            return;
        }
        if (gorila.animationFinished)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
            gorila.animationFinished = false;
        }
    }
}
