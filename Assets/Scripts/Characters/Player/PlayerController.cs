using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] private LevelManager levelManager { get; set; }

    [field: Header("Components")]
    [field: SerializeField, ReadOnlyField] private CapsuleCollider capCol { get; set; }
    [field: SerializeField, ReadOnlyField] private Rigidbody rb { get; set; }
    [field: SerializeField, ReadOnlyField] private Renderer pRenderer { get; set; }
    [field: SerializeField] private Transform groundCheck { get; set; }

    [field: Header("Movement")]
    [field: SerializeField] private float moveSpeed { get; set; } = 8f;
    [field: SerializeField] private float jumpForce { get; set; } = 10f;
    [field: Header("Jump")]
    [field: SerializeField] private int maxJumps { get; set; } = 2;
    [field: SerializeField] private float fallMultiplier { get; set; } = 2.5f;
    [field: SerializeField] private float lowJumpMultiplier { get; set; } = 2f;

    [field: Header("Ground Check")]
    [field: SerializeField] private float groundCheckRadius { get; set; } = 0.1f;
    [field: SerializeField] private LayerMask groundMask { get; set; } = 1 << 3;

    [field: Header("Color settings")]
    [field: SerializeField] private ColorType currentColor { get; set; } = ColorType.ColorA;
    [field: SerializeField] private MaterialPropertyBlock propBlock { get; set; }
    private static readonly int colorProp = Shader.PropertyToID("_BaseColor");

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private ColorType startingColor { get; set; }
    [field: SerializeField, ReadOnlyField] private bool isGrounded { get; set; }
    [field: SerializeField, ReadOnlyField] private bool jumpBuffered { get; set; }
    [field: SerializeField, ReadOnlyField] private int jumpsRemaining { get; set; }
    [field: SerializeField, ReadOnlyField] private bool jumpHeld { get; set; }
    [field: SerializeField, ReadOnlyField] private Collider[] overlapBuffer { get; set; } = new Collider[4];

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

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        if (capCol == null)
            capCol = GetComponentInChildren<CapsuleCollider>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (pRenderer == null)
            pRenderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();

        revalidateProperties = false;
    }
#endif

    void Awake() {
        propBlock = new MaterialPropertyBlock();
        pRenderer.GetPropertyBlock(propBlock);
    }

    void OnEnable() {
        levelManager.colorManager.OnColorsApplied += UpdateVisuals;
    }

    void OnDisable() {
        levelManager.colorManager.OnColorsApplied -= UpdateVisuals;
    }

    void Start() {
        startingColor = currentColor;
        jumpsRemaining = maxJumps;
        //UpdateVisuals();
    }

    void FixedUpdate() {
        bool wasGrounded = isGrounded;
        CheckGround();

        if (!wasGrounded && isGrounded) {
            jumpsRemaining = maxJumps;
        }
        // [Removed] This isn't present in the android game.
        /*else if (wasGrounded && !isGrounded && jumpsRemaining == maxJumps) {
            // Walked off edge without jumping — consume one jump
            jumpsRemaining = maxJumps - 1;
        }*/

        AutoMove();
        HandleJump();
        ApplyGravity();
    }

    void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.CompareTag("Platform")) return;

        float normalY = collision.GetContact(0).normal.y;

        // Side/frontal -> always fail
        if (Mathf.Abs(normalY) < 0.5f) {
            GameEvents.TriggerPlayerFailed();
            return;
        }

        // Top or bottom surface -> check color
        if (!collision.gameObject.TryGetComponent<ObjectColor>(out var platform)) return;

        if (platform.ColorType == ColorType.None || platform.ColorType != currentColor) {
            GameEvents.TriggerPlayerFailed();
            return;
        }
    }

    // ── Movement ──────────────────────────────────────────────

    void AutoMove() {
        Vector3 vel = rb.velocity;
        vel.z = moveSpeed;
        rb.velocity = vel;
    }

    void HandleJump() {
        if (jumpBuffered && jumpsRemaining > 0) {
            Vector3 vel = rb.velocity;
            vel.y = jumpForce;
            rb.velocity = vel;
            jumpsRemaining--;
        }

        jumpBuffered = false;
    }

    void ApplyGravity() {
        float extraGravity = rb.velocity.y < 0
            ? fallMultiplier - 1          // Falling: heavier
            : !jumpHeld ? lowJumpMultiplier - 1  // Rising but button released: cut jump short
            : 0f;                         // Rising and button held: full jump

        if (extraGravity > 0f)
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * extraGravity, ForceMode.Acceleration);
    }

    // ── Input ──────────────────────────────────────────

    public void SwitchColor() {
        currentColor = currentColor == ColorType.ColorA ? ColorType.ColorB : ColorType.ColorA;
        UpdateVisuals();

        CheckPlatformColorMismatch();
    }

    void UpdateVisuals() {
        propBlock.SetColor(colorProp, levelManager.colorManager.colorSettings.GetColor(currentColor));
        pRenderer.SetPropertyBlock(propBlock);
    }

    public void Jump() {
        jumpBuffered = true;
        jumpHeld = true;
    }

    public void JumpReleased() {
        jumpHeld = false;
    }

    // [Test] Called by Trigger Zone when reach the end of the test level.
    public void resetPlayer () {
        // Reset physics state
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = Vector3.zero;

        // Reset player state
        currentColor = startingColor;
        UpdateVisuals();
        jumpBuffered = false;
        jumpHeld = false;
        jumpsRemaining = maxJumps;
    }

    // ── Checkers ──────────────────────────────────────────

    bool CheckGround() {
        return isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
    void CheckPlatformColorMismatch() {
        if (!isGrounded) return;

        int count = Physics.OverlapSphereNonAlloc(groundCheck.position, groundCheckRadius, overlapBuffer, groundMask);

        for (int i = 0; i < count; i++) {
            if (!overlapBuffer[i].TryGetComponent<ObjectColor>(out var platform)) continue;

            if (platform.ColorType == ColorType.None || platform.ColorType != currentColor) {
                GameEvents.TriggerPlayerFailed();
                return;
            }
        }
    }

}