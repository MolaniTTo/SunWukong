public class TigerDeath : IState
{
    private EnemyTiger tiger;

    public TigerDeath(EnemyTiger tiger)
    {
        this.tiger = tiger;
    }

    public void Enter()
    {
        tiger.animator.SetTrigger("Death");
        tiger.StopMovement();
        tiger.enabled = false;
    }

    public void Update()
    {
        // No hacer nada, esperar a que se destruya
    }

    public void Exit()
    {
    }
}