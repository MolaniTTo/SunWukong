using UnityEngine;

public class MonjeTeletransportAttack : IState
{
    private Monje monje;

    public MonjeTeletransportAttack(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.lockFacing = true;
    }

    public void Exit()
    {
        monje.lockFacing = false;
    }

    public void Update()
    {
        //nose si haig de mirar alguna cosa a part de si es mor.
        //nose si haig de controlar desde aqui el verticalVelocity per per el blending de les animacions

    }
}
