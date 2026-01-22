using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CreditsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup titleCanvasGroup;      // Para "CRÉDITOS"
    [SerializeField] private CanvasGroup namesCanvasGroup;      // Para los nombres
    
    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 1f;           // Delay antes de empezar
    [SerializeField] private float titleFadeInDuration = 1.5f;  // Duración fade in título
    [SerializeField] private float titleDisplayTime = 2f;       // Tiempo que se muestra el título
    [SerializeField] private float titleFadeOutDuration = 1f;   // Duración fade out título
    [SerializeField] private float namesFadeInDuration = 1.5f;  // Duración fade in nombres
    [SerializeField] private float namesDisplayTime = 4f;       // Tiempo que se muestran los nombres
    [SerializeField] private float namesFadeOutDuration = 1f;   // Duración fade out nombres
    [SerializeField] private float finalDelay = 1f;             // Delay antes de ir a Stats
    
    [Header("Scene Settings")]
    [SerializeField] private string statsSceneName = "StatsScene";
    
    [Header("Audio (Opcional)")]
    [SerializeField] private AudioSource creditsMusic;

    private void Start()
    {
        // Asegurar que todo empiece invisible
        if (titleCanvasGroup != null)
            titleCanvasGroup.alpha = 0f;
        
        if (namesCanvasGroup != null)
            namesCanvasGroup.alpha = 0f;
        
        // Reproducir música si está asignada
        if (creditsMusic != null)
            creditsMusic.Play();
        
        // Iniciar secuencia de créditos
        StartCoroutine(CreditsSequence());
    }

    private IEnumerator CreditsSequence()
    {
        // Delay inicial
        yield return new WaitForSeconds(initialDelay);
        
        // === FASE 1: TÍTULO "CRÉDITOS" ===
        // Fade in del título
        yield return StartCoroutine(FadeCanvasGroup(titleCanvasGroup, 0f, 1f, titleFadeInDuration));
        
        // Mantener título visible
        yield return new WaitForSeconds(titleDisplayTime);
        
        // El título se queda visible
        
        
        // === FASE 2: NOMBRES ===
        // Fade in de los nombres (todos a la vez)
        yield return StartCoroutine(FadeCanvasGroup(namesCanvasGroup, 0f, 1f, namesFadeInDuration));
        
        // Mantener nombres visibles
        yield return new WaitForSeconds(namesDisplayTime);
        
        
        // === FASE 3: FADE OUT DE TODO ===
        // Hacer fade out del título y nombres a la vez
        StartCoroutine(FadeCanvasGroup(titleCanvasGroup, 1f, 0f, titleFadeOutDuration));
        yield return StartCoroutine(FadeCanvasGroup(namesCanvasGroup, 1f, 0f, namesFadeOutDuration));
        
        
        // === FASE 4: TRANSICIÓN A STATS ===
        yield return new WaitForSeconds(finalDelay);
        
        // Cargar escena de Stats
        SceneManager.LoadScene(statsSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null)
            yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
}