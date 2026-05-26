using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] private PlayerController playerController { get; set; }

    [field: Header("Components")]
    [field: SerializeField] private InputSettings settings { get; set; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool pressStartedOnUI { get; set; }

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
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

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
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#elif UNITY_ANDROID
    HandleTouchInput();
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void HandleMouseInput() {
        bool mouseDown = Input.GetMouseButtonDown(0);
        bool mouseUp = Input.GetMouseButtonUp(0);

        if (mouseDown)
            pressStartedOnUI = EventSystem.current.IsPointerOverGameObject();

        if (pressStartedOnUI) return; // Block ALL input if press started on UI

        if (mouseUp)
            playerController.JumpReleased();

        if (!mouseDown) return;

        bool isLeftHalf = Input.mousePosition.x < Screen.width * 0.5f;

        if (MatchesMode(settings.JumpMode, isLeftHalf))
            playerController.Jump();

        if (MatchesMode(settings.ColorSwitchMode, isLeftHalf))
            playerController.SwitchColor();
    }
#endif

#if UNITY_ANDROID
private void HandleTouchInput() {
    if (Input.touchCount == 0) return;

    Touch touch = Input.GetTouch(0);

    if (touch.phase == TouchPhase.Began)
        pressStartedOnUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);

    //Debug.Log($"Touch phase: {touch.phase}, Position: {touch.position}, PressStartedOnUI: {pressStartedOnUI}, FingerId: {touch.fingerId");

    if (pressStartedOnUI) return;

    bool isLeftHalf = touch.position.x < Screen.width * 0.5f;

    switch (touch.phase) {
        case TouchPhase.Began:
            if (MatchesMode(settings.JumpMode, isLeftHalf))
                playerController.Jump();
            if (MatchesMode(settings.ColorSwitchMode, isLeftHalf))
                playerController.SwitchColor();
            break;
        case TouchPhase.Ended:
        case TouchPhase.Canceled:
            playerController.JumpReleased();
            break;
    }
}
#endif

    /*private void HandleScreenInput() {
        if (Input.GetMouseButtonUp(0))
            playerController.JumpReleased();

        if (!Input.GetMouseButtonDown(0)) return;

        bool isLeftHalf = Input.mousePosition.x < Screen.width * 0.5f;

        if (MatchesMode(settings.JumpMode, isLeftHalf))
            playerController.Jump();

        if (MatchesMode(settings.ColorSwitchMode, isLeftHalf))
            playerController.SwitchColor();
    }*/

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