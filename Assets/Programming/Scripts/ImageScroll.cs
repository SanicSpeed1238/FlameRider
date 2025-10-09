using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
    public enum TextureType
    {
        Material,
        Skybox,
    }
    public TextureType textureType;

    [Range(0.01f, 1f)]
    public float scrollSpeed = 1f;

    private Renderer rend;
    private Material mat;

    private void Start()
    {
        switch (textureType)
        {
            case TextureType.Material:
                rend = GetComponent<Renderer>();
                mat = rend.material;
                break;
            case TextureType.Skybox:
                break;
        }
    }

    private void Update()
    {
        switch (textureType)
        {
            case TextureType.Material:
                float offset = Time.time * scrollSpeed;
                mat.mainTextureOffset = new Vector2(offset / 2, offset);
                break;
            case TextureType.Skybox:
                RenderSettings.skybox.SetFloat("_Rotation", Time.time * scrollSpeed);
                break;
        }
    }
}