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
        if(monje.monjeAudioSource != null)
        {
            //pone en bucle el so de fugida
            monje.monjeAudioSource.clip = monje.RunSound;
            monje.monjeAudioSource.loop = true;
            monje.monjeAudioSource.Play();

        }
    }

    public void Exit()
    {
        monje.animator.SetBool("HasToFlee", false);
        monje.StopMovement();
        monje.monjeAudioSource.Stop();
    }

    public void Update()
    {
        //COMPROVAR SI EL JUGADOR ESTÀ MORT
        if (monje.CheckIfPlayerIsDead()) //si el jugador està mort
        {
            monje.StateMachine.ChangeState(monje.IdleState); //canvia a l'estat d'idle
            return;
        }

        //CHECKEJAR EN CAS DE ESTAT CRITIC DE FUGIR O ATRAPAT A PROP D'UNA PARET

        monje.Flip(); //fa que el monje miri cap al jugador

        //Comprovem el estat critic de fugir i si està atrapat a prop d'una paret
        float dist = Vector2.Distance(monje.transform.position, monje.player.position); //agafa la distancia al jugador
        bool critical = monje.CriticalFleeState(); //comprova si està en estat crític de fugir
        bool trappedAndPlayerClose = monje.IsNearWall() && dist < monje.minDistanceToFlee; //comprova si està atrapat a prop d'una paret i el jugador està a prop

        if(critical || trappedAndPlayerClose) //si està en estat crític de fugir o està atrapat a prop d'una paret i el jugador està a prop
        {
            monje.isTeletransportingToFlee = true; //posa la variable de teletransportar-se a fugir a true
            monje.animator.SetTrigger("TeletransportToFlee"); //activa l'animator de fugir crític
            monje.StateMachine.ChangeState(monje.TeletransportState); //canvia a l'estat de teletransportar-se
            return;
        }

        //FUGIR MENTRE HA D'ANAR A FUGIR

        if (monje.HasToFlee()) //mentre ha d'anar a fugir
        {
            monje.Move(); //crida al metode de fugir
            return;
        }

        //TORNAR A IDLE QUAN JA NO HA DE FUGIR

        if (!monje.HasToFlee()) //mentre ha d'anar a fugir
        {
            monje.StateMachine.ChangeState(monje.IdleState); //torna a l'estat d'idle
        }
    }
}
