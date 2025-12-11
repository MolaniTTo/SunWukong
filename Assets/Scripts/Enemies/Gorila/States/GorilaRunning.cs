using UnityEngine;

public class GorilaRunning : IState
{
    private Gorila gorila; //Referencia a l'enemic gorila
    private float distanceToPlayer; //Distancia al jugador segons el atac

    public GorilaRunning(Gorila gorila)
    {
        this.gorila = gorila; //Assignem la referencia a l'enemic gorila
    }
    public void Enter()
    {
        gorila.lockFacing = false; //Desbloquejem la direccio del gorila
        gorila.animator.SetBool("isRunning", true);//Activem la variable de l'animator perque entri a l'estat de run
    }

    public void Exit()
    {
        gorila.animator.SetBool("isRunning", false);//Desactivem la variable de l'animator perque surti de l'estat de run
        gorila.StopMovement(); //Aturem el moviment del gorila en sortir de l'estat de run
        gorila.lockFacing = true; //Bloquejem la direccio del gorila
        gorila.gorilaAudioSource.Stop(); //Aturem l'audio de caminar en sortir de l'estat de run
    }

    public void Update()
    {
        if (gorila.CheckIfPlayerIsDead())
        {
            gorila.StateMachine.ChangeState(gorila.IdleState); //Si el jugador està mort, canviem a l'estat d'idle
            return;
        }

        if(!gorila.hasEnraged && gorila.characterHealth.currentHealth <= gorila.lowHealthThreshold) //si la vida es baixa i encara no esta enrage
        {
            gorila.hasEnraged = true;
            gorila.StateMachine.ChangeState(gorila.EnrageState);
            return;
        }

        bool isMoving = gorila.Movement(); //Fem que el gorila es mogui cap al jugador i retornem si s'està movent o no
        
        if(gorila.gorilaAudioSource != null && !gorila.gorilaAudioSource.isPlaying)
        {
            gorila.gorilaAudioSource.clip = gorila.Walk;
            gorila.gorilaAudioSource.loop = true;
            gorila.gorilaAudioSource.Play();
        }

        gorila.Flip(); //Assegurem que el gorila miri cap al jugador mentre esta corrent


        if (!isMoving) //si no s'està movent perque esta massa aprop del jugador, canviem a l'estat d'idle
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
            return;
        }

        Vector2 origin = gorila.transform.position;
        Vector2 dir = Vector2.right * gorila.facingDirection; //direccio del raycast segons cap a on miri el gorila


        RaycastHit2D shortHit = Physics2D.Raycast(origin, dir, 4f, LayerMask.GetMask("Player")); //capa del jugador
        RaycastHit2D longHit = Physics2D.Raycast(origin, dir, 8f, LayerMask.GetMask("Player")); //capa del jugador

        Debug.DrawRay(origin, dir * 4f, Color.green);
        Debug.DrawRay(origin, dir * 8f, Color.red);


        if (shortHit.collider != null && gorila.punchCounter < gorila.punchsBeforeCharged) //si el raycast curt ha colisionat amb el jugador
        {
            gorila.StateMachine.ChangeState(gorila.PunchState); //activem atac curt
            return;
        }

        if(longHit.collider != null && gorila.punchCounter == gorila.punchsBeforeCharged) //si el raycast llarg ha colisionat amb el jugador i ha fet els atacs normals suficients
        {
            gorila.StateMachine.ChangeState(gorila.ChargedJumpState); //activem atac llarg
            gorila.punchCounter = 0; //resetejem el contador d'atacs normals
            return;
        }
    }
}
