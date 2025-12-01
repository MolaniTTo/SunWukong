using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GorilaPunchAttack : IState
{
    private Gorila gorila; //Referencia a l'enemic gorila

    public GorilaPunchAttack(Gorila gorila)
    {
        this.gorila = gorila; //Assignem la referencia a l'enemic gorila
    }

    public void Enter()
    {
        gorila.lockFacing = true; //Bloquejem la direccio del gorila
        gorila.StopMovement(); //Aturem el moviment del gorila
        gorila.animator.SetTrigger("Punch"); //Activem la variable de l'animator perque entri a l'estat de punch attack
        gorila.animationFinished = false;
    }

    public void Exit()
    {
        
    }
    public void Update()
    {
        if(gorila.CheckIfPlayerIsDead())
        {
            gorila.StateMachine.ChangeState(gorila.IdleState); //Si el jugador està mort, canviem a l'estat d'idle
            return;
        }

        if (gorila.animationFinished)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
            gorila.animationFinished = false;
            return;
        }
    }
}
