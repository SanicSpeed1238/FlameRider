using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class FlameTrailVFX : MonoBehaviour
{
    [Header("References")]
    public VisualEffect vfxPrefab;     // The VFX Graph prefab
    public float pointSpacing = 1f;

    private GameObject vfxInstance;
    private VisualEffect vfx;
    private SplineContainer splineContainer;
    private Spline spline;
    private Rigidbody rb;
    private GraphicsBuffer pointsBuffer;

    private Vector3 lastPoint;
    private bool generating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartBoostTrail()
    {
        vfxInstance = Instantiate(vfxPrefab.gameObject);
        vfx = vfxInstance.GetComponent<VisualEffect>();

        splineContainer = vfxInstance.AddComponent<SplineContainer>();
        spline = new Spline();
        splineContainer.Spline = spline;

        spline.Clear();
        AddPoint(transform.position);

        generating = true;
    }

    public void StopBoostTrail()
    {
        generating = false;
    }

    void FixedUpdate()
    {
        if (!generating) return;

        float dist = Vector3.Distance(rb.position, lastPoint);
        if (dist >= pointSpacing)
        {
            AddPoint(transform.position);
        }
    }

    private void AddPoint(Vector3 point)
    {
        spline.Add(new BezierKnot(point));
        lastPoint = point;

        // Send knot positions to VFX
        UpdateVFXPoints();
    }

    private void UpdateVFXPoints()
    {
        if (vfx == null || spline == null || spline.Count == 0)
            return;

        Vector3[] knotPositions = new Vector3[spline.Count];
        for (int i = 0; i < spline.Count; i++)
            knotPositions[i] = spline[i].Position;

        // Dispose the old buffer if it exists
        pointsBuffer?.Dispose();

        // Create a new GPU buffer for the positions
        pointsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, knotPositions.Length, sizeof(float) * 3);
        pointsBuffer.SetData(knotPositions);

        // Send the data to the VFX Graph
        vfx.SetInt("PointCount", knotPositions.Length);
        vfx.SetGraphicsBuffer("PointPositions", pointsBuffer);
    }
}