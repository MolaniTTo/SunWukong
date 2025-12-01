using UnityEngine;

public class MonjeRetreating : IState
{
    private Monje monje;

    public MonjeRetreating(Monje monje)
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
