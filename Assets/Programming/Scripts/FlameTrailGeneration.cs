using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Flame Trail")]
    [Range(0.1f, 10f)]
    public float trailWidth = 1f;
    public GameObject trailPrefab;

    [Header("Flame Ring")]
    public GameObject ringPrefab;

    // Important References
    private bool generating = false;
    private Rigidbody playerRB;
    private ParticleSystem flameParticles;

    // Trail Generation Variables
    private List<Vector3> points = new();
    private Vector3 lastPoint;
    private int pointsSinceLastBake = 0;
    private readonly int pointsPerBake = 10;
    private readonly float pointSpacing = 1f;
    private readonly float colliderHeight = 1f;

    // Mesh Components
    private Mesh meshReference;
    private MeshFilter meshFilter;
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

        meshFilter = trailObj.GetComponentInChildren<MeshFilter>();
        flameParticles = trailObj.GetComponentInChildren<ParticleSystem>();

        if (meshFilter == null)
        {
            meshFilter = trailObj.AddComponent<MeshFilter>();
            trailObj.AddComponent<MeshRenderer>();
        }

        meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

        meshReference = new Mesh { name = "FlameTrailMesh" };
        meshFilter.mesh = meshReference;

        points.Clear();
        pointsSinceLastBake = 0;
        AddPoint(transform.position);

        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
        BakeMesh();

        points.Clear();
        meshReference = null;
        meshCollider = null;
        meshFilter = null;
    }

    public void SpawnFlameRing(Transform playerTransform)
    {
        Instantiate(ringPrefab, playerTransform.position, playerTransform.rotation);
    }

    #region Point Handling

    private void AddPoint(Vector3 point)
    {
        if (Physics.Raycast(point + playerRB.transform.up, -playerRB.transform.up, out RaycastHit hit, 10f))
        {
            point = hit.point + hit.normal * 0.05f;
        }

        points.Add(point);
        lastPoint = point;

        pointsSinceLastBake++;
        if (pointsSinceLastBake >= pointsPerBake)
        {
            BakeMesh();
            pointsSinceLastBake = 0;
        }
    }

    #endregion

    #region Mesh Baking

    private void BakeMesh()
    {
        if (points.Count < 2) return;

        GenerateRibbonMesh();
        BakeColliderMesh();
    }

    private void GenerateRibbonMesh()
    {
        int vertexCount = points.Count * 4;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[(points.Count - 1) * 12];

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward;
            if (i == 0) forward = points[i + 1] - points[i];
            else if (i == points.Count - 1) forward = points[i] - points[i - 1];
            else forward = points[i + 1] - points[i - 1];

            Vector3 right = Vector3.Cross(forward.normalized, playerRB.transform.up).normalized * trailWidth * 0.5f;
            Vector3 up = playerRB.transform.up.normalized * colliderHeight;

            // Bottom vertices
            vertices[i * 4] = points[i] - right;
            vertices[i * 4 + 1] = points[i] + right;

            // Top vertices (adds height)
            vertices[i * 4 + 2] = vertices[i * 4] + up;
            vertices[i * 4 + 3] = vertices[i * 4 + 1] + up;

            float v = (float)i / (points.Count - 1);

            uvs[i * 4] = new Vector2(0, v);
            uvs[i * 4 + 1] = new Vector2(1, v);
            uvs[i * 4 + 2] = new Vector2(0, v);
            uvs[i * 4 + 3] = new Vector2(1, v);
        }

        int triIndex = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            int vi = i * 4;

            // Bottom face
            triangles[triIndex++] = vi;
            triangles[triIndex++] = vi + 1;
            triangles[triIndex++] = vi + 4;

            triangles[triIndex++] = vi + 4;
            triangles[triIndex++] = vi + 1;
            triangles[triIndex++] = vi + 5;

            // Top face
            triangles[triIndex++] = vi + 2;
            triangles[triIndex++] = vi + 6;
            triangles[triIndex++] = vi + 3;

            triangles[triIndex++] = vi + 3;
            triangles[triIndex++] = vi + 6;
            triangles[triIndex++] = vi + 7;
        }

        meshReference.Clear();
        meshReference.vertices = vertices;
        meshReference.uv = uvs;
        meshReference.triangles = triangles;
        meshReference.RecalculateNormals();
        meshReference.RecalculateBounds();
    }

    private void BakeColliderMesh()
    {
        if (meshCollider != null)
        {
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

            float targetRate = Mathf.Clamp(1000f + (points.Count * 5f), 1000f, 5000f);
            var emission = flameParticles.emission;
            emission.rateOverTime = targetRate;
        }
    }

    #endregion
}

/*
using UnityEngine;
using System.Collections.Generic;

public class FlameTrailGeneration : MonoBehaviour
{
    [Header("Flame Trail")]
    [Range(1f, 10f)]
    public float trailWidth = 1f;
    public GameObject trailPrefab;

    [Header("Flame Ring")]
    public GameObject ringPrefab;

    // Variables Needed
    private bool generating = false;
    private Rigidbody rigidBody;
    private Mesh meshGenerated;
    private MeshCollider meshCollider;
    private ParticleSystem flameParticles;
    private LineRenderer currentTrail;
    private List<Vector3> currentPoints;
    private Vector3 lastPoint;
    private int pointsSinceLastBake = 0;
    private readonly int pointsPerBake = 10;
    private readonly float pointSpacing = 1f;
    private readonly float colliderHeight = 1f; 

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        currentPoints = new List<Vector3>();
    }

    void LateUpdate()
    {
        if (!generating || currentTrail == null) return;

        float dist = Vector3.Distance(rigidBody.position, lastPoint);
        if (dist >= pointSpacing)
        {
            AddPoint(transform.position);
        }
    }

    public bool IsGenerating() => generating;

    public void StartBoostTrail()
    {
        GameObject newTrail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        currentTrail = newTrail.GetComponentInChildren<LineRenderer>();
        flameParticles = newTrail.GetComponentInChildren<ParticleSystem>();

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
        BakeCollider();

        currentTrail = null;
        currentPoints.Clear();

        meshCollider = null;
        meshGenerated = null;
    }

    public void SpawnFlameRing(Transform playerTransform)
    {
        Instantiate(ringPrefab, playerTransform.position, playerTransform.rotation);
    }

    #region Trail Renderer Technical Stuff
    private void AddPoint(Vector3 point)
    {
        if (Physics.Raycast(point + rigidBody.transform.up, -rigidBody.transform.up, out RaycastHit hit, 10f))
        {
            point = hit.point + hit.normal * 0.05f;
        }

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

        meshGenerated = new Mesh { name = "FlameTrailColliderMesh" };
        currentTrail.BakeMesh(meshGenerated, true);

        Vector3[] verts = meshGenerated.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += currentTrail.transform.up * (colliderHeight * 0.5f);
        }

        meshGenerated.vertices = verts;
        meshGenerated.RecalculateBounds();
        meshGenerated.RecalculateNormals();

        if (meshCollider == null)
        {
            meshCollider = currentTrail.transform.parent.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = currentTrail.transform.parent.gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = meshGenerated;
        meshCollider.convex = false;

        var shape = flameParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.meshRenderer = null;
        shape.mesh = meshGenerated;

        float targetRate = 1000f + (currentPoints.Count * 5f);
        targetRate = Mathf.Clamp(targetRate, 1000f, 5000f);
        var emission = flameParticles.emission;
        emission.rateOverTime = targetRate;
    }
    #endregion
}
*/