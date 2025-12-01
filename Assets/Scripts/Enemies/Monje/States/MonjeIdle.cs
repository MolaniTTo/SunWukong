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
        monje.animator.SetBool("isIdle", true); //no se si ho fare anar
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        if (idleTimer >= idleDuration && monje.dialogueFinished)
        {
            //passa a fer un atac depenent del que toqui
        }


    }
          

  

}
