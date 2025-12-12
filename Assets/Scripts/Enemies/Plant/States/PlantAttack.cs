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
        enemyPlant.audioSource.PlayOneShot(enemyPlant.InRangeSound); //Reproduim el so de detectar el jugador
        //no cal fer res en aquest metode per ara ja que l'animator ja esta en estat d'atac per defecte
    }

    public void Exit()
    {
        enemyPlant.animator.SetBool("CanSeePlayer", false); //Quan sortim de l'estat d'atac, indiquem a l'animator que no pot veure el jugador
        enemyPlant.audioSource.PlayOneShot(enemyPlant.OutOfRangeSound); //Reproduim el so de perdre el jugador
    }

    public void Update()
    {
        if (enemyPlant.CheckIfPlayerIsDead())
        {
            enemyPlant.StateMachine.ChangeState(new PlantIdle(enemyPlant)); //Canviem a l'estat d'idle si el jugador esta mort
            return;
        }
        if (!enemyPlant.CanSeePlayer()) //si retorna false
        {
            enemyPlant.StateMachine.ChangeState(new PlantIdle(enemyPlant)); //Canviem a l'estat d'idle si no pot veure el jugador
        }
    }
}
