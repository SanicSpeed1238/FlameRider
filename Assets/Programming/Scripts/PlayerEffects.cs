using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]   
    public ParticleSystem driftEffect;
    public ParticleSystem trailGenerate;
    public ParticleSystem trailRide;

    [Header("Important References")]
    public Volume postProcessVolume;

    // Other Effects
    // -------------

    // Camera FOV
    float gameFOV;
    Coroutine currentZoomCoroutine;
    CinemachineCamera gameCamera;

    // Motion Blur
    MotionBlur motionBlur;
    Coroutine motionBlurCoroutine;   

    // Screen Effects
    private ParticleSystem speedLines;
    private ParticleSystem flameLines;

    void Start()
    {
        CameraManager cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        gameCamera = cameraManager.gameplayCamera.GetComponent<CinemachineCamera>();
        gameFOV = gameCamera.Lens.FieldOfView;
        speedLines = cameraManager.speedLines;
        flameLines = cameraManager.flameLines;

        if (postProcessVolume != null && postProcessVolume.profile != null) 
            postProcessVolume.profile.TryGet(out motionBlur);
    }

    public void ActivateDriftEffect(bool activate)
    {
        if (activate) driftEffect.Play();
        else driftEffect.Stop();
    }

    public void ActivateTrailGenerate(bool activate)
    {
        if (activate) trailGenerate.Play();
        else trailGenerate.Stop();
    }

    public void ActivateTrailRide(bool activate)
    {
        if (activate)
        {
            flameLines.Play();
            trailRide.Play();
        }
        else
        {
            flameLines.Stop();
            trailRide.Stop();
        }
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
        float startFOV = gameCamera.Lens.FieldOfView;
        float endFOV = targetFOV;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            gameCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, elapsed / duration);
            yield return null;
        }
        gameCamera.Lens.FieldOfView = endFOV;
    }

    public void SetMotionBlur(float targetIntensity, float duration = 0.2f)
    {
        if (motionBlur == null)
            return;

        if (motionBlurCoroutine != null)
            StopCoroutine(motionBlurCoroutine);

        motionBlurCoroutine = StartCoroutine(LerpMotionBlurIntensity(targetIntensity, duration));
    }
    private IEnumerator LerpMotionBlurIntensity(float target, float duration)
    {
        if (!motionBlur.intensity.overrideState)
            motionBlur.intensity.overrideState = true;

        float start = motionBlur.intensity.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            motionBlur.intensity.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        motionBlur.intensity.value = target;
    }

    public void StopAllEffects()
    {
        ActivateDriftEffect(false);
        ActivateTrailGenerate(false);
        ActivateTrailRide(false);
        ActivateBoostEffect(false);
    }
}