using UnityEngine;

[CreateAssetMenu(fileName = "ColorPresets", menuName = "Scriptables/Settings/Color Presets")]
public class ColorPresets : ScriptableObject {

    [field: Header("Color A Presets")]
    [field: SerializeField]
    private Color[] colorAPresets { get; set; } = {
        new Color(1f,    0.843f, 0f),    // Yellow
        new Color(1f,    0.4f,   0f),    // Orange
        new Color(0.91f, 0.125f, 0.125f) // Red
    };

    [field: Header("Color B Presets")]
    [field: SerializeField]
    private Color[] colorBPresets { get; set; } = {
        new Color(0.482f, 0.184f, 1f),   // Purple
        new Color(0f,     0.8f,   1f),   // Cyan
        new Color(0.102f, 0.831f, 0.353f)// Green
    };

    public Color GetColorA(int index) => colorAPresets[index];
    public Color GetColorB(int index) => colorBPresets[index];
}