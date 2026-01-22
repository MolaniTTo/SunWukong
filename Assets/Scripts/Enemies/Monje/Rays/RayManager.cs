using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayManager : MonoBehaviour
{
    [Header("Ray settings")]
    public float numberOfRays = 3;
    public GameObject rayPrefab;
    public float raySpacing = 1.0f;

    [Header("Warning settings")]
    public GameObject FirstWarning;
    public GameObject SecondWarning;
    public List<GameObject> warnings; 

    [Header("Spawn settings")]
    public Transform[] spawnPoints;

    [Header("Refs")]
    public Transform playerTransform;
    public Monje monje;
    public Transform monjeTransform;

    public float[] leftBoundaries = { -5, 10, 26 };
    public float[] rightBoundaries = { 8, 24, 40 };

    [Header("Audio")]
    public AudioSource rayAudioSource;
    public AudioClip StormSound;
    public AudioClip WarningSound;

    public void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }


    public void ThrowRaysRoutine()
    {
        if (!monje.firstRayThrowed) //si es el primer raig que tira
        {
            monje.firstRayThrowed = true; //marquem que ja ha tirat el primer raig
            StartCoroutine(ThrowFirstRay());
        }
        else
        {
            StartCoroutine(ThrowRaysWithWarnings());
        }
    }

    public IEnumerator ThrowFirstRay()
    {
        Debug.Log("RayManager: Tirant primer raig sense warning.");
        Vector3 spawnPosition = new Vector3(playerTransform.position.x + 4.38f, playerTransform.position.y + 20f, playerTransform.position.z);
        Instantiate(rayPrefab, spawnPosition, Quaternion.identity);
        yield return new WaitForSeconds(3f);
        monje.dialogueFinished = true; //marquem que ha acabat el diàleg després del primer raig
        monje.characterHealth.isInvincible = false; //el jugador ja pot rebre danys després del primer raig
        monje.raysFinished = true; //marquem que ha acabat de tirar els raigs
        monje.playerRef.ExitDialogueMode(); //sortim del mode diàleg després del primer raig
        monje.monjeMusicController.StartBossMusic(); //iniciem la música de boss després del primer raig

    }

    private IEnumerator ThrowRaysWithWarnings()
    {
        warnings.Clear();

        //instanciem els warnings i configurem els seus moviments
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject w = Instantiate(FirstWarning, spawnPoints[i]); //instancia el warning al spawn point
            w.transform.localPosition = Vector3.zero; //resetejem la posició local per assegurar que està al centre del spawn point
            
            warnings.Add(w); //l'afegim a la llista de warnings per poderlo gestionar després

            WarningMover mover = w.GetComponent<WarningMover>(); //li passem les referències necessàries per al moviment
            mover.player = playerTransform;
            mover.monje = monjeTransform;

            //limits personalitzats per a cada warning, va en ordre segons l'índex del spawn point
            mover.minX = leftBoundaries[i];
            mover.maxX = rightBoundaries[i];

            mover.StartMoving(); //comencem el moviment del warning
        }

        //temps de espera abans de la següent fase
        yield return new WaitForSeconds(2f);

        List<GameObject> secondWarnings = new List<GameObject>();

        for (int i = 0; i < warnings.Count; i++)
        {
            Vector3 pos = warnings[i].transform.position;
            Destroy(warnings[i]); // destruir primer warning

            //instanciem el segon warning a la mateixa posició
            GameObject sw = Instantiate(SecondWarning, spawnPoints[i]);
            sw.transform.localPosition = Vector3.zero; //resetejem la posició local per assegurar que està al centre del spawn point

            //copiem els paràmetres de moviment del primer warning
            WarningMover mover = sw.GetComponent<WarningMover>();
            mover.player = playerTransform;
            mover.monje = monjeTransform;
            mover.minX = leftBoundaries[i];
            mover.maxX = rightBoundaries[i];

            mover.StartMoving();

            secondWarnings.Add(sw);
        }

        warnings = secondWarnings; //actualitzem la llista de warnings per tenir els segons warnings

        yield return new WaitForSeconds(0.7f); //esperem una mica més abans de tirar els raigs

        //instanciem els raigs a la posició dels warnings
        foreach (var w in warnings)
        {
            Vector3 spawnPos = w.transform.position + new Vector3(4.38f, 5f, 0);
            Instantiate(rayPrefab, spawnPos, Quaternion.identity);
            Destroy(w); //destruim el warning després de tirar el raig
        }

        warnings.Clear(); //netegem la llista de warnings

        yield return new WaitForSeconds(3f);
        monje.raysFinished = true; //marquem que ha acabat de tirar els raigs
    }

}
