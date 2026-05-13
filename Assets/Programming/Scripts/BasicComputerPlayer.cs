using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicComputerPlayer : MonoBehaviour
{
    [Header("Computer Stats")]
    public float baseSpeed = 10f;
    public float turnInfluence = 5f;
    public float pointReachThreshold = 10f;

    [Header("Boost Settings")]
    public float minBoostDelay = 1f;
    public float maxBoostDelay = 9f;
    public float boostDuration = 3f;

    [Header("Important References")]
    public bool canAutoMove;

    private Rigidbody playerRB;
    private LayerMask groundLayer;
    private readonly float rotationLerpSpeed = 5f;

    private FlameTrailGeneration flameTrail;
    private PlayerEffects playerVFX;
    private PlayerAnimator playerAnimator;

    private int currentIndex = 0;
    private Vector3 targetPosition;
    private List<Transform> checkPoints = new();
    private TrackManager trackManager;

    void Awake()
    {
        canAutoMove = false;
    }

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        groundLayer = LayerMask.GetMask("Ground");

        flameTrail = GetComponent<FlameTrailGeneration>();
        playerVFX = GetComponentInChildren<PlayerEffects>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();

        trackManager = GameObject.FindAnyObjectByType<TrackManager>();
        if (trackManager == null || trackManager.checkPoints == null)
        {
            targetPosition = transform.position;
            return;
        }
        else
        {
            GetCheckpointList();
            GetTargetPosition();
        }
    }
    private void GetCheckpointList()
    {        
        if (!trackManager) return;

        checkPoints.Clear();
        foreach (GameObject checkPoint in trackManager.checkPoints)
        {
            if (checkPoint != null)
                checkPoints.Add(checkPoint.transform);
        }
        currentIndex = 0;
    }

    public void StartComputerPlayer()
    {
        canAutoMove = true;
        StartCoroutine(RandomBoostRoutine());
    }
    public void AutoMovePlayer(float currentSpeed)
    {
        canAutoMove = true;
        currentIndex = 1;
        baseSpeed = currentSpeed;
    }

    private void FixedUpdate()
    {
        if (canAutoMove) MoveTowardsTarget();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            currentIndex++;
            if (currentIndex >= checkPoints.Count) currentIndex = 0;

            GetTargetPosition();
        }
    }
    private void GetTargetPosition()
    {
        if (checkPoints.Count == 0) return;

        Transform target = checkPoints[currentIndex].transform;

        float randomOffset = Random.Range(-turnInfluence, turnInfluence);

        targetPosition = target.position + (target.right * randomOffset);
    }
    private void MoveTowardsTarget()
    {
        if (checkPoints.Count == 0) return;

        AlignToGround();

        Vector3 rawDirection = (targetPosition - playerRB.position);
        if (Physics.Raycast(playerRB.position, -playerRB.transform.up, out RaycastHit groundHit, 2f, groundLayer))
        {
            rawDirection = Vector3.ProjectOnPlane(rawDirection, groundHit.normal);
        }

        Vector3 direction = rawDirection.normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, playerRB.transform.up);
            playerRB.MoveRotation(Quaternion.Slerp(playerRB.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed));
            
            // Calculate "L Stick Input" for Steer Animation
            float angleDifference = Quaternion.Angle(playerRB.rotation, targetRotation);
            float normalizedTurn = Mathf.Clamp(angleDifference / 15f, 0f, 1f);
            Vector3 cross = Vector3.Cross(playerRB.transform.forward, direction);
            float sign = Mathf.Sign(Vector3.Dot(cross, playerRB.transform.up));
            normalizedTurn *= sign;
            playerAnimator.SteerAnimation(normalizedTurn);
        }

        Vector3 playerVelocity = baseSpeed * playerRB.transform.forward;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerVelocity = VelocityAdjustedToSlope(playerVelocity);
        playerRB.linearVelocity = playerVelocity;
    }
    private void AlignToGround()
    {
        Vector3 origin = playerRB.position;
        Vector3 direction = -playerRB.transform.up;
        float distance = 2f;
        bool isGrounded = Physics.Raycast(origin, direction, out RaycastHit ground, distance, groundLayer);

        if (isGrounded)
        {
            Vector3 groundNormal = ground.normal;

            float gravityStrength = Physics.gravity.magnitude;
            playerRB.AddForce(-groundNormal * gravityStrength, ForceMode.Acceleration);

            Vector3 forwardProjected = Vector3.ProjectOnPlane(playerRB.transform.forward, groundNormal).normalized;
            if (forwardProjected.sqrMagnitude < 0.001f) forwardProjected = playerRB.transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, groundNormal);
            playerRB.MoveRotation(targetRotation);
        }
        else
        {
            playerRB.AddForce(Physics.gravity, ForceMode.Acceleration);

            Vector3 forwardProjected = Vector3.ProjectOnPlane(playerRB.transform.forward, Vector3.up).normalized;
            if (forwardProjected.sqrMagnitude < 0.001f) forwardProjected = playerRB.transform.forward;
            Quaternion uprightTarget = Quaternion.LookRotation(forwardProjected, Vector3.up);

            playerRB.MoveRotation(Quaternion.Slerp(playerRB.rotation, uprightTarget, Time.fixedDeltaTime * (rotationLerpSpeed/2f)));
        }
    }
    private Vector3 VelocityAdjustedToSlope(Vector3 velocity)
    {
        Ray ray = new(playerRB.position, -playerRB.transform.up);

        if (Physics.Raycast(ray, out RaycastHit hit, 2f, groundLayer))
        {
            return Vector3.ProjectOnPlane(velocity, hit.normal);
        }

        return velocity;
    }

    private IEnumerator RandomBoostRoutine()
    {
        while (true)
        {
            // Wait before triggering next boost
            float waitTime = Random.Range(minBoostDelay, maxBoostDelay);
            yield return new WaitForSeconds(waitTime);

            // Start boost
            yield return new WaitUntil(() => GameState.Instance.isPlaying);
            flameTrail.StartBoostTrail();
            playerVFX.ActivateTrailGenerate(true);

            // Boost lasts a random duration
            float trailTime = Random.Range(1f, boostDuration);
            yield return new WaitForSeconds(trailTime);

            // Stop boost
            flameTrail.StopBoostTrail();
            playerVFX.ActivateTrailGenerate(false);
        }
    }    
}