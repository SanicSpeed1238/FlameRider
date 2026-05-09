using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]   
    public ParticleSystem driftEffect;
    public ParticleSystem trailGenerate;
    public ParticleSystem trailRide;

    [Header("Important References")]
    public Volume postProcessVolume;
    public Transform cameraFollowTransform;

    // Other Effects
    // -------------

    // Camera Effects
    float fieldOfVision;
    Vector3 originalCameraFollowPos;
    Coroutine currentZoomCoroutine;
    CinemachineCamera gameCamera;

    // Post Processing Effects
    MotionBlur motionBlur;
    Coroutine motionBlurCoroutine;   

    // Screen Effects
    private ParticleSystem speedLines;
    private ParticleSystem flameLines;

    void Start()
    {
        CameraManager cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        gameCamera = cameraManager.gameplayCamera.GetComponent<CinemachineCamera>();

        originalCameraFollowPos = cameraFollowTransform.localPosition;
        fieldOfVision = gameCamera.Lens.FieldOfView;        
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
            if (!trailRide.isPlaying) trailRide.Play();
            if (!flameLines.isPlaying) flameLines.Play();          
        }
        else
        {
            trailRide.Stop();
            flameLines.Stop();           
        }
    }

    public void ActivateBoostEffect(bool activate)
    {
        if (activate)
        {           
            speedLines.Play();
            ShakeCamera(0.25f, 5f, 100);
            SetCameraFOV(90f, 0.25f);
        }
        else
        {           
            speedLines.Stop();
            SetCameraFOV(fieldOfVision, 0.5f);
        }
    }

    public void ShakeCamera(float duration, float strength, int vibrato)
    {
        if (cameraFollowTransform == null) return;
        cameraFollowTransform.DOKill();

        cameraFollowTransform.localPosition = originalCameraFollowPos;
        cameraFollowTransform.DOShakePosition(duration, strength, vibrato, 90f, false, true)
            .OnComplete(() => {cameraFollowTransform.localPosition = originalCameraFollowPos;});
    }
    public void ShakeCameraAxis(float duration, float strength, int vibrato, bool shakeX, bool shakeY)
    {
        if (cameraFollowTransform == null) return;
        cameraFollowTransform.DOKill();

        cameraFollowTransform.localPosition = originalCameraFollowPos;
        Vector3 axisStrength = new(
            shakeX ? strength : 0f,
            shakeY ? strength : 0f,
            0f
        );

        cameraFollowTransform
            .DOShakePosition(duration, axisStrength, vibrato, 90f, false, true)
            .OnComplete(() =>
            {
                cameraFollowTransform.localPosition = originalCameraFollowPos;
            });
    }

    public void SetCameraFOV(float targetFOV, float duration)
    {
        if (currentZoomCoroutine != null) StopCoroutine(currentZoomCoroutine);
        currentZoomCoroutine = StartCoroutine(LerpCameraFOV(targetFOV, duration));
    }
    IEnumerator LerpCameraFOV(float targetFOV, float duration)
    {
        float startFOV = gameCamera.Lens.FieldOfView;
        float endFOV = targetFOV;
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

        motionBlurCoroutine = StartCoroutine(LerpMotionBlur(targetIntensity, duration));
    }
    private IEnumerator LerpMotionBlur(float target, float duration)
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