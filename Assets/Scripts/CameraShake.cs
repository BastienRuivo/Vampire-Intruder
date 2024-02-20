using UnityEngine;
using System.Collections;

public class CameraShake : Singleton<CameraShake>
{
    public float shakeDuration = 0.5f; 
    private Vector3 originalPosition;

    public void Shake(float shakeIntensity)
    {
        originalPosition = transform.localPosition;
        StartCoroutine(ShakeCoroutine(shakeIntensity));
    }

    IEnumerator ShakeCoroutine(float shakeIntensity)
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            transform.localPosition = originalPosition + randomOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }
}