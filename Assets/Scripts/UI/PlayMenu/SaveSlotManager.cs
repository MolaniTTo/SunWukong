using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; 
using UnityEngine.InputSystem; //Si usas el nuevo sistema de Input

public class SaveSlotManager : MonoBehaviour
{ 
    [Header("Panels")]
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private GameObject playDeletePanel;
    [SerializeField] private GameObject noHitPanel;

    [Header("Slots (partides)")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;

    [Header("SlotText")]
    [SerializeField] private TextMeshProUGUI slot1Text; 
    [SerializeField] private TextMeshProUGUI slot2Text;
    [SerializeField] private TextMeshProUGUI slot3Text;

    [Header("Play/Delete")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button deleteButton;

    [Header("NoHit Buttons")]
    [SerializeField] private Button noHitYesButton;
    [SerializeField] private Button noHitNoButton;

    [Header("Panel Navigation")]
    [SerializeField] private Button firstSlotToSelect; // Normalmente slot1Button
    [SerializeField] private Button firstPlayDeleteToSelect; // Normalmente playButton
    [SerializeField] private Button firstNoHitToSelect; // Normalmente noHitNoButton

    [Header("Input Actions")] 
    [SerializeField] private InputActionReference cancelAction;

    // Estado interno
    private int selectedSlot = -1; // -1 = ninguno, 0 = slot1, 1 = slot2, 2 = slot3
    private SaveData[] saveData = new SaveData[3]; // Datos de las 3 partidas

    // Panel actual
    private enum MenuPanel { Slots, PlayDelete, NoHit }
    private MenuPanel currentPanel = MenuPanel.Slots;

    private void OnEnable()
    {
        LoadAllSaveData();

        if(cancelAction != null )
        {
            cancelAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.Disable();
        }
    }
    void Start()
    {
        // Cargar datos de partidas (simulado por ahora)
        LoadAllSaveData();

        // Configurar botones
        slot1Button.onClick.AddListener(() => OnSlotSelected(0));
        slot2Button.onClick.AddListener(() => OnSlotSelected(1));
        slot3Button.onClick.AddListener(() => OnSlotSelected(2));

        playButton.onClick.AddListener(OnPlayClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);

        noHitYesButton.onClick.AddListener(() => OnNoHitSelected(true));
        noHitNoButton.onClick.AddListener(() => OnNoHitSelected(false));

        // Inicializar
        ShowSlotsPanel();
    }
    void Update()
    {
        // Detectar botón B (o equivalente) para volver atrás
        if (cancelAction != null && cancelAction.action.WasPerformedThisFrame())
        {
            GoBack();
        }
        // Mantener navegación activa si se pierde
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            RestoreNavigation();
        }
    }

    // ==================== CARGA DE DATOS ====================
    private void LoadAllSaveData()
    {
        // AQUÍ iría tu sistema de guardado real
        // Por ahora simulamos datos
        for (int i = 0; i < 3; i++)
        {
            saveData[i] = LoadSaveData(i);
            UpdateSlotUI(i);
        }
    }

    private SaveData LoadSaveData(int slotIndex)
    {
        // TEMPORAL: Simula si hay partida guardada
        // Reemplaza esto con tu sistema de guardado real
        bool hasData = PlayerPrefs.GetInt($"Slot{slotIndex}_HasData", 0) == 1;
        float progress = PlayerPrefs.GetFloat($"Slot{slotIndex}_Progress", 0f);

        return new SaveData { hasData = hasData, progressPercentage = progress };
    }

    private void UpdateSlotUI(int slotIndex)
    {
        TextMeshProUGUI slotText = GetSlotText(slotIndex);

        if (saveData[slotIndex].hasData)
        {
            slotText.text = $"Progreso: {saveData[slotIndex].progressPercentage:F0}%";
        }
        else
        {
            slotText.text = "Nueva Partida";
        }
    }

    private TextMeshProUGUI GetSlotText(int index)
    {
        switch (index)
        {
            case 0: return slot1Text;
            case 1: return slot2Text;
            case 2: return slot3Text;
            default: return null;
        }
    }

    // ==================== NAVEGACIÓN ENTRE PANELES ====================
    private void ShowSlotsPanel()
    {
        currentPanel = MenuPanel.Slots;

        slotsPanel.SetActive(true);
        playDeletePanel.SetActive(false);
        noHitPanel.SetActive(false);

        // Habilitar todos los slots
        slot1Button.interactable = true;
        slot2Button.interactable = true;
        slot3Button.interactable = true;

        // Deshabilitar botones Jugar/Borrar
        playButton.interactable = false;
        deleteButton.interactable = false;

        selectedSlot = -1;

        // Seleccionar primer slot
        EventSystem.current.SetSelectedGameObject(firstSlotToSelect.gameObject);
    }

    private void ShowPlayDeletePanel()
    {
        currentPanel = MenuPanel.PlayDelete;

        playDeletePanel.SetActive(true);

        // Habilitar botón Jugar siempre
        playButton.interactable = true;

        // Habilitar Borrar solo si hay datos en el slot
        deleteButton.interactable = saveData[selectedSlot].hasData;

        // Bloquear los otros 2 slots
        LockUnselectedSlots();

        // Seleccionar primer botón
        EventSystem.current.SetSelectedGameObject(firstPlayDeleteToSelect.gameObject);
    }

    private void ShowNoHitPanel()
    {
        currentPanel = MenuPanel.NoHit;

        noHitPanel.SetActive(true);

        // Seleccionar primer botón
        EventSystem.current.SetSelectedGameObject(firstNoHitToSelect.gameObject);
    }

    private void LockUnselectedSlots()
    {
        slot1Button.interactable = (selectedSlot == 0);
        slot2Button.interactable = (selectedSlot == 1);
        slot3Button.interactable = (selectedSlot == 2);
    }

    // ==================== EVENTOS DE BOTONES ====================
    private void OnSlotSelected(int slotIndex)
    {
        selectedSlot = slotIndex;
        Debug.Log($"Slot {slotIndex + 1} seleccionado");

        ShowPlayDeletePanel();
    }

    private void OnPlayClicked()
    {
        Debug.Log($"Jugar en Slot {selectedSlot + 1}");
        if (!saveData[selectedSlot].hasData) //si no te dades de partida, mostrar panel NoHit
        {
            ShowNoHitPanel();
        }
        else
        {
            //carreguem la partida amb la configuració NoHit guardada
            StartGame(selectedSlot, PlayerPrefs.GetInt($"Slot{selectedSlot}_NoHit", 0) == 1);
        }
    }

    private void OnDeleteClicked()
    {
        Debug.Log($"Borrar Slot {selectedSlot + 1}");

        if (ProgressManager.Instance != null) //borrar dades del ProgressManager
        {
            ProgressManager.Instance.ResetSlot(selectedSlot);
        }
        else
        {
            // Borrar datos del slot
            PlayerPrefs.DeleteKey($"Slot{selectedSlot}_HasData");
            PlayerPrefs.DeleteKey($"Slot{selectedSlot}_Progress");
            PlayerPrefs.DeleteKey($"Slot{selectedSlot}_GameProgress");
            PlayerPrefs.DeleteKey($"Slot{selectedSlot}_NoHit");
            PlayerPrefs.Save();
        }
        
        // Actualizar datos y UI
        saveData[selectedSlot] = new SaveData { hasData = false, progressPercentage = 0f };
        UpdateSlotUI(selectedSlot);

        // Volver al panel de slots
        ShowSlotsPanel();
    }

    private void OnNoHitSelected(bool noHitEnabled)
    {
        Debug.Log($"NoHit: {(noHitEnabled ? "Activado" : "Desactivado")}");

        // AQUÍ inicias la partida con el slot seleccionado y la configuración NoHit
        StartGame(selectedSlot, noHitEnabled);
    }

    private void StartGame(int slotIndex, bool noHit)
    {
        // Guardar configuración
        PlayerPrefs.SetInt($"Slot{slotIndex}_NoHit", noHit ? 1 : 0);
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);
        PlayerPrefs.Save();

        Debug.Log($"¡Iniciando partida en Slot {slotIndex + 1} con NoHit: {noHit}!");

        // AQUÍ cargas tu escena de juego
        if (!saveData[slotIndex].hasData)
        {
            ProgressManager.Instance?.ResetSlot(slotIndex);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("LVL1");
    }

    // ==================== NAVEGACIÓN HACIA ATRÁS (Botón B) ====================
    private void GoBack()
    {
        switch (currentPanel)
        {
            case MenuPanel.Slots:
                // Ya estamos en el panel principal, no hacemos nada
                // O puedes salir del juego / volver al menú anterior
                Debug.Log("Ya estás en el menú principal");
                break;

            case MenuPanel.PlayDelete:
                // Volver a slots
                ShowSlotsPanel();
                break;

            case MenuPanel.NoHit:
                // Volver a Play/Delete
                noHitPanel.SetActive(false);
                ShowPlayDeletePanel();
                break;
        }
    }

    private void RestoreNavigation()
    {
        // Si se pierde la selección, restaurarla según el panel actual
        switch (currentPanel)
        {
            case MenuPanel.Slots:
                EventSystem.current.SetSelectedGameObject(firstSlotToSelect.gameObject);
                break;
            case MenuPanel.PlayDelete:
                EventSystem.current.SetSelectedGameObject(firstPlayDeleteToSelect.gameObject);
                break;
            case MenuPanel.NoHit:
                EventSystem.current.SetSelectedGameObject(firstNoHitToSelect.gameObject);
                break;
        }
    }

    // ==================== ESTRUCTURA DE DATOS ====================
    [System.Serializable]
    private struct SaveData
    {
        public bool hasData;
        public float progressPercentage;
    }
}