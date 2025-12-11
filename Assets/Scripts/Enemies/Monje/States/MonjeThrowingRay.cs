using UnityEngine;

public class MonjeThrowingRay : IState
{
    private Monje monje;

    public MonjeThrowingRay(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.lockFacing = true; //es innecesari de moment pq nomes crido a Flip() desde els estats que ho necessiten
        monje.attackIndex = 1; //posem l'index d'atac a 0 (atac de raig)
        monje.animationFinished = false;
        monje.raysFinished = false;
        monje.animator.SetTrigger("ThrowRay"); //activem el animator per tirar el raig
        monje.monjeAudioSource.PlayOneShot(monje.ThrowLightningSound); //reproduim el so de tirar el raig
    }

    public void Exit()
    {

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
