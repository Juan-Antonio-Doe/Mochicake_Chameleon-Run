using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour {

    [field: Header("Components")]
    [field: SerializeField] private InputSettings settings { get; set; }

    [field: Header("Events")]
    [field: SerializeField] public UnityEvent onJumpRequested;
    [field: SerializeField] public UnityEvent onColorSwitchRequested;

    void Update() {
        HandleScreenInput();
    }

    private void HandleScreenInput() {
        if (!Input.GetMouseButtonDown(0)) return;

        bool isLeftHalf = Input.mousePosition.x < Screen.width * 0.5f;

        if (MatchesMode(settings.JumpMode, isLeftHalf))
            onJumpRequested?.Invoke();

        if (MatchesMode(settings.ColorSwitchMode, isLeftHalf))
            onColorSwitchRequested?.Invoke();
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