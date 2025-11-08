using UnityEngine;

public class GorilaSleeping : IState
{
    private Gorila gorila; //Referencia a l'enemic gorila

    public GorilaSleeping(Gorila gorila)
    {
        this.gorila = gorila; //Assignem la referencia a l'enemic gorila
    }
    public void Enter()
    {
        
    }

    public void Exit()
    {

    }

    public void Update()
    {
        //aqui hem de posar alguna cosa per veure si el confiner s'activa, per posar la animacio de WakeUp i canviar l'estat a GorilaIdle
        if(gorila.playerIsOnConfiner)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
        }
    }
}
