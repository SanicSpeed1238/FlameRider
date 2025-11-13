using UnityEngine;
using UnityEngine.Events;

public class FlameTrailCheck : MonoBehaviour
{
    [Header("Trail Detection Settings")]
    [Range(0.1f, 2f)]
    public float raycastDistance;
    public bool debugRay = true;

    [Header("Trail Events")]
    public UnityEvent OnTrail;

    private int trailLayer;

    void Start()
    {
        trailLayer = LayerMask.GetMask("Trail");
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _, raycastDistance, trailLayer))
        {
            OnTrail?.Invoke();
            Debug.Log("On Trail");
        }

        if (debugRay)
        {
            bool hitTrail = Physics.Raycast(transform.position, Vector3.down, out _, raycastDistance, trailLayer);
            Debug.DrawRay(transform.position, Vector3.down * raycastDistance, hitTrail ? Color.green : Color.red);
        }
    }
}