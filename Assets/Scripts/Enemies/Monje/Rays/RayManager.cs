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

    public float[] leftBoundaries = { -9f, -3f, 3f }; //valors limits de moviment per cada warning
    public float[] rightBoundaries = { -3f, 3f, 9f }; //valors limits de moviment per cada warning


    public void ThrowRaysRoutine()
    {
        if (!monje.firstRayThrowed) //si es el primer raig que tira
        {
            monje.firstRayThrowed = true; //marquem que ja ha tirat el primer raig
            ThrowFirstRay();
        }
        else
        {
            StartCoroutine(ThrowRaysWithWarnings());
        }
    }

    public void ThrowFirstRay()
    {
        Vector3 spawnPosition = new Vector3(playerTransform.position.x, playerTransform.position.y + 50f, playerTransform.position.z);
        GameObject.Instantiate(rayPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator ThrowRaysWithWarnings()
    {
        warnings.Clear();

        //instanciem els warnings i configurem els seus moviments
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject w = Instantiate(FirstWarning, spawnPoints[i].position, Quaternion.identity); //instanciem el warning a la posició del spawn point
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
        yield return new WaitForSeconds(2.5f);

        //parem el moviment dels warnings
        foreach (var w in warnings)
        {
            w.GetComponent<WarningMover>().StopMoving();
        }

        //segona fase de warnings
        foreach (var w in warnings)
        {
            Instantiate(SecondWarning, w.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.7f); //esperem una mica més abans de tirar els raigs

        //instanciem els raigs a la posició dels warnings
        foreach (var w in warnings)
        {
            Vector3 spawnPos = w.transform.position + new Vector3(0, 5f, 0); //
            Instantiate(rayPrefab, spawnPos, Quaternion.identity);
        }
    }

}
