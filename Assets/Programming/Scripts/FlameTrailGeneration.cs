using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Trail Settings")]
    [Range(0.1f, 10f)] public float trailWidth = 1f;
    public GameObject trailPrefab;
    public Material trailMaterial;
    public PhysicsMaterial colliderMaterial;

    // Variables Needed
    private bool generating = false;
    private readonly float pointSpacing = 1f;
    private Vector3 lastPoint;
    private Rigidbody rigidBody;
    private GameObject currentTrailObj;
    private LineRenderer currentTrailLine;
    private SplineContainer splineContainer;
    private Spline splinePoints;  

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void StartBoostTrail()
    {
        currentTrailObj = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        splineContainer = currentTrailObj.AddComponent<SplineContainer>();
        splinePoints = new Spline();
        splineContainer.Spline = splinePoints;

        currentTrailLine = currentTrailObj.GetComponentInChildren<LineRenderer>();
        if (currentTrailLine == null) currentTrailLine = currentTrailObj.AddComponent<LineRenderer>();

        currentTrailLine.startWidth = trailWidth;
        currentTrailLine.endWidth = trailWidth;
        currentTrailLine.alignment = LineAlignment.TransformZ;
        currentTrailLine.material = trailMaterial;
        currentTrailLine.positionCount = 0;

        splinePoints.Clear();
        AddPoint(transform.position);
        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
        GenerateMeshCollider();
        currentTrailLine = null;
        splinePoints = null;
        splineContainer = null;
    }

    public bool IsGenerating() => generating;

    #region Trail Mesh Generation (idk what's going on here lol)
    void FixedUpdate()
    {
        if (generating && splinePoints != null)
        {
            float dist = Vector3.Distance(rigidBody.position, lastPoint);
            if (dist >= pointSpacing) AddPoint(transform.position);
        }
    }
    private void AddPoint(Vector3 point)
    {
        // Snap to Ground
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 20f))
            point.y = hit.point.y + 0.05f;

        var knot = new BezierKnot(point);
        splinePoints.Add(knot);

        // Update Line Renderer
        if (currentTrailLine != null)
        {
            currentTrailLine.positionCount = splinePoints.Count;
            for (int i = 0; i < splinePoints.Count; i++)
                currentTrailLine.SetPosition(i, splinePoints[i].Position);
        }

        lastPoint = point;
    }
    private void GenerateMeshCollider()
    {
        if (splineContainer == null || splinePoints == null || splinePoints.Count < 2)
            return;

        // Create Mesh Components
        Mesh trailMesh = BuildTrailMesh(splineContainer, trailWidth);
        MeshFilter mf = currentTrailObj.AddComponent<MeshFilter>();
        MeshRenderer mr = currentTrailObj.AddComponent<MeshRenderer>();
        MeshCollider mc = currentTrailObj.AddComponent<MeshCollider>();

        mf.sharedMesh = trailMesh;
        mr.sharedMaterial = trailMaterial;
        mc.sharedMesh = trailMesh;
        mc.sharedMaterial = colliderMaterial;
        mc.convex = false;
    }
    private Mesh BuildTrailMesh(SplineContainer container, float width)
    {
        if (container == null || container.Spline.Count < 2)
            return null;

        // Estimate spline length by summing distances between knots
        float length = 0f;
        int knotCount = Mathf.Max(2, container.Spline.Count);
        Vector3 prev = ToVector3(container.EvaluatePosition(0, 0f));
        for (int k = 1; k < knotCount; ++k)
        {
            float tKnot = (float)k / (knotCount - 1);
            Vector3 p = ToVector3(container.EvaluatePosition(0, tKnot));
            length += Vector3.Distance(prev, p);
            prev = p;
        }

        // Number of segments along the spline to build the ribbon mesh
        int segments = Mathf.Max(2, Mathf.CeilToInt(length / pointSpacing));

        var verts = new List<Vector3>((segments + 1) * 2);
        var tris = new List<int>(segments * 6);
        var uvs = new List<Vector2>((segments + 1) * 2);

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;

            // Evaluate world-space position and tangent from the container
            // Returns Unity.Mathematics.float3, so convert to Vector3
            Vector3 center = ToVector3(container.EvaluatePosition(0, t));
            Vector3 tangent = ToVector3(container.EvaluateTangent(0, t)).normalized;

            // Assuming mostly horizontal ground; compute lateral vector
            Vector3 normal = Vector3.up;
            Vector3 lateral = Vector3.Cross(normal, tangent);
            if (lateral.sqrMagnitude <= 1e-6f) lateral = Vector3.right; // avoid degenerate cross
            lateral = lateral.normalized * (width * 0.5f);

            verts.Add(center - lateral);
            verts.Add(center + lateral);

            uvs.Add(new Vector2(0f, t));
            uvs.Add(new Vector2(1f, t));

            if (i > 0)
            {
                int baseIndex = i * 2;
                // tri 1
                tris.Add(baseIndex - 2);
                tris.Add(baseIndex - 1);
                tris.Add(baseIndex);
                // tri 2
                tris.Add(baseIndex + 1);
                tris.Add(baseIndex);
                tris.Add(baseIndex - 1);
            }
        }

        Mesh mesh = new() { name = "FlameTrail_Ribbon" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
    private static Vector3 ToVector3(Unity.Mathematics.float3 f)
    {
        // Helper conversion from Unity.Mathematics.float3 (returned by EvaluatePosition/Tangent)
        // to UnityEngine.Vector3. EvaluatePosition/EvaluateTangent return float3 types.
        return new Vector3(f.x, f.y, f.z);
    }
    #endregion
}