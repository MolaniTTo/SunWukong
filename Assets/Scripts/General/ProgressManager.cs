using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor de progreso integrado con tu GameManager y CharacterHealth existente
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerStateMachine player;

    private int currentSlot = 0; //slot al que estem jugant
    private GameProgress currentProgress;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //Singleton que persisteix entre escenes

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0); //currentSlot es guarda al PlayerPrefs des del GameManager
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}");

        currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0); //actualitza el slot actual
        Debug.Log($"Slot actual: {currentSlot}");
        
        FindReferences();

        
        if (scene.name != "MainMenu" && scene.name != "StatsScene" && scene.name != "Options" && scene.name != "PlayMenu") //només carrega el progrés en escenes de joc
        {
            LoadProgress();
            ApplyProgressToWorld();
        }

    }

    private void FindReferences()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        player = FindFirstObjectByType<PlayerStateMachine>();

        if(gameManager != null && player != null)
        {
            Debug.Log("Referencias encontradas: GameManager y PlayerStateMachine");
        }
        else
        {
            if (gameManager == null) Debug.LogWarning("GameManager no encontrado");
            if (player == null) Debug.LogWarning("Player no encontrado");

        }
    }


    // ==================== Guardar Carregar i Eliminar ====================

    public void SaveProgress() //guarda el progrés actual
    {
        currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0); //actualitza el slot actual

        if (player == null) player = FindFirstObjectByType<PlayerStateMachine>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();

        if (player == null || player.characterHealth == null)
        {
            Debug.LogError("Player no encontrado, no se puede guardar");
            return;
        }

        //Guarda les dades de vida i del bastó del jugador
        currentProgress.playerHealth = player.characterHealth.currentHealth;
        currentProgress.hasStaff = player.hasStaff;

        //Guarda l'ultim checkpoint
        if (player.lastCheckPoint != null)
        {
            currentProgress.lastCheckpointPosition = player.lastCheckPoint.position;
            currentProgress.lastCheckpointName = player.lastCheckPoint.name;
        }

        //Guarda la configuració de NoHit
        if (gameManager != null)
        {
            currentProgress.isOneHitMode = gameManager.isOneHitMode;
        }

        //Ho passa a JSON i ho guarda al PlayerPrefs
        string json = JsonUtility.ToJson(currentProgress, true); //serialitza a JSON
        PlayerPrefs.SetString($"Slot{currentSlot}_GameProgress", json); //guarda el JSON al PlayerPrefs

        //Calcula i guarda el percentatge de progrés
        float progressPercentage = CalculateProgressPercentage(); //calcula el percentatge de progrés
        PlayerPrefs.SetFloat($"Slot{currentSlot}_Progress", progressPercentage); //guarda el percentatge en un PlayerPrefs amb el nom del slot
        PlayerPrefs.SetInt($"Slot{currentSlot}_HasData", 1); //marca que aquest slot té dades guardades

        PlayerPrefs.Save(); //assegura que es guardin les dades
        Debug.Log($"? Progreso guardado en Slot {currentSlot}: {progressPercentage:F0}%");
    }

    public void LoadProgress() //carrega el progrés desat
    {
        currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0); //actualitza el slot actual

        string json = PlayerPrefs.GetString($"Slot{currentSlot}_GameProgress", ""); //agafa playerprefs del slot actual en format JSON

        if (string.IsNullOrEmpty(json)) //si no hi ha dades guardades
        {
            //nova partida
            currentProgress = new GameProgress(); //inicialitza nou progrés
            currentProgress.isOneHitMode = PlayerPrefs.GetInt($"Slot{currentSlot}_NoHit", 0) == 1; //carrega la configuració de NoHit
            Debug.Log("?? Nueva partida iniciada");
            gameManager.firstSequence.StartSequence(); //nomes comença la sequència si es nova partida

        }
        else
        {
            currentProgress = JsonUtility.FromJson<GameProgress>(json); //deserialitza el JSON a l'objecte GameProgress
            Debug.Log($"?? Progreso cargado: {currentProgress.defeatedEnemies.Count} enemigos derrotados");
            gameManager.screenFade.FadeIn(); //fa un fade in suau al carregar
            AudioManager.Instance.PlayMusic("Base", 1f); //posem musica base
        }
    }

    
    private void ApplyProgressToWorld() //aplica el procrés carregat al joc
    {
        if (player == null) player = FindFirstObjectByType<PlayerStateMachine>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();

        if (player == null)
        {
            Debug.LogWarning("Player no encontrado, no se puede aplicar progreso");
            return;
        }

        //Restaura la vida al player
        if (player.characterHealth != null)
        {
            player.characterHealth.currentHealth = currentProgress.playerHealth;
            player.characterHealth.ForceHealthUpdate();
        }

        //Restaurem el bastó
        player.hasStaff = currentProgress.hasStaff;
        if (player.staffObj != null)
        {
            player.staffObj.SetActive(currentProgress.hasStaff); //mostra o amaga el bastó segons el progrés
        }

        //Restaura la posicio del ultim checkpoint
        if (currentProgress.lastCheckpointPosition != Vector3.zero)
        {
            player.transform.position = currentProgress.lastCheckpointPosition; //posa al player a la posició del checkpoint
        }

        //Aplica la configuració de NoHit
        if (gameManager != null)
        {
            gameManager.isOneHitMode = currentProgress.isOneHitMode; //aplica la configuració de NoHit
        }

        //Desactiva els enemics que ja han sigut derrotats
        ApplyDefeatedEnemies();

        ApplyCollectedBananas();

        ApplyCompletedDialogues();
    }

    private void ApplyDefeatedEnemies()
    {
        //Busca a tots els enemics a l'escena
        CharacterHealth[] allEnemies = FindObjectsByType<CharacterHealth>(FindObjectsSortMode.None);
        int enemiesDisabled = 0;

        foreach (CharacterHealth enemy in allEnemies) //per cada component CharacterHealth en la llista
        {
            if (enemy.isPlayer) continue; //Saltem al jugador

            //Genera un ID únic per a l'enemic
            string enemyID = GenerateEnemyID(enemy.gameObject);

            //Si ja ha estat derrotat, el desactiva
            if (currentProgress.defeatedEnemies.Contains(enemyID))
            {
                enemy.gameObject.SetActive(false);
                enemiesDisabled++;
            }
        }
        if(enemiesDisabled > 0)
        {
            Debug.Log($"?? Enemigos desactivados según progreso: {enemiesDisabled}");
        }
    }

    private void ApplyCollectedBananas()
    {
        //Busca tots els plátanos a l'escena
        BananaPickup[] allBananas = FindObjectsByType<BananaPickup>(FindObjectsSortMode.None);
        int bananasDisabled = 0;

        foreach (BananaPickup banana in allBananas)
        {
            //Genera un ID únic per al plátano
            string bananaID = GenerateBananaID(banana.gameObject);

            //Si ja ha estat recollit, el desactiva
            if (currentProgress.collectedBananas.Contains(bananaID))
            {
                banana.gameObject.SetActive(false);
                bananasDisabled++;
            }
        }

        if (bananasDisabled > 0)
        {
            Debug.Log($"plátanos desactivados según progreso: {bananasDisabled}");
        }
    }

    private void ApplyCompletedDialogues()
    {
        //Busca tots els NPCs amb diàleg a l'escena
        NPCDialogue[] allNPCs = FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        int dialoguesApplied = 0;

        foreach (NPCDialogue npc in allNPCs)
        {
            if (npc.dialogue != null)
            {
                string dialogueID = npc.dialogue.name; //Usem el nom del ScriptableObject com a ID

                //Si aquest diàleg ja s'ha completat, marquem-lo com a usat
                if (currentProgress.completedDialogues.Contains(dialogueID))
                {
                    npc.dialogue.hasBeenUsed = true;
                    dialoguesApplied++;
                    Debug.Log($"Diálogo '{dialogueID}' marcado como completado");
                }
            }
        }

        if (dialoguesApplied > 0)
        {
            Debug.Log($"Diálogos aplicados según progreso: {dialoguesApplied}");
        }
    }

    private float CalculateProgressPercentage() //calcula el percentatge de progrés basat en els enemics derrotats, checkpoints i habilitats
    {
        //Exemple simple: cada enemic derrotat = 1 punt, bastó = 10 punts, cada checkpoint = 5 punts
        int totalPossibleItems = 202; // Ajusta según tu juego
        int completedItems = currentProgress.defeatedEnemies.Count +
                            (currentProgress.hasStaff ? 10 : 0) +
                            currentProgress.unlockedCheckpoints.Count * 5;

        return Mathf.Clamp((float)completedItems / totalPossibleItems * 100f, 0f, 100f);
    }


    // ==================== Enemics ====================

    public void RegisterEnemyDefeated(GameObject enemy) //registra un enemic com a derrotat
    {
        string enemyID = GenerateEnemyID(enemy); //genera un ID únic per a l'enemic

        if (!currentProgress.defeatedEnemies.Contains(enemyID)) //si no està ja registrat com a derrotat
        {
            currentProgress.defeatedEnemies.Add(enemyID); //l'afegeix a la llista
            Debug.Log($"?? Enemigo derrotado: {enemyID}");
            SaveProgress(); //Auto-guardar en derrotar enemics
        }
    }

    public bool IsEnemyDefeated(GameObject enemy) //comprova si un enemic ja ha sigut derrotat
    {
        string enemyID = GenerateEnemyID(enemy); //genera l'ID únic per a l'enemic
        return currentProgress.defeatedEnemies.Contains(enemyID); //comprova si està a la llista
    }

    
    private string GenerateEnemyID(GameObject enemy) //Generem un ID únic per a cada enemic basat en l'escena i la seva posició
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; //nom de l'escena actual
        Vector3 pos = enemy.transform.position; //posició de l'enemic
        return $"{sceneName}_{enemy.name}_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.y)}"; //ID únic
    }

    // ==================== PLÁTANOS ====================

    public void RegisterBananaCollected(GameObject banana)
    {
        string bananaID = GenerateBananaID(banana);

        if (!currentProgress.collectedBananas.Contains(bananaID))
        {
            currentProgress.collectedBananas.Add(bananaID);
            Debug.Log($"Plátano recogido: {bananaID}");
            SaveProgress(); //Auto-guardar al recoger plátanos
        }
    }

    public bool IsBananaCollected(GameObject banana)
    {
        string bananaID = GenerateBananaID(banana);
        return currentProgress.collectedBananas.Contains(bananaID);
    }

    private string GenerateBananaID(GameObject banana)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Vector3 pos = banana.transform.position;

        //Incluïm també el tipus de plátano per si hi ha dos del mateix tipus al mateix lloc
        BananaPickup bananaScript = banana.GetComponent<BananaPickup>();
        string bananaType = bananaScript != null ? bananaScript.bananaType.ToString() : "Unknown";

        return $"{sceneName}_Banana_{bananaType}_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.y)}";
    }

    // ==================== DIÁLOGOS ====================

    public void RegisterDialogueCompleted(DialogueData dialogue)
    {
        if (dialogue == null) return;

        string dialogueID = dialogue.name; //Usem el nom del ScriptableObject

        if (!currentProgress.completedDialogues.Contains(dialogueID))
        {
            currentProgress.completedDialogues.Add(dialogueID);
            Debug.Log($"Diálogo completado: {dialogueID}");
            SaveProgress(); //Auto-guardar al completar diálogos
        }
    }

    public bool IsDialogueCompleted(DialogueData dialogue)
    {
        if (dialogue == null) return false;

        string dialogueID = dialogue.name;
        return currentProgress.completedDialogues.Contains(dialogueID);
    }


    // ==================== CHECKPOINTS ====================

    public void RegisterCheckpoint(Transform checkpoint) //registra un checkpoint com a desbloquejat
    {
        string checkpointID = checkpoint.name; //utilitza el nom del checkpoint com a ID

        if (!currentProgress.unlockedCheckpoints.Contains(checkpointID)) //si no conte el checkpoint en una llista dels desbloquejats
        {
            currentProgress.unlockedCheckpoints.Add(checkpointID); //l'afegeix
            currentProgress.lastCheckpointPosition = checkpoint.position; //actualitza la posició del checkpoint
            currentProgress.lastCheckpointName = checkpointID;  //actualitza el nom del checkpoint

            Debug.Log($"?? Checkpoint guardado: {checkpointID}");
            SaveProgress(); // Auto-guardar en checkpoints
        }
    }

    // ==================== Habilitats ====================

    public void UnlockStaff() //desbloqueja el bastó per al jugador
    {
        if (!currentProgress.hasStaff) //si encara no el té desbloquejat
        {
            currentProgress.hasStaff = true; //l'afegeix al progrés

            if (player != null)
            {
                player.hasStaff = true; //actualitza l'estat del jugador
                if (player.staffObj != null)
                {
                    player.staffObj.SetActive(true); //mostra l'objecte del bastó
                }
            }

            Debug.Log("Staff desbloqueado!");
            SaveProgress(); // Auto-guardar en desbloquejar habilitats
        }
    }

    // ==================== Utilitats ====================

    public void ResetSlot(int slotIndex = -1) //aixo es per resetejar les dades d'un slot concret en el Menu de jugar
    {
        if (slotIndex == -1) //si no es passa cap index, reseteja el slot actual
        {
            slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0); //si no es passa cap index, reseteja el slot actual
        }

        PlayerPrefs.DeleteKey($"Slot{slotIndex}_GameProgress");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_Progress");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_HasData");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_NoHit");
        PlayerPrefs.Save(); //assegura que es guardin les dades

        if (slotIndex == currentSlot)
        {
            currentProgress = new GameProgress();
        }

        Debug.Log($"??? Slot {currentSlot} reseteado");
    }

    public GameProgress GetCurrentProgress() //retorna l'objecte de progrés actual
    {
        return currentProgress;
    }
}

// ==================== Estructura de dades ====================

[System.Serializable]
public class GameProgress //estructura que guarda totes les dades del progrés del joc
{
    //Jugador
    public float playerHealth = 100f;
    public bool hasStaff = false;

    //Checkpoint
    public Vector3 lastCheckpointPosition = Vector3.zero;
    public string lastCheckpointName = "";
    public List<string> unlockedCheckpoints = new List<string>();

    //Enemics derrotats amb IDs únics
    public List<string> defeatedEnemies = new List<string>();

    //Plátanos recollits amb IDs únics
    public List<string> collectedBananas = new List<string>();

    //Diàlegs completats (guardant el nom del ScriptableObject)
    public List<string> completedDialogues = new List<string>();

    //Configuracio de mode NoHit
    public bool isOneHitMode = false;
}