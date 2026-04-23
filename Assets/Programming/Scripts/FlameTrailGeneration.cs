using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Flame Trail")]
    public GameObject trailPrefab;
    public Material trailMaterial;
    [Range(1f, 10f)]
    public float trailWidth = 2f;

    [Header("Flame Ring")]
    public GameObject ringPrefab;

    // Important References
    private bool generating = false;
    private Rigidbody playerRB;
    private ParticleSystem flameParticles;

    // Trail Data

    GameObject currentTrail;

    private Vector3 lastPoint = Vector3.zero;
    private List<Vector3> points = new();
    private List<Vector3> normals = new();

    private List<Vector3> vertexBuffer = new();
    private List<Vector2> uvBuffer = new();
    private List<int> triangleBuffer = new();

    private int pointsSinceLastBake = 0;
    private int colliderBakeCounter = 0;

    private readonly int meshBakeInterval = 2;
    private readonly float pointSpacing = 0.5f;
    private readonly float textureTiling = 0.1f;
    private readonly int colliderBakeInterval = 20;
    private readonly float colliderHeight = 0.5f;

    // Mesh Components
    private Mesh meshReference;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (generating)
        {
            float dist = Vector3.Distance(playerRB.position, lastPoint);
            if (dist >= pointSpacing)
            {
                AddPoint(playerRB.position);
            }
        }
    }

    public bool IsGenerating() => generating;

    public void StartBoostTrail()
    {
        currentTrail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);

        flameParticles = currentTrail.GetComponentInChildren<ParticleSystem>();
        meshRenderer = currentTrail.AddComponent<MeshRenderer>();
        meshFilter = currentTrail.AddComponent<MeshFilter>();
        meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

        meshReference = new Mesh { name = "FlameTrailMesh" };
        meshReference.MarkDynamic();
        meshFilter.sharedMesh = meshReference;
        meshRenderer.material = trailMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

        points.Clear();
        normals.Clear();
        pointsSinceLastBake = 0;

        AddPoint(playerRB.position);

        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
        CreateTrail();

        RepositionTrailPivot();
        currentTrail.GetComponent<FlameTrailObject>().CullParticles();
        points.Clear();
        normals.Clear();       

        meshReference = null;
        meshCollider = null;
        meshFilter = null;
    }

    public void SpawnFlameRing(Transform playerTransform)
    {
        //Instantiate(ringPrefab, playerTransform.position, playerTransform.rotation);
        Debug.Log("Flame Ring Spawn Disabled");
    }

    #region Trail Generation

    private void CreateTrail()
    {
        if (points.Count >= 2)
        {
            GenerateRibbonMesh();

            colliderBakeCounter++;
            if (colliderBakeCounter >= colliderBakeInterval)
            {
                BakeColliderMesh();
                colliderBakeCounter = 0;
            }
            else if (!generating) BakeColliderMesh();
        }
    }
    
    private void AddPoint(Vector3 pointPosition)
    {
        Vector3 normal = playerRB.transform.up;
        if (Physics.Raycast(pointPosition + playerRB.transform.up, -playerRB.transform.up, out RaycastHit hit, 10f))
        {
            pointPosition = hit.point + hit.normal * 0.02f;
            normal = hit.normal;
        }

        if (points.Count > 0 && Vector3.Distance(points[^1], pointPosition) < 0.05f)
            return;
        points.Add(pointPosition);
        normals.Add(normal.normalized);

        lastPoint = pointPosition;
        pointsSinceLastBake++;
        if (pointsSinceLastBake >= meshBakeInterval)
        {
            CreateTrail();
            pointsSinceLastBake = 0;
        }
    }

    private void GenerateRibbonMesh()
    {
        int pointCount = points.Count;
        if (pointCount < 2) return;

        vertexBuffer.Clear();
        uvBuffer.Clear();
        triangleBuffer.Clear();

        float totalDistance = 0f;
        List<float> distances = new() { 0f };

        for (int i = 1; i < pointCount; i++)
        {
            float dist = Vector3.Distance(points[i], points[i - 1]);
            totalDistance += dist;
            distances.Add(totalDistance);
        }

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 forward;
            if (i == 0) forward = points[i + 1] - points[i];
            else if (i == pointCount - 1) forward = points[i] - points[i - 1];
            else forward = points[i + 1] - points[i - 1];

            forward.Normalize();
            Vector3 normal = normals[i];
            if (Mathf.Abs(Vector3.Dot(forward, normal)) > 0.95f) normal = Vector3.up;

            Vector3 right = Vector3.Cross(forward, normal).normalized * (trailWidth * 0.5f);
            Vector3 up = normal * colliderHeight;

            Vector3 basePoint = points[i];
            Vector3 bottomLeft = basePoint - right;
            Vector3 bottomRight = basePoint + right;
            Vector3 topLeft = bottomLeft + up;
            Vector3 topRight = bottomRight + up;

            vertexBuffer.Add(bottomLeft);
            vertexBuffer.Add(bottomRight);
            vertexBuffer.Add(topLeft);
            vertexBuffer.Add(topRight);

            float v = distances[i] * textureTiling;
            uvBuffer.Add(new Vector2(0, v));
            uvBuffer.Add(new Vector2(1, v));
            uvBuffer.Add(new Vector2(0, v));
            uvBuffer.Add(new Vector2(1, v));
        }

        for (int i = 0; i < pointCount - 1; i++)
        {
            int vi = i * 4;

            // Bottom
            triangleBuffer.Add(vi);
            triangleBuffer.Add(vi + 1);
            triangleBuffer.Add(vi + 4);

            triangleBuffer.Add(vi + 4);
            triangleBuffer.Add(vi + 1);
            triangleBuffer.Add(vi + 5);

            // Top
            triangleBuffer.Add(vi + 2);
            triangleBuffer.Add(vi + 6);
            triangleBuffer.Add(vi + 3);

            triangleBuffer.Add(vi + 3);
            triangleBuffer.Add(vi + 6);
            triangleBuffer.Add(vi + 7);
        }

        meshReference.SetVertices(vertexBuffer);
        meshReference.SetUVs(0, uvBuffer);
        meshReference.SetTriangles(triangleBuffer, 0);

        meshReference.RecalculateNormals();
        meshReference.RecalculateBounds();
    }

    private void RepositionTrailPivot()
    {
        if (meshReference == null || currentTrail == null)
            return;

        meshReference.RecalculateBounds();
        Vector3 center = points[0];
        currentTrail.transform.position += center;

        Vector3[] verts = meshReference.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] -= center;
        }
        meshReference.vertices = verts;
        meshReference.RecalculateBounds();

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshReference;
        }
    }

    private void BakeColliderMesh()
    {
        if (meshCollider != null || meshReference.vertexCount < 3)
        {
            meshReference.RecalculateBounds();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshReference;
            meshCollider.convex = false;
        }

        if (flameParticles != null)
        {
            var shape = flameParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.mesh = meshReference;

            float targetRate = Mathf.Clamp(100f + (points.Count * 10f), 100f, 10000f);
            var emission = flameParticles.emission;
            emission.rateOverTime = targetRate;
        }
    }

    #endregion
}