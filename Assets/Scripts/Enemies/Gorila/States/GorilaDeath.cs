using UnityEngine;

public class GorilaDeath : IState
{
    private Gorila gorila;

    public GorilaDeath(Gorila gorila)
    {
        this.gorila = gorila;
    }

    public void Enter()
    {
        gorila.lockFacing = true;
        gorila.StopMovement();
        gorila.animator.SetTrigger("Die");
        if (gorila.gorilaAudioSource != null)
        {
            gorila.gorilaAudioSource.PlayOneShot(gorila.Death);
        }

    }

    public void Exit()
    {
        //no implementat ja que es l'ultim estat
    }

    public void Update()
    {
        //no implementat ja que es l'ultim estat
    }
}
