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

    // Variables Needed
    private bool generating = false;
    private readonly float collisionHeight = 0.2f;
    private readonly float pointSpacing = 1f;   
    private const int updateInterval = 3;
    private int pointsSinceLastColliderUpdate = 0;
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

    #region Trail Mesh Generation
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
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 20f))
            point.y = hit.point.y + 0.05f;

        var knot = new BezierKnot(point);
        splinePoints.Add(knot);

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

        Mesh trailMesh = BuildTrailMesh(splineContainer, trailWidth);
        MeshFilter mf = currentTrailObj.AddComponent<MeshFilter>();
        MeshRenderer mr = currentTrailObj.AddComponent<MeshRenderer>();
        MeshCollider mc = currentTrailObj.AddComponent<MeshCollider>();

        mf.sharedMesh = trailMesh;
        mr.sharedMaterial = trailMaterial;
        mc.sharedMesh = trailMesh;
        mc.convex = false;
    }

    private Mesh BuildTrailMesh(SplineContainer container, float width)
    {
        if (container == null || container.Spline.Count < 2)
            return null;

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

        int segments = Mathf.Max(2, Mathf.CeilToInt(length / pointSpacing));

        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 center = ToVector3(container.EvaluatePosition(0, t));
            Vector3 tangent = ToVector3(container.EvaluateTangent(0, t)).normalized;
            Vector3 normal = Vector3.up;
            Vector3 lateral = Vector3.Cross(normal, tangent).normalized * (width * 0.5f);
            Vector3 up = 0.5f * collisionHeight * normal;

            // bottom vertices (left/right)
            verts.Add(center - lateral - up);
            verts.Add(center + lateral - up);
            // top vertices (left/right)
            verts.Add(center - lateral + up);
            verts.Add(center + lateral + up);

            uvs.Add(new Vector2(0f, t));
            uvs.Add(new Vector2(1f, t));
            uvs.Add(new Vector2(0f, t));
            uvs.Add(new Vector2(1f, t));

            if (i > 0)
            {
                int baseIndex = i * 4;
                int prevIndex = baseIndex - 4;

                // bottom face
                tris.Add(prevIndex); tris.Add(baseIndex); tris.Add(prevIndex + 1);
                tris.Add(baseIndex); tris.Add(baseIndex + 1); tris.Add(prevIndex + 1);

                // top face
                tris.Add(prevIndex + 2); tris.Add(prevIndex + 3); tris.Add(baseIndex + 2);
                tris.Add(baseIndex + 3); tris.Add(baseIndex + 2); tris.Add(prevIndex + 3);

                // left side
                tris.Add(prevIndex); tris.Add(prevIndex + 2); tris.Add(baseIndex);
                tris.Add(baseIndex); tris.Add(prevIndex + 2); tris.Add(baseIndex + 2);

                // right side
                tris.Add(prevIndex + 1); tris.Add(baseIndex + 1); tris.Add(prevIndex + 3);
                tris.Add(baseIndex + 1); tris.Add(baseIndex + 3); tris.Add(prevIndex + 3);
            }
        }

        // Add start cap
        int first = 0;
        tris.Add(first); tris.Add(first + 1); tris.Add(first + 2);
        tris.Add(first + 1); tris.Add(first + 3); tris.Add(first + 2);

        // Add end cap
        int last = verts.Count - 4;
        tris.Add(last + 2); tris.Add(last + 1); tris.Add(last);
        tris.Add(last + 2); tris.Add(last + 3); tris.Add(last + 1);

        Mesh mesh = new() { name = "FlameTrail_Volume" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static Vector3 ToVector3(Unity.Mathematics.float3 f)
    {
        return new Vector3(f.x, f.y, f.z);
    }
    #endregion
}