using UnityEngine;

public class MonjeIdle : IState
{
    private Monje monje;

    private float idleTimer;
    private float idleDuration = 1.5f;

    public MonjeIdle(Monje monje)
    {
        this.monje = monje;
    }
    public void Enter()
    {
        monje.lockFacing = false;
        idleTimer = 0f;
        monje.FacePlayer(); //fa que el monje miri cap al jugador
        //monje.animator.SetBool("isIdle", true); //no se si ho fare anar
    }

    public void Exit()
    {

    }

    public void Update()
    {

        if(monje.CheckIfPlayerIsDead()) //si el jugador està mort fem return
        {
            return;
        }

        //HA ACABAT EL DIÀLEG I VE DEL IDLE INICIAL, CANVIA A TIRAR RAIG
        if (monje.dialogueFinished && monje.StateMachine.PreviousState == null) //si ha acabat el diàleg i ve del idle inicial el previous state es null
        {
            monje.rb.bodyType = RigidbodyType2D.Dynamic; //canvia el rigidbody a dynamic perque es mogui
            monje.StateMachine.ChangeState(monje.ThrowRayState); //canvia a l'estat de tirar raig
            monje.npcDialogue.enabled = false; //desactiva el diàleg
            return;
        }

        //COMPROVA SI POT ATACAR

        float dist = Vector2.Distance(monje.transform.position, monje.player.position); //agafa la distancia al jugador
        bool optimalToAttack = dist > monje.minDistanceToFlee; //comprova si està en distancia òptima per atacar (o sigui, fora de la distancia de fugir)

        if (!monje.HasToFlee() && optimalToAttack && idleDuration <= idleTimer && monje.dialogueFinished) //SI NO HA DE FUGIR I ESTÀ EN DISTANCIA ÒPTIMA PER ATACAR I HA PASSAT EL TEMPS D'IDLE
        {
            if (monje.attackIndex == 0) //SI VE DE LLENÇAR RAIG
            {
                Debug.Log("Monje switching to Teletransport State from Idle State");
                monje.StateMachine.ChangeState(monje.ThrowRayState); //tira un raig
                return;
            }
            else if (monje.attackIndex == 1 && monje.raysFinished) //Si ve de tirar raig i ja ha acabat tots els raigs
            {
                monje.StateMachine.ChangeState(monje.TeletransportState); //es teltransporta
                return;
            }
            else if (monje.attackIndex == 2) //SI VE de teletransportarse
            {
                monje.StateMachine.ChangeState(monje.ThrowGasState); //canvia a l'estat de tirar gas
                return;
            }
        }

        //SI HA DE FUGIR CANVIA A L'ESTAT DE FUGIR
        if (monje.HasToFlee() && monje.dialogueFinished)
        {
            monje.StateMachine.ChangeState(monje.RunState);
            return;
        }

        idleTimer += Time.deltaTime; //afegit per continuar comptant el temps d'idle
    }
}
