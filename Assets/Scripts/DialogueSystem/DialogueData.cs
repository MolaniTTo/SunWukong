using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public bool exitDialogueModeByScripting = false;
    public bool onlyOnce = false;
    public bool hasBeenUsed = false;
    public bool changeMusic = true;
    public string dialogueMusicKey = "Dialogue1"; //nom de la musica de dialogo

    public DialogueLine[] lines;

    [System.Serializable]
    public class DialogueLine
    {
        public string text;

        public string animatorTrigger;

        public bool requestCameraZoom = false;

        public bool blockPlayerDuringLine = true;

        public bool deactivateObjects = false;

    }
}