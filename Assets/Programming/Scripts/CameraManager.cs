using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Objects")]
    public GameObject mainCamera;
    public GameObject gameplayCamera;

    [Header("Screen Effects")]
    public ParticleSystem speedLines;
    public ParticleSystem flameLines;
}