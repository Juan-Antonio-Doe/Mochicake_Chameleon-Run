using System;
using UnityEngine;

[Serializable]
public class CustomColorSetting {

    [field: SerializeField] private Color colorA { get; set; } = new Color(1f, 0.87f, 0f); // Yellow
    [field: SerializeField] private Color colorB { get; set; } = new Color(1f, 0.35f, 0f); // Orange

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