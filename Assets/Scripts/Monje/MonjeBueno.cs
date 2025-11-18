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
        if (player != null)
        {
            player.hasStaff = true;
        }
    }


}
