using UnityEngine;

public class StadiumScreen : MonoBehaviour
{
    [Header("Material Reference")]
    [SerializeField] private int materialIndex;
    private Material screenMaterial;

    [Header("Texture References")]
    public Texture renderTexture;
    public Texture standbyTexture;

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (materialIndex >= 0 && materialIndex < rend.materials.Length) screenMaterial = rend.materials[materialIndex];
    }

    public void SetRenderTexture()
    {
        screenMaterial.SetTexture("_Camera_Render", renderTexture);
    }

    public void SetStandbyTexture()
    {
        screenMaterial.SetTexture("_Camera_Render", standbyTexture);
    }
}