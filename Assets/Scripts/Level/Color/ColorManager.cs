using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

    [field: Header("Color Settings")]
    [field: SerializeField] private Material baseMaterial { get; set; }
    [field: SerializeField] public CustomColorSetting colorSettings { get; private set; }

    private Material matA { get; set; }
    private Material matB { get; set; }

    //private static readonly int colorProp = Shader.PropertyToID("_BaseColor");
    private static readonly int colorProp = Shader.PropertyToID("_CurrentColor");

    public event Action OnColorsApplied;

    void Awake() {
        matA = new Material(baseMaterial);
        matB = new Material(baseMaterial);

        ApplyColors();
    }

    public Material GetMaterial(ColorType type) => type switch {
        ColorType.ColorA => matA,
        ColorType.ColorB => matB,
        _ => null
    };

    public void ApplyColors() {
        matA.SetColor(colorProp, colorSettings.GetColor(ColorType.ColorA));
        colorSettings.SetColorA(matA.GetColor(colorProp));
        matB.SetColor(colorProp, colorSettings.GetColor(ColorType.ColorB));
        colorSettings.SetColorB(matB.GetColor(colorProp));
        OnColorsApplied?.Invoke();
    }
}