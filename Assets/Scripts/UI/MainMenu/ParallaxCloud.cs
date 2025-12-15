using UnityEngine;
using UnityEngine.UI;

public class ParallaxCloud : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float resetPosition = -1920f; // Ancho de pantalla
    [SerializeField] private float startPosition = 1920f;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        // Mover la nube
        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;
        
        // Reset cuando sale de pantalla
        if (rectTransform.anchoredPosition.x <= resetPosition)
        {
            rectTransform.anchoredPosition = new Vector2(startPosition, rectTransform.anchoredPosition.y);
        }
    }
}