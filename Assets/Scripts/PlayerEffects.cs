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

    void Start()
    {
        Camera playerCam = Camera.main;
        gameFOV = playerCam.fieldOfView;
    }

    public void ActivateBoostEffect(bool activate)
    {
        if (activate)
        {
            speedLines.Play();
            StartCoroutine(CameraZoomOut(70f));
        }
        else
        {
            speedLines.Stop();
            StartCoroutine(CameraZoomOut(gameFOV));
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