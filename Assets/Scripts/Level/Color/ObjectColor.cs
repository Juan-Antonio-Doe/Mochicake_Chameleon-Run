using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectColor : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] private BoxCollider col { get; set; }
    [field: SerializeField, ReadOnlyField] private Renderer cubeRenderer { get; set; }

    [field: Header("Color settings")]
	[field: SerializeField] private ColorType colorType { get; set; } = ColorType.ColorA;
    public ColorType ColorType => colorType;
    [field: SerializeField, ReadOnlyField] private CustomColorSetting colorSettings { get; set; }
    [field: SerializeField] private LayerMask colorALayer { get; set; } = 1 << 6;
    [field: SerializeField, ReadOnlyField] private int colorAMask { get; set; }
    [field: SerializeField] private LayerMask colorBLayer { get; set; } = 1 << 7;
    [field: SerializeField, ReadOnlyField] private int colorBMask { get; set; }

#if UNITY_EDITOR

    void OnValidate() {
        if (!Application.isPlaying) {

            // Código que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage && prefabConnected) {
                if (revalidateProperties)
                    AssingOnValidate();
            }
        }
    }

    void AssingOnValidate() {
        // Code to execute when revalidating properties

        if (col == null)
            col = GetComponent<BoxCollider>();

        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        colorAMask = GeneralUtilities.ToLayerIndex(colorALayer);
        colorBMask = GeneralUtilities.ToLayerIndex(colorBLayer);

        if (colorType != ColorType.None)
            gameObject.layer = colorType == ColorType.ColorA ? colorAMask : colorBMask;
        else
            gameObject.layer = 3; // Ground layer

        revalidateProperties = false;
    }
#endif

    void Start() {
        if (colorType != ColorType.None) {
            cubeRenderer.sharedMaterial = LevelManager.Instance.colorManager.GetMaterial(colorType);
        }
    }
}