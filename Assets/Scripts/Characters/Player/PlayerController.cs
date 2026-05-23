using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("Components")]
    [field: SerializeField, ReadOnlyField] private CapsuleCollider capCol { get; set; }
    [field: SerializeField, ReadOnlyField] private Rigidbody rb { get; set; }
    [field: SerializeField, ReadOnlyField] private Renderer pRenderer { get; set; }
    //[field: SerializeField, ReadOnlyField] private PlayerInput playerInput { get; set; }
    [field: SerializeField] private Transform groundCheck { get; set; }

    [field: Header("Movement")]
    [field: SerializeField] private float moveSpeed { get; set; } = 8f;
    [field: SerializeField] private float jumpForce { get; set; } = 10f;

    [field: Header("Ground Check")]
    [field: SerializeField] private float groundCheckRadius { get; set; } = 0.1f;
    [field: SerializeField] private LayerMask groundMask { get; set; } = 1 << 3;

    [field: Header("Color settings")]
    [field: SerializeField] private ColorType currentColor { get; set; } = ColorType.ColorA;
    [field: SerializeField] private LayerMask colorALayer { get; set; } = 1 << 6;
    [field: SerializeField, ReadOnlyField] private int colorAMask { get; set; }
    [field: SerializeField] private LayerMask colorBLayer { get; set; } = 1 << 7;
    [field: SerializeField, ReadOnlyField] private int colorBMask { get; set; }
    [field: SerializeField] private MaterialPropertyBlock propBlock { get; set; }
    private static readonly int colorProp = Shader.PropertyToID("_BaseColor");

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool isGrounded { get; set; }
    [field: SerializeField, ReadOnlyField] private bool jumpBuffered { get; set; }
    [field: SerializeField, ReadOnlyField] private bool colorSwitchBuffered { get; set; }

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

        if (capCol == null)
            capCol = GetComponentInChildren<CapsuleCollider>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (pRenderer == null)
            pRenderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();

        if (colorAMask != ToLayerIndex(colorALayer))
            colorAMask = ToLayerIndex(colorALayer);

        if (colorBMask != ToLayerIndex(colorBLayer))
            colorBMask = ToLayerIndex(colorBLayer);

        revalidateProperties = false;
    }
#endif

    /*void OnEnable() {
        playerInput.onJumpRequested += () => jumpBuffered = true;
        playerInput.onColorSwitchRequested += () => colorSwitchBuffered = true;
    }*/

    /*void Update() {
        HandleInput();
    }*/

    void FixedUpdate() {
        CheckGround();
        AutoMove();
        HandleJump();
    }

    /*void OnDisable() {
        playerInput.onJumpRequested -= () => jumpBuffered = true;
        playerInput.onColorSwitchRequested -= () => colorSwitchBuffered = true;
    }*/

    // ── Movement ──────────────────────────────────────────────

    void AutoMove() {
        Vector3 vel = rb.velocity;
        vel.z = moveSpeed;
        rb.velocity = vel;
    }

    void HandleJump() {
        if (jumpBuffered && isGrounded) {
            Vector3 vel = rb.velocity;
            vel.y = jumpForce;
            rb.velocity = vel;
        }
        jumpBuffered = false;
    }

    // Called by PlayerInput through UnityEvent in the Inspector
    public void SwitchColor() {
        currentColor = currentColor == ColorType.ColorA ? ColorType.ColorB : ColorType.ColorA;

        gameObject.layer = currentColor == ColorType.ColorA ? colorAMask : colorBMask;

        UpdateVisuals(currentColor == ColorType.ColorA ? Color.red : Color.blue);
    }

    void UpdateVisuals(Color color) {
        propBlock ??= new MaterialPropertyBlock();
        pRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(colorProp, color);
        pRenderer.SetPropertyBlock(propBlock);
    }

    // Called by PlayerInput through UnityEvent in the Inspector
    public void Jump() {
        jumpBuffered = true;
    }

    // [Test] Called by Trigger Zone when reach the end of the test level.
    public void resetPos () {
        transform.position = new Vector3(0, transform.position.y, 0);
    }

    // ── Input ─────────────────────────────────────────────────

    /*void HandleInput() {
        if (!Input.GetMouseButtonDown(0)) return;

        if (Input.mousePosition.x < Screen.width * 0.5f)
            jumpBuffered = true;
        else
            SwitchColor();
    }*/

    // ── Ground Check ──────────────────────────────────────────

    bool CheckGround() {
        return isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    // ── Utilities ──────────────────────────────────────────
    private int ToLayerIndex(LayerMask mask) => Mathf.RoundToInt(Mathf.Log(mask.value, 2));
}