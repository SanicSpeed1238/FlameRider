using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]
    public ParticleSystem speedLines;
    public ParticleSystem flameLines;
    public ParticleSystem flameTire;
    public ParticleSystem flameGenerate;
    public ParticleSystem flameRide;

    [Header("Important References")]
    public CinemachineCamera playerCam;
    public Volume postProcessVolume;

    // Other Variables Needed
    float gameFOV;
    Coroutine currentZoomCoroutine;
    Coroutine motionBlurCoroutine;
    MotionBlur motionBlur;

    void Start()
    {
        gameFOV = playerCam.Lens.FieldOfView;
        if (postProcessVolume != null && postProcessVolume.profile != null) postProcessVolume.profile.TryGet(out motionBlur);
    }

    public void ActivateFlameTire(bool activate)
    {
        if (activate) flameTire.Play();
        else flameTire.Stop();
    }

    public void ActivateFlameGenerate(bool activate)
    {
        if (activate) flameGenerate.Play();
        else flameGenerate.Stop();
    }

    public void ActivateFlameLines(bool activate)
    {
        if (activate)
        {
            flameLines.Play();
            flameRide.Play();
        }
        else
        {
            flameLines.Stop();
            flameRide.Stop();
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

    public void SetMotionBlurIntensity(float targetIntensity, float duration = 0.2f)
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
        ActivateFlameTire(false);
        ActivateFlameGenerate(false);
        ActivateFlameLines(false);
        ActivateBoostEffect(false);
    }
}