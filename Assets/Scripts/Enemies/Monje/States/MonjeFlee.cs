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
        monje.Flip(); //fa que el monje miri cap al jugador mentre fuig
        //si ha de fugir fugeix
        if (monje.HasToFlee()) //mentre ha d'anar a fugir
        {
            monje.Move(); //crida al metode de fugir
            return;
        }

        //si el player esta MOLT aprop, es gira cap al player i li llança una bola de gas

        //si esta lluny del player, torna a idle
    }
}
