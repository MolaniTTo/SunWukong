using UnityEngine;

public class MonjeDeath : IState
{
    private Monje monje;

    public MonjeDeath(Monje monje)
    {
        this.monje = monje;
    }

    public void Enter()
    {
        monje.lockFacing = true;
        monje.LowHealthPrefab.SetActive(false);
        monje.StopMovement();
        monje.animator.SetTrigger("Die");
        if(monje.monjeAudioSource != null)
        {
            monje.monjeAudioSource.PlayOneShot(monje.DeathSound);
        }
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        
    }
}
