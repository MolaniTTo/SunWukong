using UnityEngine;

public interface IState
{
    void Enter(); //metode cridat quan s'entra a l'estat
    void Update(); //metode cridat cada frame mentre s'est� en l'estat
    void Exit(); //metode cridat quan es surt de l'estat

}
