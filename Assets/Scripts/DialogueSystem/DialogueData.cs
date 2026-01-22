using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public bool exitDialogueModeByScripting = false;
    public bool onlyOnce = false;
    public bool hasBeenUsed = false;
    public bool changeMusic = true;
    public string dialogueMusicKey = "Dialogue1"; //nom de la musica de dialogo

    [Header("Boss Dialogue Settings")]
    [Tooltip("Si está activado, este diálogo se reseteará cuando el jugador muera")]
    public bool isBossDialogue = false; // NUEVO CAMPO

    public DialogueLine[] lines;

    [Header("NPC Teleport Settings")]
    public bool teleportNPCAfterDialogue = false; 
    public string nextLocationID = "";
    public string nextDialogueKey = "";

    [Header("Conditional Appearance")]
    [Tooltip("Si está activado, este NPC solo aparecerá después de derrotar al boss especificado")]
    public bool requiresBossDefeated = false;
    [Tooltip("ID del boss que debe estar derrotado para que aparezca este NPC")]
    public string requiredBossID = "";

    [System.Serializable]
    public class DialogueLine
    {
        public string text;

        public string animatorTrigger;

        public bool requestCameraZoom = false;

        public bool blockPlayerDuringLine = true;

        public bool deactivateObjects = false;

        [Header("Audio")]
        public AudioClip lineAudio;

    }
}