using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeValueText;

    [Header("Graphics")]
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Settings Keys")]
    private const string VolumeKey = "Volume";
    private const string FullScreenKey = "FullScreen";
    private const string QualityKey = "Quality";

    private void Start()
    {
        LoadSettings();

        if(volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }
        if(fullScreenToggle != null)
        {
            fullScreenToggle.onValueChanged.AddListener(FullScreen);
        }
        if(qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    // ==================== PANTALLA COMPLETA ====================
    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreenMode = fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt(FullScreenKey, fullScreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("FullScreen set to: " + fullScreen);
    }

    // ==================== VOLUM ====================
    public void ChangeVolume(float volume)
    {
        float volumeDB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f; //converteix de 0-1 a dB -> -80 a 0

        audioMixer.SetFloat("Volume", volumeDB);

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();

        if(volumeValueText != null)
        {
            float normalizedVolume = Mathf.InverseLerp(0.0001f, 1f, volume);
            int percentage = Mathf.RoundToInt(normalizedVolume * 100f);
            volumeValueText.text = "Volumen: " + percentage + "%";
        }

        Debug.Log("Volume set to: " + volume + " (" + volumeDB + " dB)");
    }

    // ==================== QUALITAT DELS GRAFICS ====================

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QualityKey, qualityIndex);
        PlayerPrefs.Save();
        Debug.Log("Quality set to: " + qualityIndex);
    }

    // ==================== LOAD SETTINGS ====================

    private void LoadSettings()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.75f); // 75% por defecto
            volumeSlider.value = savedVolume;
            ChangeVolume(savedVolume);
        }

        if (fullScreenToggle != null)
        {
            bool savedFullscreen = PlayerPrefs.GetInt(FullScreenKey, 1) == 1; // Fullscreen por defecto
            fullScreenToggle.isOn = savedFullscreen;
            FullScreen(savedFullscreen);
        }

        if (qualityDropdown != null)
        {
            int savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
            qualityDropdown.value = savedQuality;
            SetQuality(savedQuality);
        }
    }

    // ==================== RESET A DEFAULT ====================
    public void ResetToDefaults()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = 0.75f;
            ChangeVolume(0.75f);
        }

        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = true;
            FullScreen(true);
        }

        if (qualityDropdown != null)
        {
            int defaultQuality = QualitySettings.names.Length - 1; // Máxima calidad
            qualityDropdown.value = defaultQuality;
            SetQuality(defaultQuality);
        }

        Debug.Log(Equals("Settings reset to default values."));
    }


}
