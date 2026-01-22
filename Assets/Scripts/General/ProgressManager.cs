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
    [SerializeField] private DialogueData bossDialogueToReset; //Diàleg de boss a resetear

    [Header("NPC Prefabs")]
    [SerializeField] private GameObject[] npcPrefabs; //Array de prefabs de NPCs

    private int currentSlot = 0; //slot al que estem jugant
    private GameProgress currentProgress;
    private Dictionary<string, GameObject> spawnedNPCs = new Dictionary<string, GameObject>(); //Diccionari per fer seguiment dels NPCs instanciats

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
            SpawnNPCsBasedOnProgress();
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

    // ==================== NPC MANAGEMENT ====================

    public void SetNPCLocation(string npcID, string locationID, bool waitForConditions = false)
    {
        if (!currentProgress.npcLocations.ContainsKey(npcID)) //si no existeix encara l'entrada per a aquest NPC
        {
            currentProgress.npcLocations.Add(npcID, locationID); //afegeix l'entrada
        }
        else //si ja existeix l'entrada
        {
            currentProgress.npcLocations[npcID] = locationID; //actualitza l'entrada existent
        }

        if (waitForConditions)
        {
            if (!currentProgress.npcPendingLocations.Contains(npcID))
            {
                currentProgress.npcPendingLocations.Add(npcID);
            }
        }
        SaveProgress();
        Debug.Log($"NPC {npcID} ubicación guardada: {locationID}");
    }

    public string GetNPCLocation(string npcID)
    {
        if (currentProgress.npcLocations.ContainsKey(npcID)) //si existeix l'entrada per a aquest NPC
        {
            return currentProgress.npcLocations[npcID]; //retorna la ubicació guardada
        }
        return "";
    }

    public void SetNPCDialogue(string npcID, string dialogueKey)
    {
        if (!currentProgress.npcCurrentDialogues.ContainsKey(npcID)) //si no existeix encara l'entrada per a aquest NPC
        {
            currentProgress.npcCurrentDialogues.Add(npcID, dialogueKey);
        }
        else
        {
            currentProgress.npcCurrentDialogues[npcID] = dialogueKey;
        }
        SaveProgress();
        Debug.Log($"NPC {npcID} diálogo guardado: {dialogueKey}");
    }

    public string GetNPCCurrentDialogue(string npcID)
    {
        if (currentProgress.npcCurrentDialogues.ContainsKey(npcID))
        {
            return currentProgress.npcCurrentDialogues[npcID];
        }
        return "";
    }

    public void SpawnNPCAtLocation(string npcID)
    {
        string locationID = GetNPCLocation(npcID); //obtenim la ubicació guardada per a aquest NPC
        if (string.IsNullOrEmpty(locationID))
        {
            Debug.LogWarning($"No hay ubicación guardada para {npcID}");
            return;
        }

        if (currentProgress.npcPendingLocations.Contains(npcID)) //si aquest NPC està pendent de spawnejar per condicions especials
        {
            string dialogueKey = GetNPCCurrentDialogue(npcID);
            if (!string.IsNullOrEmpty(dialogueKey))
            {
                DialogueData dialogue = Resources.Load<DialogueData>($"Dialogues/{dialogueKey}");
                if (dialogue != null && dialogue.requiresBossDefeated)
                {
                    // Verificar si el boss requerido está derrotado
                    if (!IsBossDefeated(dialogue.requiredBossID))
                    {
                        Debug.Log($"NPC {npcID} no puede aparecer aún. Boss {dialogue.requiredBossID} no derrotado.");
                        return; // No spawneamos el NPC todavía
                    }
                    else
                    {
                        // Boss derrotado, remover de pendientes
                        currentProgress.npcPendingLocations.Remove(npcID);
                        SaveProgress();
                        Debug.Log($"Condiciones cumplidas para {npcID}, puede aparecer!");
                    }
                }
            }
        }

        //Buscar el spawn point corresponent
        NPCSpawnPoint[] spawnPoints = FindObjectsByType<NPCSpawnPoint>(FindObjectsSortMode.None);
        NPCSpawnPoint targetSpawn = null;

        foreach (NPCSpawnPoint sp in spawnPoints)
        {
            if (sp.locationID == locationID) //si coincideix l'ID de la ubicació
            {
                targetSpawn = sp; //hem trobat el spawn point i el guardem al targetSpawn
                break;
            }
        }

        if (targetSpawn == null)
        {
            Debug.LogWarning($"No se encontró spawn point con ID: {locationID}");
            return;
        }

        //Busquem el prefab corresponent
        GameObject npcPrefab = null;
        foreach (GameObject prefab in npcPrefabs)
        {
            if (prefab.name == targetSpawn.npcPrefabName)
            {
                npcPrefab = prefab;
                break;
            }
        }

        if (npcPrefab == null)
        {
            Debug.LogWarning($"No se encontró prefab: {targetSpawn.npcPrefabName}");
            return;
        }

        GameObject npcInstance = Instantiate(npcPrefab, targetSpawn.transform.position, Quaternion.identity);
        npcInstance.SetActive(false);

        NPCDialogue npcDialogue = npcInstance.GetComponent<NPCDialogue>();
        if (npcDialogue != null)
        {
            npcDialogue.npcID = npcID;
            npcDialogue.MarkAsSpawned(); 

            // Cargar y asignar el diálogo
            string dialogueKey = GetNPCCurrentDialogue(npcID);
            if (!string.IsNullOrEmpty(dialogueKey))
            {
                DialogueData loadedDialogue = Resources.Load<DialogueData>($"Dialogues/{dialogueKey}");
                if (loadedDialogue != null)
                {
                    npcDialogue.dialogue = loadedDialogue;
                    Debug.Log($"Diálogo asignado a NPC spawneado: {dialogueKey}");
                }
            }
        }

        if (spawnedNPCs.ContainsKey(npcID))
        {
            Destroy(spawnedNPCs[npcID]); // Destruir instancia anterior si existe
            spawnedNPCs[npcID] = npcInstance;
        }
        else
        {
            spawnedNPCs.Add(npcID, npcInstance);
        }

        npcInstance.SetActive(true);

        Debug.Log($"NPC {npcID} spawneado en {locationID}");
    }

    private void SpawnNPCsBasedOnProgress()
    {
        //Neteja les instàncies anteriors
        spawnedNPCs.Clear();

        //Perque cada NPC amb ubicació guardada, el spawneja a la seva ubicació
        foreach (var kvp in currentProgress.npcLocations)
        {
            string npcID = kvp.Key;
            SpawnNPCAtLocation(npcID);
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

        currentProgress.ConvertDictionariesToLists(); //converteix els diccionaris a llistes per serialitzar-los

        //Ho passa a JSON i ho guarda al PlayerPrefs
        string json = JsonUtility.ToJson(currentProgress, true); //serialitza a JSON
        PlayerPrefs.SetString($"Slot{currentSlot}_GameProgress", json); //guarda el JSON al PlayerPrefs

        //Calcula i guarda el percentatge de progrés
        float progressPercentage = CalculateProgressPercentage(); //calcula el percentatge de progrés
        PlayerPrefs.SetFloat($"Slot{currentSlot}_Progress", progressPercentage); //guarda el percentatge en un PlayerPrefs amb el nom del slot
        PlayerPrefs.SetInt($"Slot{currentSlot}_HasData", 1); //marca que aquest slot té dades guardades

        PlayerPrefs.Save();
        Debug.Log($"Progreso guardado en Slot {currentSlot}: {progressPercentage:F0}%");
        Debug.Log($"   - Enemigos derrotados: {currentProgress.defeatedEnemies.Count}");
        Debug.Log($"   - Plátanos recogidos: {currentProgress.collectedBananas.Count}");
        Debug.Log($"   - Diálogos completados: {currentProgress.completedDialogues.Count}");
        Debug.Log($"   - Bosses derrotados: {currentProgress.defeatedBosses.Count}");
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
            Debug.Log("Nueva partida iniciada");
            if (gameManager != null && gameManager.firstSequence != null)
            {
                gameManager.firstSequence.StartSequence();
            }

        }
        else
        {
            currentProgress = JsonUtility.FromJson<GameProgress>(json); //deserialitza el JSON a l'objecte GameProgress

            currentProgress.ConvertListsToDictionaries(); //converteix les llistes a diccionaris per facilitar l'accés

            Debug.Log($"📂 Progreso cargado del Slot {currentSlot}:");
            Debug.Log($"   - Enemigos derrotados: {currentProgress.defeatedEnemies.Count}");
            Debug.Log($"   - Plátanos recogidos: {currentProgress.collectedBananas.Count}");
            Debug.Log($"   - Diálogos completados: {currentProgress.completedDialogues.Count}");
            Debug.Log($"   - Bosses derrotados: {currentProgress.defeatedBosses.Count}");

            if (gameManager != null && gameManager.screenFade != null)
            {
                gameManager.screenFade.FadeIn();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("Base", 1f);
            }
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
            GameObject checkpointObj = GameObject.Find(currentProgress.lastCheckpointName);
            if (checkpointObj != null)
            {
                CheckpointTrigger checkpoint = checkpointObj.GetComponent<CheckpointTrigger>();
                if (checkpoint != null)
                {
                    player.lastCheckPoint = checkpoint.transform; //actualitza el checkpoint del player
                }
            }
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
            Debug.Log($"Enemigos desactivados según progreso: {enemiesDisabled}");
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
            Debug.Log($"Enemigo derrotado: {enemyID}");
            SaveProgress(); //Auto-guardar en derrotar enemics
        }
    }

    // ==================== BOSSES ====================

    public void RegisterBossDefeated(string bossID)
    {
        if (!currentProgress.defeatedBosses.Contains(bossID))
        {
            currentProgress.defeatedBosses.Add(bossID);
            Debug.Log($"BOSS DERROTADO: {bossID}");
            SaveProgress();

            // IMPORTANTE: Intentar spawnear NPCs que estaban esperando este boss
            CheckPendingNPCs();
        }
    }

    public bool IsBossDefeated(string bossID)
    {
        return currentProgress.defeatedBosses.Contains(bossID);
    }

    private void CheckPendingNPCs()
    {
        // Hacer una copia de la lista para evitar modificarla durante la iteración
        List<string> pendingNPCs = new List<string>(currentProgress.npcPendingLocations);

        foreach (string npcID in pendingNPCs)
        {
            Debug.Log($"Verificando condiciones para NPC pendiente: {npcID}");
            SpawnNPCAtLocation(npcID);
        }
    }

    public void ResetBossDialogues()
    {
        if (bossDialogueToReset != null)
        {
            bossDialogueToReset.hasBeenUsed = false;

            if (currentProgress.completedDialogues.Contains(bossDialogueToReset.name))
            {
                currentProgress.completedDialogues.Remove(bossDialogueToReset.name);
            }

            Debug.Log($"✓ Diálogo del boss '{bossDialogueToReset.name}' reseteado.");
            SaveProgress();
        }
        else
        {
            Debug.LogWarning("No se ha asignado ningún diálogo de boss para resetear.");
        }
    }


    public bool IsEnemyDefeated(GameObject enemy) //comprova si un enemic ja ha sigut derrotat
    {
        string enemyID = GenerateEnemyID(enemy); //genera l'ID únic per a l'enemic
        return currentProgress.defeatedEnemies.Contains(enemyID); //comprova si està a la llista
    }

    
    private string GenerateEnemyID(GameObject enemy) //Generem un ID únic per a cada enemic basat en l'escena i la seva posició
    {
        UniqueID uniqueID = enemy.GetComponent<UniqueID>();
        if (uniqueID != null)
        {
            return uniqueID.ID;
        }
        else
        {
            // Fallback: usar posición inicial + nombre + índice en jerarquía
            Debug.LogWarning($"El enemigo {enemy.name} no tiene componente UniqueID. Usando fallback.");

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int siblingIndex = enemy.transform.GetSiblingIndex();
            Transform parent = enemy.transform.parent;
            string parentName = parent != null ? parent.name : "Root";

            return $"{sceneName}_{parentName}_{enemy.name}_{siblingIndex}";
        }
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

        if(dialogue.isBossDialogue) return; //no registrem diàlegs de bosses aquí

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

    private void OnApplicationQuit()
    {
        // Si estamos en modo NoHit y el jugador está muerto, borrar el slot
        if (currentProgress.isOneHitMode && player != null && player.isDead)
        {
            ResetSlot(currentSlot);
            Debug.Log("Aplicación cerrada en modo NoHit con jugador muerto - Slot borrado");
        }
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

    // Bosses derrotados
    public List<string> defeatedBosses = new List<string>();

    [System.Serializable]
    public class StringPair //estructura per a parells clau-valor
    {
        public string key;
        public string value; 
    }

    public List<StringPair> npcLocationsList = new List<StringPair>(); //llista per serialitzar el diccionari de ubicacions dels NPCs
    public List<StringPair> npcDialoguesList = new List<StringPair>(); //llista per serialitzar el diccionari de diàlegs actuals dels NPCs
    public List<string> npcPendingLocations = new List<string>(); //NPCs esperando condiciones

    [System.NonSerialized]
    public Dictionary<string, string> npcLocations = new Dictionary<string, string>(); //diccionari de ubicacions dels NPCs (no serialitzat)
    [System.NonSerialized]
    public Dictionary<string, string> npcCurrentDialogues = new Dictionary<string, string>(); //diccionari de diàlegs actuals dels NPCs (no serialitzat)

    //Configuracio de mode NoHit
    public bool isOneHitMode = false;

    public void ConvertDictionariesToLists()
    {
        npcLocationsList.Clear();
        foreach (var kvp in npcLocations)
        {
            npcLocationsList.Add(new StringPair { key = kvp.Key, value = kvp.Value });
        }

        npcDialoguesList.Clear();
        foreach (var kvp in npcCurrentDialogues)
        {
            npcDialoguesList.Add(new StringPair { key = kvp.Key, value = kvp.Value });
        }
    }
    public void ConvertListsToDictionaries()
    {
        npcLocations = new Dictionary<string, string>();
        foreach (var pair in npcLocationsList)
        {
            if (!npcLocations.ContainsKey(pair.key))
                npcLocations.Add(pair.key, pair.value);
        }

        npcCurrentDialogues = new Dictionary<string, string>();
        foreach (var pair in npcDialoguesList)
        {
            if (!npcCurrentDialogues.ContainsKey(pair.key))
                npcCurrentDialogues.Add(pair.key, pair.value);
        }
    }
}