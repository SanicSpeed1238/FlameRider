using UnityEngine;

public class FlameTrailCheck : MonoBehaviour
{
    [Header("Trail Detection Settings")]
    [Range(0.1f, 2f)]
    public float raycastDistance;
    public bool debugRay = true;

    // Variables Needed
    private PlayerController playerController;
    private int trailLayer;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        trailLayer = LayerMask.GetMask("Trail");
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit trailHit, raycastDistance, trailLayer))
        {
            float speedBoost = trailHit.collider.GetComponent<FlameTrailObject>().speedBoost;
            playerController.RideFlameTrail(speedBoost);

            if(debugRay) Debug.Log("On Trail");
        }

        if (debugRay)
        {
            bool hitTrail = Physics.Raycast(transform.position, Vector3.down, out _, raycastDistance, trailLayer);
            Debug.DrawRay(transform.position, Vector3.down * raycastDistance, hitTrail ? Color.green : Color.red);
        }
    }
}