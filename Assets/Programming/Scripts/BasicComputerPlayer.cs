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
    private FlameTrailGeneration flameTrail;
    private LayerMask groundLayer;

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
        flameTrail = GetComponent<FlameTrailGeneration>();
        groundLayer = LayerMask.GetMask("Ground");

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
        if (Physics.Raycast(playerRB.position, -transform.up, out RaycastHit groundHit, 2f, groundLayer))
        {
            rawDirection = Vector3.ProjectOnPlane(rawDirection, groundHit.normal);
        }

        Vector3 direction = rawDirection.normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            playerRB.MoveRotation(Quaternion.Slerp(playerRB.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }

        Vector3 playerVelocity = baseSpeed * transform.forward;
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

            Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            if (forwardProjected.sqrMagnitude < 0.001f)
                forwardProjected = transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, groundNormal);
            playerRB.MoveRotation(targetRotation);
        }
        else
        {
            playerRB.AddForce(Physics.gravity, ForceMode.Acceleration);

            float uprightSpeed = 2f;

            Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            if (forwardProjected.sqrMagnitude < 0.001f)
                forwardProjected = transform.forward;

            Quaternion uprightTarget = Quaternion.LookRotation(forwardProjected, Vector3.up);

            playerRB.MoveRotation(
                Quaternion.Slerp(playerRB.rotation, uprightTarget, uprightSpeed * Time.fixedDeltaTime)
            );
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
            flameTrail.StartBoostTrail();

            // Boost lasts a random duration
            float trailTime = Random.Range(1f, boostDuration);
            yield return new WaitForSeconds(trailTime);

            // Stop boost
            flameTrail.StopBoostTrail();
        }
    }    
}