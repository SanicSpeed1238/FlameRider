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

    void FixedUpdate()
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
        if (Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 10f))
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

        meshGenerated = new Mesh { name = "FlameTrailColliderMesh" };
        currentTrail.BakeMesh(meshGenerated, true);

        Vector3[] verts = meshGenerated.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i].y += colliderHeight * 0.5f;
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