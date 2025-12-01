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
        throw new System.NotImplementedException();
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
