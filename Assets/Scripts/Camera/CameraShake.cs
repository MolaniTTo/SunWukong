using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>(); //Agafem el component de soroll
        }
    }


    /// <summary>
    /// Inicia el efecte shake a la càmera
    /// </summary>
    /// <param name="amplitude">Intensitat del shake</param>
    /// <param name="frequency">Frequencia del shake</param>
    /// <param name="duration">Duració en segons</param>
    public void Shake(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;
        StartCoroutine(ShakeCoroutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeCoroutine(float amplitude, float frequency, float duration)
    {
        // Guardar valores originales
        float originalAmplitude = noise.AmplitudeGain;
        float originalFrequency = noise.FrequencyGain;

        // Aplicar shake
        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        // Restaurar valores originales
        noise.AmplitudeGain = originalAmplitude;
        noise.FrequencyGain = originalFrequency;
    }
}
