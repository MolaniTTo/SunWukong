using Unity.VisualScripting;
using UnityEngine;

public class GorilaIdle : IState
{
    private Gorila gorila; //Referencia a l'enemic gorila
    private float idleTimer;
    private float idleDuration = 2f; //temps que porta en idle
    private bool cameFromAttack = false;
    private bool cameFromRun = false;
    private float chaseDistanceThreshold = 4f;
    public GorilaIdle(Gorila gorila)
    {
        this.gorila = gorila; //Assignem la referencia a l'enemic gorila
    }
    public void Enter()
    {
        cameFromAttack = gorila.StateMachine.PreviousState == gorila.PunchState || gorila.StateMachine.PreviousState == gorila.ChargedJumpState;
        cameFromRun = gorila.StateMachine.PreviousState == gorila.RunState;
        idleTimer = 0f;
        gorila.StopMovement();
        //gorila.animator.SetBool("isIdle", true);//Activem la variable de l'animator perque entri a l'estat de idle
        //si el gorila encara no esta despert significa que ve de sleeping i hem d'executar la sequencia de wakeUp
        if (!gorila.hasBeenAwaken) 
        {
            gorila.lockFacing = true; //mentre es desperta no pot girar
            gorila.StartWakeUpSequence(); 
        }
        gorila.lockFacing = false;

        //els altres cops no hem de fer res ja que ja executara el animator la animacio d'idle automaticament (hasexitTime)
    }

    public void Exit()
    {
        //gorila.animator.SetBool("isIdle", false);//Desactivem la variable de l'animator perque surti de l'estat de idle
        //aqui no cal posar res perque l'animator ja canvia automaticament a l'estat de Run o Death

    }

    public void Update()
    {
        if (gorila.CheckIfPlayerIsDead())
        {
            return;
        }

        if(!gorila.hasEnraged && gorila.characterHealth.currentHealth <= gorila.lowHealthThreshold) //si la vida es baixa i encara no esta enrage
        {
            gorila.hasEnraged = true;
            gorila.StateMachine.ChangeState(gorila.EnrageState);
            return;
        }

        //si ve de sleeping no fem res fins que acabi la sequencia de wake up
        if (!gorila.hasBeenAwaken) return;

        if (gorila.hasBeenAwaken) { gorila.Flip(); }

        if (cameFromAttack) //nomes estara 2 segons en idle en cas de que vingui de atac
        {
            if (gorila.IsPlayerTrapped()) 
            { 
                gorila.StateMachine.ChangeState(gorila.RetreatState); //no cal fer el temps de espera si el jugador esta atrapat
                return; 
            } 
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration && gorila.hasBeenAwaken)
            {
                gorila.StateMachine.ChangeState(gorila.RunState);
            }
        }

        float distanceToPlayer = Mathf.Abs(gorila.player.position.x - gorila.transform.position.x);

        if (cameFromRun && distanceToPlayer > chaseDistanceThreshold)
        {
            gorila.StateMachine.ChangeState(gorila.RunState);
            return;
        }

        if (!cameFromAttack && !cameFromRun && gorila.hasBeenAwaken)
        {
            gorila.StateMachine.ChangeState(gorila.RunState);
            return;
        }
    }
}
