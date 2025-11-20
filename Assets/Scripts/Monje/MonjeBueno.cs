using UnityEngine;

public class MonjeBueno : MonoBehaviour
{
    [SerializeField] private NPCDialogue monjeDialogue;
    public DialogueData[] dialogue;
    public PlayerStateMachine player;
    public void ChangeDialogue(string name)
    {
        if (monjeDialogue != null && dialogue.Length > 0)
        {
            foreach (var dia in dialogue)
            {
                if (dia.name == name)
                {
                    monjeDialogue.dialogue = dia;
                    Debug.Log("MonjeBueno: Diálogo cambiado a " + name);
                    break;
                }
            }

        }
    }

    public void ActivateStaffToPlayer()
    {
        Debug.Log("MonjeBueno: Intentando activar el bastón para el jugador.");
        if (player != null)
        {
            Debug.Log("MonjeBueno: Activando el bastón para el jugador.");
            player.ActivateStaff();
        }
    }


}
