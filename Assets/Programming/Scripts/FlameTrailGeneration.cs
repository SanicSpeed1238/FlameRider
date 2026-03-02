using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Flame Trail")]
    public GameObject trailPrefab;
    public Material trailMaterial;
    [Range(0.1f, 10f)]
    public float trailWidth = 1f;

    [Header("Flame Ring")]
    public GameObject ringPrefab;

    // Important References
    private bool generating = false;
    private Rigidbody playerRB;
    private ParticleSystem flameParticles;

    // Trail Generation Variables
    private List<Vector3> points = new();
    private List<Vector3> vertexBuffer = new();
    private List<Vector2> uvBuffer = new();
    private List<int> triangleBuffer = new();
    private Vector3 lastPoint = Vector3.zero;
    private int pointsSinceLastBake = 0;
    private int colliderBakeCounter = 0;
    private readonly int meshBakeInterval = 2;
    private readonly float pointSpacing = 0.5f;
    private readonly int colliderBakeInterval = 10;
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
                AddPoint(transform.position);
            }
        }
    }

    public bool IsGenerating() => generating;

    public void StartBoostTrail()
    {
        GameObject trailObj = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);

        flameParticles = trailObj.GetComponentInChildren<ParticleSystem>();
        meshRenderer = trailObj.AddComponent<MeshRenderer>();
        meshFilter = trailObj.AddComponent<MeshFilter>();
        meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

        meshReference = new Mesh { name = "FlameTrailMesh" };
        meshReference.MarkDynamic();
        meshFilter.sharedMesh = meshReference;
        meshRenderer.material = trailMaterial;       

        points.Clear();
        pointsSinceLastBake = 0;
        AddPoint(transform.position);       

        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
        CreateTrail();

        points.Clear();
        meshReference = null;
        meshCollider = null;
        meshFilter = null;
    }

    public void SpawnFlameRing(Transform playerTransform)
    {
        Instantiate(ringPrefab, playerTransform.position, playerTransform.rotation);
    }

    #region Technical Trail Generation Stuff

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
        }     
    }

    private void AddPoint(Vector3 point)
    {
        if (Physics.Raycast(point + playerRB.transform.up, -playerRB.transform.up, out RaycastHit hit, 10f))
        {
            point = hit.point + hit.normal * 0.05f;
        }

        points.Add(point);
        lastPoint = point;

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

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 forward;
            if (i == 0) forward = points[i + 1] - points[i];
            else if (i == pointCount - 1) forward = points[i] - points[i - 1];
            else forward = points[i + 1] - points[i - 1];

            Vector3 right = 0.5f * trailWidth * Vector3.Cross(forward.normalized, playerRB.transform.up).normalized;
            Vector3 up = playerRB.transform.up.normalized * colliderHeight;

            Vector3 bottomLeft = points[i] - right;
            Vector3 bottomRight = points[i] + right;
            Vector3 topLeft = bottomLeft + up;
            Vector3 topRight = bottomRight + up;

            vertexBuffer.Add(bottomLeft);
            vertexBuffer.Add(bottomRight);
            vertexBuffer.Add(topLeft);
            vertexBuffer.Add(topRight);

            float v = (float)i / (pointCount - 1);

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

    private void BakeColliderMesh()
    {
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = meshReference;
            meshCollider.convex = false;
        }

        if (flameParticles != null)
        {
            var shape = flameParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.mesh = meshReference;

            float targetRate = Mathf.Clamp(1000f + (points.Count * 5f), 1000f, 5000f);
            var emission = flameParticles.emission;
            emission.rateOverTime = targetRate;
        }
    }

    #endregion
}