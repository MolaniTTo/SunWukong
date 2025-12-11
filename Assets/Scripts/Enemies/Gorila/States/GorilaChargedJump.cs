using UnityEngine;

public class GorilaChargedJump : IState
{
    private Gorila gorila;

    public GorilaChargedJump(Gorila gorila)
    {
        this.gorila = gorila;
    }

    public void Enter()
    {
        gorila.lockFacing = true;
        gorila.StopMovement();
        gorila.animator.SetTrigger("ChargedJump");
        if (gorila.gorilaAudioSource != null)
        {
            gorila.gorilaAudioSource.PlayOneShot(gorila.AttackOnda);
        }
        gorila.animationFinished = false;
    }
   
    public void Exit()
    {
        gorila.gorilaAudioSource.Stop();

    }

    public void Update()
    {
        if (gorila.characterHealth.currentHealth <= 0)
        {
            gorila.StateMachine.ChangeState(gorila.DeathState);
            return;
        }
        if (gorila.animationFinished)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
            gorila.animationFinished = false;
        }
    }
}
