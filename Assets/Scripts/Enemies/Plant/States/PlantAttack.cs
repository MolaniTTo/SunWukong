using UnityEngine;

public class PlantAttack : IState
{
    private EnemyPlant enemyPlant; //Referencia a l'enemic planta

    public PlantAttack(EnemyPlant enemyPlant)
    {
        this.enemyPlant = enemyPlant; //Assignem la referencia a l'enemic planta
    }

    public void Enter()
    {
        //no cal fer res en aquest metode per ara ja que l'animator ja esta en estat d'atac per defecte
    }

    public void Exit()
    {
        enemyPlant.animator.SetBool("CanSeePlayer", false); //Quan sortim de l'estat d'atac, indiquem a l'animator que no pot veure el jugador
    }

    public void Update()
    {
        if (!enemyPlant.CanSeePlayer() || enemyPlant.CheckIfPlayerIsDeath()) //si retorna false o el jugador esta mort
        {
            enemyPlant.StateMachine.ChangeState(new PlantIdle(enemyPlant)); //Canviem a l'estat de idle
        }
    }
}
