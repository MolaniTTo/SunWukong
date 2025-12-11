using UnityEngine;

public class PickUpObjects : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject staffVisual;

    void Start()
    {
        if (ProgressManager.Instance != null)
        {
            if (ProgressManager.Instance.GetCurrentProgress().hasStaff)
            {
                // Ya lo tiene, desactivar el objeto
                gameObject.SetActive(false);
            }
        }
    }
}