using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "InputSettings", menuName = "Scriptables/Settings/Input Settings")]
public class InputSettings : ScriptableObject {

    [field: Header("Jump")]

    [field: SerializeField]
    private InputMode jumpMode { get; set; } = InputMode.HalfLeft;
    /*public InputMode JumpMode {
        get { return jumpMode; }
        set {
            if (value == InputMode.Fullscreen) {
                colorSwitchMode = InputMode.Button;
            }
            else if (value == InputMode.HalfLeft) {
                colorSwitchMode = InputMode.HalfRight;
            }
            else if (value == InputMode.HalfRight) {
                colorSwitchMode = InputMode.HalfLeft;
            }
            jumpMode = value;
        }
    }*/

    [field: Header("Color Switch")]
    [field: SerializeField] 
    private InputMode colorSwitchMode { get; set; } = InputMode.HalfRight;
    /*public InputMode ColorSwitchMode {
        get { return colorSwitchMode; }
        set {
            if (value == InputMode.Fullscreen) {
                jumpMode = InputMode.Button;
            }
            else if (value == InputMode.HalfLeft) {
                jumpMode = InputMode.HalfRight;
            }
            else if (value == InputMode.HalfRight) {
                jumpMode = InputMode.HalfLeft;
            }
            colorSwitchMode = value;
        }
    }*/

    public InputMode JumpMode => jumpMode;
    public InputMode ColorSwitchMode => colorSwitchMode;

#if UNITY_EDITOR
    private InputMode prevJumpMode;
    private InputMode prevColorSwitchMode;

    private void OnEnable() {
        prevJumpMode = jumpMode;
        prevColorSwitchMode = colorSwitchMode;
    }

    private void OnValidate() {
        if (jumpMode != prevJumpMode)
            EnforceConstraints(changedJump: true);
        else if (colorSwitchMode != prevColorSwitchMode)
            EnforceConstraints(changedJump: false);

        prevJumpMode = jumpMode;
        prevColorSwitchMode = colorSwitchMode;
    }
#endif

    public void SetJumpMode(InputMode mode) {
        jumpMode = mode;
        EnforceConstraints(changedJump: true);
    }

    public void SetColorSwitchMode(InputMode mode) {
        colorSwitchMode = mode;
        EnforceConstraints(changedJump: false);
    }

    // [Prototype] Called by 'InputSettings_Btn' on Settings UI for just switch screen layout.
    public void SimpleSwitchMode(Text inputLayoutText) {
        if (jumpMode == InputMode.HalfLeft) {
            SetJumpMode(InputMode.HalfRight);
            SetColorSwitchMode(InputMode.HalfLeft);
            inputLayoutText.text = "C | J";
        }
        else if (jumpMode == InputMode.HalfRight) {
            SetJumpMode(InputMode.HalfLeft);
            SetColorSwitchMode(InputMode.HalfRight);
            inputLayoutText.text = "J | C";
        }
    }

    private void EnforceConstraints(bool changedJump) {
        if (changedJump) {
            colorSwitchMode = jumpMode switch {
                InputMode.Fullscreen => InputMode.Button,
                InputMode.HalfLeft => InputMode.HalfRight,
                InputMode.HalfRight => InputMode.HalfLeft,
                _ => colorSwitchMode   // Button: doesn't constrain the other mode
            };
        }
        else {
            jumpMode = colorSwitchMode switch {
                InputMode.Fullscreen => InputMode.Button,
                InputMode.HalfLeft => InputMode.HalfRight,
                InputMode.HalfRight => InputMode.HalfLeft,
                _ => jumpMode
            };
        }
    }
}