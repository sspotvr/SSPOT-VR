using UnityEngine;

public class MaterialColorController : MonoBehaviour {

    public FlexibleColorPicker fcp;
    public Material material;

    private void Start() {
        fcp.color = material.color;
        fcp.onColorChange.AddListener(OnChangeColor);
    }

    private void OnChangeColor(Color co) {
        material.color = co;
    }
}
