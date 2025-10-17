using UnityEngine;

public abstract class EnemyBase : MonoBehaviour //CLASSE PARE DE TOTS ELS ENEMICS
{
    public StateMachine StateMachine { get; private set; } //Encapsulament de la maquina d'estats

    protected virtual void Awake() //nomes es pot sobreescriure desde les classes hereditaries
    {
        StateMachine = new StateMachine(); //inicialitzem la maquina d'estats
    }

    protected virtual void Update()
    {
        StateMachine.Update();
    }

    //metodes comuns dels enemics

    public abstract bool CanSeePlayer(); //metode abstracte que hauran de implementar les classes filles per determinar si poden veure el jugador
    public abstract void Move(); //moure l'enemic
    public abstract void Attack(); //atacar el jugador
    public abstract void Die(); //morir 

}
