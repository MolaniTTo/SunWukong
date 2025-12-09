using UnityEngine;

public class GorilaEnrage : IState
{
    private Gorila gorila;
    private bool animationFinished = false;

    public GorilaEnrage(Gorila gorila)
    {
        this.gorila = gorila;
    }

    public void Enter()
    {
        animationFinished = false;
        gorila.StopMovement();
        gorila.lockFacing = true;
        gorila.animator.SetTrigger("Enrage"); 
    }

    public void Exit()
    {
        gorila.lockFacing = false;
    }

    public void Update()
    {
        if (animationFinished)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
        }
    }

    public void OnEnrageAnimationFinished()
    {
        animationFinished = true;
    }
}
