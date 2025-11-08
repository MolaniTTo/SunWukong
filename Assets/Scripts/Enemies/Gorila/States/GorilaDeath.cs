using UnityEngine;

public class GorilaDeath : IState
{
    private Gorila gorila;

    public GorilaDeath(Gorila gorila)
    {
        this.gorila = gorila;
    }

    public void Enter()
    {
        gorila.lockFacing = true;
        gorila.StopMovement();
        gorila.animator.SetTrigger("Die");

    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}
