using UnityEngine;

public class MonjeIdle : IState
{
    private Monje monje;

    private float idleTimer;
    private float idleDuration = 1f; 

    public MonjeIdle(Monje monje)
    {
        this.monje = monje;
    }
    public void Enter()
    {
        monje.lockFacing = false;
        idleTimer = 0f;
        //monje.animator.SetBool("isIdle", true); //no se si ho fare anar
    }

    public void Exit()
    {
        Debug.Log("Exiting Idle State");
    }

    public void Update()
    {
        //si ha acabat el dialeg, encara amb el mono en idle quiet, li tira un atac de raig (el primer, per fer mal al jugador, es controlara per el codi a part del raig)

        //si ja esta en mode normal i ha de posarse a una distancia prudent surt correns del jugador

        //si esta en mode normal i esta a una distancia prudent del jugador, tira un atac segons el patro d'atacs


        //si esta en mode 
        if (monje.HasToFlee()) //si ha d'anar a fugir canvia a run
        {
            Debug.Log("Monje needs to flee, transitioning to Run State.");
            monje.StateMachine.ChangeState(monje.RunState);
            return;
        }
        /*if (idleTimer >= idleDuration && monje.dialogueFinished) //si ha passat el temps d'idle i ha acabat el diàleg
        {
            Debug.Log("Idle duration passed and dialogue finished, transitioning to attack state.");
            //passa a fer un atac segons el que toqui
            return;

        }*/
        

        idleTimer += Time.deltaTime; //afegit per continuar comptant el temps d'idle

    }
          

  

}
