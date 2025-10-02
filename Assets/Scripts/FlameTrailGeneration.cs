using UnityEngine;
using System.Collections.Generic;

public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Trail Settings")]
    public GameObject trailPrefab;
    public GameObject trailCollider;
    public float pointSpacing = 1f;

    // Variables Needed
    private bool generating = false;
    private Rigidbody rigidBody;
    private LineRenderer currentTrail;
    private List<Vector3> currentPoints;
    private Vector3 lastPoint; 

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
        if (currentPoints.Count > 0)
        {
            Vector3 last = currentPoints[^1];
            Vector3 mid = (last + point) / 2f;
            float length = Vector3.Distance(last, point);

            GameObject col = Instantiate(trailCollider, mid, Quaternion.identity, currentTrail.gameObject.transform);
            col.transform.LookAt(point);
            col.transform.localScale = new Vector3(1f, 0.2f, length);
        }

        currentPoints.Add(point);
        currentTrail.positionCount = currentPoints.Count;
        currentTrail.SetPosition(currentPoints.Count - 1, point);
        lastPoint = point;
    }

    public void StartBoostTrail()
    {
        GameObject newTrail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        currentTrail = newTrail.GetComponent<LineRenderer>();

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