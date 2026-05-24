using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] private PlayerController playerController { get; set; }

    [field: Header("Components")]
    [field: SerializeField] private InputSettings settings { get; set; }

#if UNITY_EDITOR
    /*
     * Suelo usar este método para automatizar la asignación de propiedades en el inspector en tiempo de edición.
     * Este código se ejecuta cuando se modifica un componente en el inspector. La propiedad `revalidateProperties`
     * sirve para evitar que el código se ejecute constantemente. Se podría considerar dicho bool como un trigger.
     */

    void OnValidate() {
        if (!Application.isPlaying) {

            // Código que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            //bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage /*&& prefabConnected*/) {
                if (revalidateProperties)
                    AssingOnValidate();
            }
        }
    }

    void AssingOnValidate() {
        // Code to execute when revalidating properties

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        revalidateProperties = false;
    }
#endif

    void Update() {
        HandleScreenInput();
    }

    private void HandleScreenInput() {
        if (Input.GetMouseButtonUp(0))
            playerController.JumpReleased();

        if (!Input.GetMouseButtonDown(0)) return;

        bool isLeftHalf = Input.mousePosition.x < Screen.width * 0.5f;

        if (MatchesMode(settings.JumpMode, isLeftHalf))
            playerController.Jump();

        if (MatchesMode(settings.ColorSwitchMode, isLeftHalf))
            playerController.SwitchColor();
    }

    private bool MatchesMode(InputMode mode, bool isLeftHalf) {
        return mode switch {
            InputMode.Fullscreen => true,
            InputMode.HalfLeft => isLeftHalf,
            InputMode.HalfRight => !isLeftHalf,
            InputMode.Button => false,  // Se gestiona desde el botón UI
            _ => false
        };
    }
}