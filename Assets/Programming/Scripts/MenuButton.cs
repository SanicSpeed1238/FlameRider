using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    private Material materialInstance;
    private float originalSpeed;

    private static readonly int SpeedID = Shader.PropertyToID("_Speed");

    private void Awake()
    {
        Image img = GetComponent<Image>();
        materialInstance = Instantiate(img.material);
        img.material = materialInstance;
        originalSpeed = materialInstance.GetFloat(SpeedID);

        SetSelectedMaterial(false);
    }

    public void SetSelectedMaterial(bool selected)
    {
        if (materialInstance == null) return;

        float targetSpeed = selected ? originalSpeed : 0f;
        materialInstance.SetFloat(SpeedID, targetSpeed);
    }
}