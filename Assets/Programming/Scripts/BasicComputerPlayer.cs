using UnityEngine;
using System.Collections;

public class BasicComputerPlayer : MonoBehaviour
{
    [Header("Computer Stats")]
    public float baseSpeed = 10f;
    public float turnSpeed = 5f;
    public float pointReachThreshold = 10f;

    [Header("Boost Settings")]
    public float minBoostDelay = 2f;
    public float maxBoostDelay = 8f;

    [Header("Important References")]
    public bool canAutoMove;
    public GameObject[] targetPoints;

    private Rigidbody rigbody;
    private FlameTrailGeneration flameTrail;
    private int currentIndex = 0;

    IEnumerator Start()
    {
        rigbody = GetComponent<Rigidbody>();
        flameTrail = GetComponent<FlameTrailGeneration>();

        currentIndex = 0;

        if (canAutoMove)
        {
            canAutoMove = false;
            yield return new WaitForSeconds(5f);

            canAutoMove = true;
            StartCoroutine(RandomBoostRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (canAutoMove) MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        // make rotate towards target point
        Vector3 targetPos = targetPoints[currentIndex].transform.position;
        Vector3 direction = (targetPos - transform.position).normalized;

        // Smoothly rotate toward target
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rigbody.rotation = Quaternion.Slerp(rigbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        Vector3 playerVelocity = baseSpeed * rigbody.transform.forward;
        playerVelocity.y = rigbody.linearVelocity.y;
        rigbody.linearVelocity = playerVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            currentIndex++;
            if (currentIndex == targetPoints.Length)
            {
                currentIndex = 0;
                StopAllCoroutines();
            }
        }
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
            float trailTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(trailTime);

            // Stop boost
            flameTrail.StopBoostTrail();
        }
    }

    public void SetAutoMove()
    {
        canAutoMove = true;
        currentIndex = 0;
        rigbody = GetComponent<Rigidbody>();
    }
}
