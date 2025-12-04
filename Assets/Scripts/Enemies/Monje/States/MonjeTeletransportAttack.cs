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
        monje.attackIndex = 1; //posem l'index d'atac a 1 (atac de teletransport)
        monje.animationFinished = false;
        monje.animator.SetTrigger("Teletransport"); //activem el animator per teletransportar-se i atacar
    }

    public void Exit()
    {
        monje.lockFacing = false;
        monje.animator.SetTrigger("ExitTeletransport"); //activem el animator per sortir del teletransport
    }

    public void Update()
    {
        if (monje.CheckIfPlayerIsDead())
        {
            monje.StateMachine.ChangeState(monje.IdleState); //Si el jugador està mort, canviem a l'estat d'idle
            return;
        }
        if (monje.animationFinished)
        {
            monje.StateMachine.ChangeState(monje.IdleState);
            monje.animationFinished = false;
            return;
        }
    }
}
