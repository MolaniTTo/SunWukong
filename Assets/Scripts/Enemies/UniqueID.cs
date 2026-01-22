using UnityEngine;

public class UniqueID : MonoBehaviour
{
    [Header("Unique Identifier")]
    [SerializeField] private string uniqueID = "";

    [Header("Auto-Generate Settings")]
    [SerializeField] private bool autoGenerateInEditor = true;

    public string ID
    {
        get
        {
            // Si no tiene ID, generar uno
            if (string.IsNullOrEmpty(uniqueID))
            {
                GenerateID();
            }
            return uniqueID;
        }
    }

    private void Awake()
    {
        // Asegurar que tiene un ID al iniciar
        if (string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generar ID en el editor si está vacío
        if (autoGenerateInEditor && string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();

            // Marcar el objeto como "dirty" para que Unity guarde los cambios
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    [ContextMenu("Generate New ID")]
    public void GenerateID()
    {
        // Generar un ID único basado en:
        // - Nombre del objeto
        // - Nombre de la escena
        // - Posición inicial (la que tiene en el editor)
        // - Un GUID para garantizar unicidad

        string sceneName = gameObject.scene.name;
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = "Unknown";
        }

        Vector3 pos = transform.position;
        string posString = $"{Mathf.RoundToInt(pos.x * 100)}_{Mathf.RoundToInt(pos.y * 100)}";

        // Usar un GUID corto (primeros 8 caracteres)
        string guid = System.Guid.NewGuid().ToString().Substring(0, 8);

        uniqueID = $"{sceneName}_{gameObject.name}_{posString}_{guid}";

        Debug.Log($"ID generado para {gameObject.name}: {uniqueID}");
    }

    [ContextMenu("Show ID")]
    public void ShowID()
    {
        Debug.Log($"ID de {gameObject.name}: {ID}");
    }
}