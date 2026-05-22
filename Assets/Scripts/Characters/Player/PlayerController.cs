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
    [field: SerializeField] private Transform groundCheck { get; set; }

    [field: Header("Movement")]
    [field: SerializeField] private float moveSpeed { get; set; } = 8f;
    [field: SerializeField] private float jumpForce { get; set; } = 10f;

    [field: Header("Ground Check")]
    [field: SerializeField] private float groundCheckRadius { get; set; } = 0.1f;
    [field: SerializeField] private LayerMask groundMask { get; set; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool isGrounded { get; set; }
    [field: SerializeField, ReadOnlyField] private bool jumpRequested { get; set; }

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

        revalidateProperties = false;
    }
#endif

    void Update() {
        HandleInput();
    }

    void FixedUpdate() {
        CheckGround();
        AutoMove();
        HandleJump();
    }

    // ── Movement ──────────────────────────────────────────────

    void AutoMove() {
        Vector3 vel = rb.velocity;
        vel.z = moveSpeed;
        rb.velocity = vel;
    }

    void TryJump() {
        if (!isGrounded) return;

        Vector3 vel = rb.velocity;
        vel.y = jumpForce;
        rb.velocity = vel;
    }

    void HandleJump() {
        if (jumpRequested && isGrounded) {
            Vector3 vel = rb.velocity;
            vel.y = jumpForce;
            rb.velocity = vel;
        }
        jumpRequested = false;
    }

    void SwitchColor() {
        // Next step
    }

    // ── Input ─────────────────────────────────────────────────

    void HandleInput() {
        if (!Input.GetMouseButtonDown(0)) return;

        if (Input.mousePosition.x < Screen.width * 0.5f)
            jumpRequested = true;
        //TryJump();
        else
            SwitchColor();
    }

    // ── Ground Check ──────────────────────────────────────────

    bool CheckGround() {
        return isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
}