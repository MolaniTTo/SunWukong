using UnityEngine;

public class MonjeDeath : IState
{
    private Monje monje;

    public MonjeDeath(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.lockFacing = true;
        monje.StopMovement();
        monje.animator.SetTrigger("Die");
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        
    }
}
