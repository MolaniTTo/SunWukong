using UnityEngine;

public class SkyGradientGenerator : MonoBehaviour
{
    [Header("Colores del degradado")]
    public Color colorArriba = new Color(0.1f, 0.05f, 0.3f, 1f); // Púrpura oscuro
    public Color colorMedio = new Color(1f, 0.42f, 0.21f, 1f); // Naranja
    public Color colorAbajo = new Color(1f, 0.85f, 0.24f, 1f); // Dorado
    
    [Header("Configuración")]
    public int alturaTextura = 1024;
    public int anchuraTextura = 64;
    
    [Header("Tamaño en escena")]
    public float ancho = 20f;
    public float alto = 35f;
    
    void Start()
    {
        GenerarDegradado();
    }
    
    void GenerarDegradado()
    {
        // Crear textura
        Texture2D textura = new Texture2D(anchuraTextura, alturaTextura);
        textura.filterMode = FilterMode.Bilinear;
        textura.wrapMode = TextureWrapMode.Clamp;
        
        for (int y = 0; y < alturaTextura; y++)
        {
            float progreso = y / (float)alturaTextura;
            Color color;
            
            // Tres zonas de degradado
            if (progreso < 0.4f) // Zona baja (dorado)
            {
                color = Color.Lerp(colorAbajo, colorMedio, progreso / 0.4f);
            }
            else if (progreso < 0.7f) // Zona media (naranja)
            {
                color = Color.Lerp(colorMedio, colorArriba, (progreso - 0.4f) / 0.3f);
            }
            else // Zona alta (púrpura)
            {
                color = Color.Lerp(colorArriba, colorArriba * 0.7f, (progreso - 0.7f) / 0.3f);
            }
            
            // Pintar toda la fila del mismo color
            for (int x = 0; x < anchuraTextura; x++)
            {
                textura.SetPixel(x, y, color);
            }
        }
        
        textura.Apply();
        
        // Crear sprite
        Sprite sprite = Sprite.Create(
            textura, 
            new Rect(0, 0, anchuraTextura, alturaTextura), 
            new Vector2(0.5f, 0.5f),
            alturaTextura / alto // Pixels per unit
        );
        
        // Asignar al SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprite;
            sr.sortingLayerName = "Background";
            sr.sortingOrder = -10;
        }
        
        // Escalar para cubrir el área
        transform.localScale = new Vector3(ancho / (anchuraTextura / (float)(alturaTextura / alto)), 1f, 1f);
    }
    
    // Para regenerar en el editor
    [ContextMenu("Regenerar Degradado")]
    void RegenerarEnEditor()
    {
        GenerarDegradado();
    }
}