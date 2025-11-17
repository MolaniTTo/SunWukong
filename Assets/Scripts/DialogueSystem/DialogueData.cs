using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

    [System.Serializable]
    public class DialogueLine
    {
        public string text;

        public string animatorTrigger;

        public bool requestCameraZoom = false;

        public bool blockPlayerDuringLine = true;
    }
}