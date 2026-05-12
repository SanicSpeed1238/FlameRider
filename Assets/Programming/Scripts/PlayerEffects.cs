using DG.Tweening;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerEffects : MonoBehaviour
{
    [Header("VFX References")]   
    public ParticleSystem driftEffect;
    public ParticleSystem trailGenerate;
    public ParticleSystem trailRide;

    [Header("Important References")]
    public Volume postProcessVolume;
    public Transform cameraFollowTransform;
    public bool humanPlayer;

    // Other Effects
    // -------------

    // Camera Effects
    Vector3 originalCameraFollowPos;
    Coroutine currentZoomCoroutine;
    CinemachineCamera gameCamera;  

    // Screen Effects
    private ParticleSystem speedLines;
    private ParticleSystem flameLines;

    void Start()
    {
        if (humanPlayer)
        {
            CameraManager cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
            gameCamera = cameraManager.gameplayCamera.GetComponent<CinemachineCamera>();
            originalCameraFollowPos = cameraFollowTransform.localPosition;
            speedLines = cameraManager.speedLines;
            flameLines = cameraManager.flameLines;
        }  
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
            if (trailRide != null && !trailRide.isPlaying) trailRide.Play();
            if (flameLines != null && !flameLines.isPlaying) flameLines.Play();          
        }
        else
        {
            if (trailRide != null) trailRide.Stop();
            if (flameLines != null) flameLines.Stop();           
        }
    }

    public void ActivateBoostCameraFX(bool activate)
    {
        if (activate)
        {           
            speedLines.Play();
            SetCameraFOV(90f, 0.25f);
            ShakeCamera(0.3f, 10f, 500);
        }
        else
        {           
            speedLines.Stop();
            SetCameraFOV(60f, 0.5f);
        }
    }

    public void SteerCamera(float input, float maxAngle)
    {
        float targetCamAngle = input * maxAngle;
        currentCamAngle = Mathf.SmoothDamp(currentCamAngle, targetCamAngle, ref yCamRotation, 0.2f);
        cameraFollowTransform.localRotation = Quaternion.Euler(0f, currentCamAngle, 0f);
    }
        float currentCamAngle = 0f;
        float yCamRotation;

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

    public void StopAllEffects()
    {
        ActivateDriftEffect(false);
        ActivateTrailGenerate(false);
        ActivateTrailRide(false);
        ActivateBoostCameraFX(false);
    }
}