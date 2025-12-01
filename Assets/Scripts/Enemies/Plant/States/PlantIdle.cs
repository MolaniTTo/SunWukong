using UnityEngine;

public class PlantIdle : IState
{
    private EnemyPlant enemyPlant; //Referencia a l'enemic planta

    public PlantIdle(EnemyPlant enemyPlant)
    {
        this.enemyPlant = enemyPlant; //Assignem la referencia a l'enemic planta
    }

    public void Enter()
    {
        //No cal fer res en aquest mètode per ara ja que l'animator ja està en estat d'idle per defecte
    }

    public void Exit()
    {
        enemyPlant.animator.SetBool("CanSeePlayer", true); //Quan sortim de l'estat d'idle, indiquem a l'animator que pot veure el jugador
    }

    public void Update()
    {
        if(enemyPlant.CheckIfPlayerIsDead())
        {
            return; //No fem res si el jugador està mort
        }
        if (enemyPlant.CanSeePlayer())
        {
            enemyPlant.StateMachine.ChangeState(new PlantAttack(enemyPlant)); //Canviem a l'estat d'atac si pot veure el jugador
        }
    }
}
