using UnityEngine;
using System.Collections.Generic;

public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Trail Settings")]
    [Range(1f, 10f)]
    public float trailWidth = 1f;
    public GameObject trailPrefab;

    [Header("Collider Settings")]
    public float colliderHeight = 1f; // vertical thickness of collider
    public int pointsPerBake = 10;    // bake collider every 10 points

    [Header("Debug Settings")]
    public bool drawColliderGizmos = true; // visualize the collider in the editor
    public Color gizmoColor = Color.red;

    // Internal variables
    private bool generating = false;
    private Rigidbody rigidBody;
    private LineRenderer currentTrail;
    private List<Vector3> currentPoints;
    private Vector3 lastPoint;
    private int pointsSinceLastBake = 0;
    private readonly float pointSpacing = 1f;

    private MeshCollider meshCollider;
    private Mesh bakedMesh;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        currentPoints = new List<Vector3>();
    }

    void FixedUpdate()
    {
        if (!generating || currentTrail == null) return;

        float dist = Vector3.Distance(rigidBody.position, lastPoint);
        if (dist >= pointSpacing)
        {
            AddPoint(transform.position);
        }
    }

    private void AddPoint(Vector3 point)
    {
        // Snap to ground
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 20f))
            point.y = hit.point.y + 0.05f;
        else
            point.y = 0f;

        currentPoints.Add(point);
        currentTrail.positionCount = currentPoints.Count;
        currentTrail.SetPosition(currentPoints.Count - 1, point);
        lastPoint = point;

        pointsSinceLastBake++;
        if (pointsSinceLastBake >= pointsPerBake)
        {
            BakeCollider();
            pointsSinceLastBake = 0;
        }
    }

    private void BakeCollider()
    {
        if (currentTrail.positionCount < 2) return;

        bakedMesh = new Mesh { name = "FlameTrailColliderMesh" };
        currentTrail.BakeMesh(bakedMesh, true);

        // Add vertical thickness
        Vector3[] verts = bakedMesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i].y += colliderHeight * 0.5f; // raise mesh upward
        }
        bakedMesh.vertices = verts;
        bakedMesh.RecalculateBounds();
        bakedMesh.RecalculateNormals();

        // Assign mesh to MeshCollider
        if (meshCollider == null)
        {
            meshCollider = currentTrail.transform.parent.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = currentTrail.transform.parent.gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = bakedMesh;
        meshCollider.convex = false;
    }

    public void StartBoostTrail()
    {
        GameObject newTrail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        currentTrail = newTrail.GetComponentInChildren<LineRenderer>();

        currentTrail.startWidth = trailWidth;
        currentTrail.endWidth = trailWidth;
        currentPoints.Clear();
        pointsSinceLastBake = 0;

        AddPoint(transform.position);
        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
        currentTrail = null;
        currentPoints.Clear();
        meshCollider = null;
        bakedMesh = null;
    }

    public bool IsGenerating() => generating;

    // ---------------- Debug Gizmos ----------------
    private void OnDrawGizmos()
    {
        if (!drawColliderGizmos || bakedMesh == null) return;

        Gizmos.color = gizmoColor;

        Vector3[] verts = bakedMesh.vertices;
        int[] tris = bakedMesh.triangles;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = currentTrail.transform.TransformPoint(verts[tris[i]]);
            Vector3 v1 = currentTrail.transform.TransformPoint(verts[tris[i + 1]]);
            Vector3 v2 = currentTrail.transform.TransformPoint(verts[tris[i + 2]]);

            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v0);
        }
    }
}
