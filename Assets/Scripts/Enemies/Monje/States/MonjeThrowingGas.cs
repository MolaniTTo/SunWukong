using Unity.VisualScripting;
using UnityEngine;

public class MonjeThrowingGas : IState
{
    private Monje monje;

    public MonjeThrowingGas(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.lockFacing = true;
        monje.attackIndex = 0; //posem l'index d'atac a 2 (atac de gas)
        monje.animationFinished = false;
        monje.animator.SetTrigger("ThrowGas"); //activem el animator per tirar gas
        monje.monjeAudioSource.PlayOneShot(monje.ThrowToxicGasSound); //reproduim el so de tirar gas
        //desde la animacio de tirar gas es crida amb un animationEvent a un metode que esta al Monje que es diu "ThrowGasAttack"

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
