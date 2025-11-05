using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]
    public ParticleSystem speedLines;
    public ParticleSystem flameLines;
    public ParticleSystem flameTire;

    [Header("Important References")]
    public CinemachineCamera playerCam;

    // Other Variables Needed
    float gameFOV;
    Coroutine currentZoomCoroutine;

    void Start()
    {
        gameFOV = playerCam.Lens.FieldOfView;
    }

    public void ActivateFlameLines(bool activate)
    {
        if (activate) flameLines.Play();
        else flameLines.Stop();
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
        float startFOV = playerCam.Lens.FieldOfView;
        float endFOV = targetFOV;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCam.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, elapsed / duration);
            yield return null;
        }
        playerCam.Lens.FieldOfView = endFOV;
    }

    public void ActivateFlameTire(bool activate)
    {
        if (activate)
        {
            flameTire.Play();
        }
        else
        {
            flameTire.Stop();
        }
    }
}