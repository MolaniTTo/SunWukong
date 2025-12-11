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
        monje.animationFinished = false;

        if (monje.isTeletransportingToFlee)
        {
            monje.animator.SetTrigger("TeletransportToFlee"); //activem el animator per entrar al teletransport des de fugir
            monje.monjeAudioSource.PlayOneShot(monje.TeletransportToFleeSound); //reproduim el so de teletransport
        }
        else
        {
            monje.attackIndex = 2; //posem l'index d'atac a 2 (atac de teletransport)
            monje.animator.SetTrigger("Teletransport"); //activem el animator per entrar al teletransport
            monje.monjeAudioSource.PlayOneShot(monje.TeletransportSound); //reproduim el so de teletransport
        }
    }

    public void Exit()
    {
        if(!monje.isTeletransportingToFlee) //si no s'esta teletransportant per fugir (vol dir que es per atacar) resetejem l'animator
        {
            monje.animator.SetTrigger("ExitTeletransport"); //activem el animator per sortir del teletransport
        }
        monje.isTeletransportingToFlee = false;
        monje.lockFacing = false;
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
