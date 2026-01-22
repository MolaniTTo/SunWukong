using UnityEngine;

public class BossPrefabController : MonoBehaviour
{
    [Header("Boss Settings")]
    public string bossID = "GorilaBoss";

    private void Start()
    {
        // Verificar si este boss ya fue derrotado
        if (ProgressManager.Instance != null)
        {
            if (ProgressManager.Instance.IsBossDefeated(bossID))
            {
                Debug.Log($"Boss {bossID} ya fue derrotado. Desactivando prefab completo...");
                gameObject.SetActive(false);
            }
        }
    }
}