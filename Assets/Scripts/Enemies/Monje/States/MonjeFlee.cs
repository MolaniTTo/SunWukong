using UnityEngine;

public class MonjeFlee : IState
{
    private Monje monje;

    public MonjeFlee(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.animator.SetBool("HasToFlee", true);
    }

    public void Exit()
    {
        monje.animator.SetBool("HasToFlee", false);
    }

    public void Update()
    {
        //si ha de fugir fugeix
        if(monje.HasToFlee()) //mentre ha d'anar a fugir
        {
            monje.Move(); //crida al metode de fugir
            return;
        }

        //si el player esta MOLT aprop, fa el atac de gas

        //si esta lluny del player, torna a idle

        monje.StateMachine.ChangeState(monje.IdleState); //un cop ja no ha de fugir, torna a idle


    }
}
