using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.VFX;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Trail Settings")]
    [Range(0.1f, 10f)] public float trailWidth = 1f;
    public VisualEffect flameTrailVFX;
    public GameObject trailPrefab;
    public Material trailMaterial;

    // Trail generation variables
    private bool generating = false;
    private readonly float collisionHeight = 0.2f;
    private readonly float pointSpacing = 1f;
    private Vector3 lastPoint;

    private Rigidbody rigidBody;
    private GameObject currentTrailObj;
    private LineRenderer currentTrailLine;
    private SplineContainer splineContainer;
    private Spline splinePoints;
    private GraphicsBuffer splineBuffer;

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

        flameTrailVFX = currentTrailObj.GetComponent<VisualEffect>();
    }

    public void StopBoostTrail()
    {
        generating = false;
        GenerateMeshCollider();
        currentTrailLine = null;
        splinePoints = null;
        splineContainer = null;

        if (splineBuffer != null)
        {
            flameTrailVFX.SetGraphicsBuffer("SplineBuffer", null);
            splineBuffer.Release();
            splineBuffer = null;
        }
    }

    public bool IsGenerating() => generating;

    void FixedUpdate()
    {
        if (generating && splinePoints != null)
        {
            float dist = Vector3.Distance(rigidBody.position, lastPoint);
            if (dist >= pointSpacing) AddPoint(transform.position);
        }
    }

    #region Trail Mesh & Particle Generation
    private void AddPoint(Vector3 point)
    {
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 20f))
            point.y = hit.point.y + 0.05f;

        splinePoints.Add(new BezierKnot(point));

        if (currentTrailLine != null)
        {
            currentTrailLine.positionCount = splinePoints.Count;
            for (int i = 0; i < splinePoints.Count; i++)
                currentTrailLine.SetPosition(i, splinePoints[i].Position);
        }

        lastPoint = point;

        UpdateVFXBuffer();
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

        UpdateVFXBuffer();
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

            verts.Add(center - lateral - up);
            verts.Add(center + lateral - up);
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

                tris.Add(prevIndex); tris.Add(baseIndex); tris.Add(prevIndex + 1);
                tris.Add(baseIndex); tris.Add(baseIndex + 1); tris.Add(prevIndex + 1);

                tris.Add(prevIndex + 2); tris.Add(prevIndex + 3); tris.Add(baseIndex + 2);
                tris.Add(baseIndex + 3); tris.Add(baseIndex + 2); tris.Add(prevIndex + 3);

                tris.Add(prevIndex); tris.Add(prevIndex + 2); tris.Add(baseIndex);
                tris.Add(baseIndex); tris.Add(prevIndex + 2); tris.Add(baseIndex + 2);

                tris.Add(prevIndex + 1); tris.Add(baseIndex + 1); tris.Add(prevIndex + 3);
                tris.Add(baseIndex + 1); tris.Add(baseIndex + 3); tris.Add(prevIndex + 3);
            }
        }

        int first = 0;
        tris.Add(first); tris.Add(first + 1); tris.Add(first + 2);
        tris.Add(first + 1); tris.Add(first + 3); tris.Add(first + 2);

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

    private void UpdateVFXBuffer()
    {
        if (flameTrailVFX == null || splinePoints == null) return;

        int count = splinePoints.Count;
        if (count == 0) return;

        Vector3[] points = new Vector3[count];
        Transform splineTransform = splineContainer.transform;

        // Convert spline local positions to world positions
        for (int i = 0; i < count; i++)
        {
            Vector3 localPos = splinePoints[i].Position;
            Vector3 worldPos = splineTransform.TransformPoint(localPos);
            points[i] = worldPos;
        }

        // Recreate buffer if needed
        if (splineBuffer != null && splineBuffer.count != count)
        {
            flameTrailVFX.SetGraphicsBuffer("SplineBuffer", null);
            splineBuffer.Release();
            splineBuffer = null;
        }

        if (splineBuffer == null)
        {
            splineBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                count,
                sizeof(float) * 3
            );
        }

        splineBuffer.SetData(points);

        flameTrailVFX.SetGraphicsBuffer("SplineBuffer", splineBuffer);
        flameTrailVFX.SetInt("PointsCount", count);
    }

    private static Vector3 ToVector3(Unity.Mathematics.float3 f) => new(f.x, f.y, f.z);
    #endregion
}