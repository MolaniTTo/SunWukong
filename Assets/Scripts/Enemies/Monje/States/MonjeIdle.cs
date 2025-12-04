using UnityEditor.Tilemaps;
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
        //HA ACABAT EL DIÀLEG I VE DEL IDLE INICIAL, CANVIA A TIRAR RAIG
        if (monje.dialogueFinished && monje.StateMachine.PreviousState == null) //si ha acabat el diàleg i ve del idle inicial el previous state es null
        {
            monje.StateMachine.ChangeState(monje.ThrowRayState); //canvia a l'estat de tirar raig
            return;
        }

        //SI NO HA DE FUGIR TIRA UN ATAC SEGONS EL PATRÓ
        if (!monje.HasToFlee())
        {
            if (monje.attackIndex == 0) //SI VE DE LLENÇAR RAIG
            {
                Debug.Log("Monje switching to Teletransport State from Idle State");
                monje.StateMachine.ChangeState(monje.TeletransportState); //es teletransporta
                return;
            }
            else if (monje.attackIndex == 1) //SI VE DE LLENÇAR TELETRANSPORT
            {
                monje.StateMachine.ChangeState(monje.ThrowRayState); //llença raig
                return;
            }
            else if (monje.attackIndex == 2) //SI VE DE LLENÇAR GAS
            {
                monje.StateMachine.ChangeState(monje.ThrowRayState); //canvia a l'estat de llençar raig
                return;
            }
        }

        //SI HA DE FUGIR CANVIA A L'ESTAT DE FUGIR
        if (monje.HasToFlee())
        {
            monje.StateMachine.ChangeState(monje.RunState);
            return;
        }

        //EM FALTA DEFINIR SI VULL QUE ESPERI UN TEMPS O NO (DE MOMENT NO)


        idleTimer += Time.deltaTime; //afegit per continuar comptant el temps d'idle
    }
}
