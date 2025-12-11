using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }

    public void ChangeVolume(float volume)
    {
        audioMixer.SetFloat("Volumen", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        qualityIndex += 1; // Ajusta el índice para que 0 sea "Bajo"
        QualitySettings.SetQualityLevel(qualityIndex);
    }

}
