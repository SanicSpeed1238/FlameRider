using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator References")]
    [SerializeField] private Animator motorcycleAnim;
    [SerializeField] private Animator playerAnim;

    [Header("Elastic Properties")]
    [SerializeField] private Transform[] elasticBones;
    public float minAngle = -50f;
    public float maxAngle = -100;
    public float elasticSpeed = 20f;
    private float oscillationSpeed;
    private float oscillationTime;
    private Quaternion[] originalRotations;

    void Start()
    {
        if (motorcycleAnim == null) Debug.LogWarning("Motorcycle Animator not set!");
        if (playerAnim == null) Debug.LogWarning("Player Animator not set!");

        originalRotations = new Quaternion[elasticBones.Length];
        for (int i = 0; i < elasticBones.Length; i++) { originalRotations[i] = elasticBones[i].localRotation; }
    }

    private void Update()
    {
        float normalizedSpeed = Mathf.Clamp01(oscillationSpeed);
        if (normalizedSpeed > 0.01f) oscillationTime += Time.deltaTime * elasticSpeed * (0.5f + normalizedSpeed);

        for (int i = 0; i < elasticBones.Length; i++)
        {
            float angle;

            if (normalizedSpeed <= 0.01f)
            {
                angle = minAngle;
            }
            else
            {
                float offset = i * 0.5f;
                float wave = Mathf.Sin(oscillationTime + offset) * 0.5f + 0.5f;
                float targetAngle = Mathf.Lerp(minAngle, maxAngle, wave);
                angle = Mathf.Lerp(minAngle, targetAngle, normalizedSpeed);
            }

            elasticBones[i].localRotation = Quaternion.Slerp
                (elasticBones[i].localRotation, originalRotations[i] * Quaternion.Euler(angle, 0f, 0f), Time.deltaTime * 10f);
        }
    }

    public void SetGrounded(bool grounded)
    {
        motorcycleAnim.SetBool("Grounded", grounded);
        playerAnim.SetBool("Grounded", grounded);
    }

    public void SetSpeed(float speed)
    {
        motorcycleAnim.SetFloat("Speed", speed, 0.2f, Time.deltaTime);
        playerAnim.SetFloat("Speed", speed, 0.2f, Time.deltaTime);

        oscillationSpeed = speed;
    }

    public void SteerAnimation(float tilt)
    {
        motorcycleAnim.SetFloat("Tilt", tilt, 0.2f, Time.deltaTime);
        playerAnim.SetFloat("Tilt", tilt, 0.2f, Time.deltaTime);
    }

    public void DriftAnimation(bool activate, float direction)
    {
        motorcycleAnim.SetBool("Drifting", activate);
        motorcycleAnim.SetFloat("Direction", direction);

        playerAnim.SetBool("Drifting", activate);
        playerAnim.SetFloat("Direction", direction);
    }
}