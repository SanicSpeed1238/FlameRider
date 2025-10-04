using UnityEngine;

public class FlameTrailObject : MonoBehaviour
{
    public float speedBoost = 1f;
    private GameObject LineRenderer;

    private void Start()
    {
        LineRenderer = GetComponentInChildren<LineRenderer>().gameObject;
        LineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}