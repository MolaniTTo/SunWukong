using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private float currentAmplitude = 0f;
    private float currentFrequency = 0f;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Shake(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;

        //acumulem els valors per a un efecte més intens
        currentAmplitude = Mathf.Max(currentAmplitude, amplitude);
        currentFrequency = Mathf.Max(currentFrequency, frequency);

        noise.AmplitudeGain = currentAmplitude;
        noise.FrequencyGain = currentFrequency;

        //reiniciem la coroutine si ja n'hi ha una activa
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeDecay(duration));
    }

    private IEnumerator ShakeDecay(float duration)
    {
        float timer = duration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            float t = timer / duration;

            //fem que decaigui cap a 0
            noise.AmplitudeGain = currentAmplitude * t;
            noise.FrequencyGain = currentFrequency * t;

            yield return null;
        }

        //resetejem els valors
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
        currentAmplitude = 0f;
        currentFrequency = 0f;
        shakeRoutine = null;
    }
}
