using UnityEngine;
using System.Collections.Generic;

public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Trail Settings")]
    [Range(1f, 10f)]
    public float trailWidth = 1f;
    public GameObject trailPrefab;
    public GameObject trailCollider;

    // Variables Needed
    private bool generating = false;
    private Rigidbody rigidBody;
    private LineRenderer currentTrail;
    private List<Vector3> currentPoints;
    private Vector3 lastPoint;
    private readonly float pointSpacing = 1f;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        currentPoints = new List<Vector3>();
    }

    void FixedUpdate()
    {
        if (generating && currentTrail != null)
        {
            float dist = Vector3.Distance(rigidBody.position, lastPoint);
            if (dist >= pointSpacing)
            {
                AddPoint(transform.position);
            }
        }
    }

    private void AddPoint(Vector3 point)
    {
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 20f)) point.y = hit.point.y + 0.05f;
        else point.y = 0f;

        if (currentPoints.Count > 0)
        {
            Vector3 last = currentPoints[^1];
            Vector3 mid = (last + point) / 2f;
            float length = Vector3.Distance(last, point);

            GameObject collider = Instantiate(trailCollider, mid, Quaternion.identity, currentTrail.gameObject.transform);
            BoxCollider colliderSize = collider.GetComponent<BoxCollider>();
            collider.transform.LookAt(point);
            collider.transform.localScale = new Vector3(colliderSize.size.x, colliderSize.size.y, length);
        }

        currentPoints.Add(point);
        currentTrail.positionCount = currentPoints.Count;
        currentTrail.SetPosition(currentPoints.Count - 1, point);
        lastPoint = point;
    }

    public void StartBoostTrail()
    {
        GameObject newTrail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        currentTrail = newTrail.GetComponentInChildren<LineRenderer>();

        currentTrail.startWidth = trailWidth;
        currentTrail.endWidth = trailWidth;
        currentTrail.alignment = LineAlignment.TransformZ;
        currentPoints.Clear();
        AddPoint(transform.position);

        generating = true;
    }
    public void StopBoostTrail()
    {
        currentTrail = null;
        generating = false;        
    }  

    public bool IsGenerating()
    {
        return generating;
    }
}