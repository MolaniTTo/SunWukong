using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private const string VOLUME_KEY = "Volume";
    private const string FULLSCREEN_KEY = "FullScreen";
    private const string QUALITY_KEY = "Quality";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        ApplySettings();
    }

    private void ApplySettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.75f);
        float volumeDB = Mathf.Log10(Mathf.Clamp(savedVolume, 0.0001f, 1f)) * 20f;

        if (audioMixer != null)
        {
            audioMixer.SetFloat("Volume", volumeDB);
        }

        bool savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        Screen.fullScreenMode = savedFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

        int savedQuality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality);
    }

    public void ResetAllSettings() //Per debugar
    {
        PlayerPrefs.DeleteKey(VOLUME_KEY);
        PlayerPrefs.DeleteKey(FULLSCREEN_KEY);
        PlayerPrefs.DeleteKey(QUALITY_KEY);
        PlayerPrefs.Save();

        ApplySettings();
    }
}