using UnityEngine;

public class AuraFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.zero;
    
    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}