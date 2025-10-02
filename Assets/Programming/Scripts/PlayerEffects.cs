using UnityEngine;
using System.Collections;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]
    public ParticleSystem speedLines;
    public ParticleSystem driftSparks;

    [Header("Important References")]
    public Camera playerCam;

    // Other Variables Needed
    float gameFOV;
    Coroutine currentZoomCoroutine;

    void Start()
    {
        Camera playerCam = Camera.main;
        gameFOV = playerCam.fieldOfView;
    }

    public void ActivateBoostEffect(bool activate)
    {
        if (currentZoomCoroutine != null) StopCoroutine(currentZoomCoroutine);

        if (activate)
        {       
            currentZoomCoroutine = StartCoroutine(CameraZoomOut(70f));
            speedLines.Play();
        }
        else
        {
            currentZoomCoroutine = StartCoroutine(CameraZoomOut(gameFOV));
            speedLines.Stop();
        }
    }
    IEnumerator CameraZoomOut(float targetFOV)
    {
        float startFOV = playerCam.fieldOfView;
        float endFOV = targetFOV;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCam.fieldOfView = Mathf.Lerp(startFOV, endFOV, elapsed / duration);
            yield return null;
        }
        playerCam.fieldOfView = endFOV;
    }

    public void ActivateDriftSparks(bool activate)
    {
        if (activate)
        {
            driftSparks.Play();
        }
        else
        {
            driftSparks.Stop();
        }
    }
}