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

    [Header("Spawn settings")]
    public Transform leftLimit;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
