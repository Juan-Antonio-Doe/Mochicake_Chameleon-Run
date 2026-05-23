using System;
using UnityEngine;

[Serializable]
public class CustomColorSetting {

    [field: SerializeField] private Color colorA { get; set; } = new Color(1f, 0.5f, 0f); // Orange
    [field: SerializeField] private Color colorB { get; set; } = new Color(0.5f, 0f, 1f); // Purple

    public Color ColorA => colorA;
    public Color ColorB => colorB;

    public void SetColorA(Color color) => colorA = color;
    public void SetColorB(Color color) => colorB = color;

    public Color GetColor(ColorType type) => type switch {
        ColorType.ColorA => colorA,
        ColorType.ColorB => colorB,
        _ => Color.black  // ColorType.None
    };
}