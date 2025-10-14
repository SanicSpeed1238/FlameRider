using UnityEngine;
using System.Collections;

public class FlameTrailCollider : MonoBehaviour
{
    public float warmupTime = 1f;
    [SerializeField] private BoxCollider colliderRef;

    void Awake()
    {
        colliderRef.enabled = false;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(warmupTime);
        colliderRef.enabled = true;
    }
}