using UnityEngine;

public class BehaviorParticle : MonoBehaviour
{
    [SerializeField] float timeToLive = 5f;

    public void Start()
    {
        Destroy(gameObject, timeToLive);
    }

}
