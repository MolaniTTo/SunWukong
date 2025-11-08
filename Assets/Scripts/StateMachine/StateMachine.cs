using UnityEngine;

public class StateMachine //Maquina d'estats que gestiona els diferents estats dels enemics
{
    public IState CurrentState { get; private set; } //Estat actual de la maquina d'estats
    public IState PreviousState { get; private set; } //Estat anterior de la maquina d'estats

    public void Initialize(IState startState) //inicialitza la maquina d'estats amb l'estat inicial
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(IState newState) //canvia l'estat actual per un nou
    {
        PreviousState = CurrentState; //assigna l'estat actual a l'estat anterior
        CurrentState.Exit(); //surt de l'estat actual
        CurrentState = newState; //canvia l'estat actual pel nou estat
        CurrentState.Enter(); //entra al nou estat
    }

    public void Update() //actualitza l'estat actual
    {
        CurrentState?.Update();
    }

}
