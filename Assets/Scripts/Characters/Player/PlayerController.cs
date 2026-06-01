using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] private LevelManager levelManager { get; set; }

    [field: Header("Components")]
    [field: SerializeField, ReadOnlyField] private CapsuleCollider capCol { get; set; }
    [field: SerializeField, ReadOnlyField] private Rigidbody rb { get; set; }
    [field: SerializeField, ReadOnlyField] private Renderer[] pRenderers { get; set; } = new Renderer[0];
    [field: SerializeField, ReadOnlyField] private Animator anim { get; set; }
    [field: SerializeField, ReadOnlyField] private ParticleSystem runningDust_vfx { get; set; }
    [field: SerializeField, ReadOnlyField] public AudioSource pAudioSource { get; private set; }
    [field: SerializeField] private Transform groundCheck { get; set; }

    [field: Header("Movement")]
    [field: SerializeField] private float moveSpeed { get; set; } = 12f;
    [field: SerializeField] private float jumpForce { get; set; } = 10f;
    [field: Header("Jump")]
    [field: SerializeField] private int maxJumps { get; set; } = 2;
    [field: SerializeField] private float fallMultiplier { get; set; } = 2.5f;
    [field: SerializeField] private float lowJumpMultiplier { get; set; } = 2f;
    [field: SerializeField, ReadOnlyField] private RotationConstraint hipsRotationConstraint {  get; set; }
    [field: SerializeField, ReadOnlyField] private RotationConstraint spineRotationConstraint {  get; set; }
    [field: Header("Somersault settings")]
    [field: SerializeField, Tooltip("Total time of the hip rotation.")] private float hipsDuration { get; set; } = 0.6f;
    [field: SerializeField, Tooltip("Fraction of the time where spine reaches {spinePeakAngle}.")] private float spinePeakTime {  get; set; } = 0.35f;
    [field: SerializeField] private float spinePeakAngle {  get; set; } = 45f;
    [field: SerializeField] private AnimationCurve hipsCurve {  get; set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [field: SerializeField] private AnimationCurve spineCurve {  get; set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [field: Header("Ground Check")]
    [field: SerializeField] private float groundCheckRadius { get; set; } = 0.1f;
    [field: SerializeField] private LayerMask groundMask { get; set; } = 1 << 3;

    [field: Header("Color settings")]
    [field: SerializeField] private ColorType currentColor { get; set; } = ColorType.ColorA;
    [field: SerializeField, ReadOnlyField] private MaterialPropertyBlock propBlock { get; set; }
    private static readonly int colorProp = Shader.PropertyToID("_CurrentColor");

    [field: Header("Camera")]
    [field: SerializeField, ReadOnlyField] private Camera mainCam {  get; set; }
    [field: SerializeField, ReadOnlyField] private Camera backgroundOverlayCamera {  get; set; }
    [field: SerializeField] private float minFov { get; set; } = 60f;
    [field: SerializeField] private float maxFov { get; set; } = 80f;
    [field: SerializeField] private float approachTime { get; set; } = 0.35f;
    [field: SerializeField] private float returnTime { get; set; } = 0.6f;

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private ColorType startingColor { get; set; }
    [field: SerializeField, ReadOnlyField] private bool isGrounded { get; set; } = true;
    [field: SerializeField, ReadOnlyField] private bool jumpBuffered { get; set; }
    [field: SerializeField, ReadOnlyField] private int jumpsRemaining { get; set; }
    [field: SerializeField, ReadOnlyField] private bool jumpHeld { get; set; }
    [field: SerializeField, ReadOnlyField] public bool bigFall { get; set; }    // Called by trigger in scene.
    [field: SerializeField, ReadOnlyField] private Collider[] overlapBuffer { get; set; } = new Collider[4];

    private Coroutine somersaultCoroutine { get; set; }
    private bool alreadySomersault { get; set; }
    private Vector3 hipsOrig { get; set; } = new Vector3(0f, 1f, 0f);
    private Vector3 spineOrig { get; set; } = new Vector3(0f, 0.597640872f, 0f);

    private float fovVelocity = 0f;
    private float targetFov { get; set; }   

#if UNITY_EDITOR
    
    /*
     * Suelo usar este método para automatizar la asignación de propiedades en el inspector en tiempo de edición.
     * Este código se ejecuta cuando se modifica un componente en el inspector. La propiedad `revalidateProperties`
     * sirve para evitar que el código se ejecute constantemente. Se podría considerar dicho bool como un trigger.
     */

    void OnValidate() {
        if (!Application.isPlaying) {

            // Codigo que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage && prefabConnected) {
                if (revalidateProperties)
                    AssingOnValidate(); //Variables que solo se verificaran cuando estan en una escena
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

        if (pRenderers == null || pRenderers.Length == 0) {
            var renderersAux = transform.GetChild(0).GetChild(1).GetComponentsInChildren<Renderer>();

            // Only add the affected renderers to the array.
            pRenderers = renderersAux.Where(r => {
                var mat = r.sharedMaterial;
                return mat != null && mat.name.StartsWith("playerSwapColorShaderMat");
            }).ToArray();
        }

        if (anim == null) {
            anim = transform.GetChild(0).GetChild(1).GetComponent<Animator>();
        }

        if (hipsRotationConstraint == null) {
            // C0 -> __Mesh__ | C1 -> Rogue_Hooded | C0(x3) ->  Rig_Medium, root, hips
            hipsRotationConstraint = transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<RotationConstraint>();
            spineRotationConstraint = hipsRotationConstraint.transform.GetChild(0).GetComponent<RotationConstraint>();
        }

        if (mainCam == null) {
            mainCam = Camera.main;
            backgroundOverlayCamera = mainCam.transform.GetChild(0).GetComponent<Camera>();
        }

        if (pAudioSource == null)
            pAudioSource = GetComponent<AudioSource>();

        if (runningDust_vfx == null)
            runningDust_vfx = GetComponentInChildren<ParticleSystem>();

        revalidateProperties = false;
    }
#endif

    void Awake() {
        propBlock = new MaterialPropertyBlock();
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

    void Update() {
        CameraFovOnFall();

        if (isGrounded && rb.velocity.sqrMagnitude > 1f && !runningDust_vfx.isPlaying) {
            runningDust_vfx.Play();
        }
        else if ((!isGrounded || rb.velocity.sqrMagnitude < 1f) && runningDust_vfx.isPlaying) {
            runningDust_vfx.Stop();
        }
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

    // -- Movement ----------------------------------------------

    #region Movement methods
    void AutoMove() {
        Vector3 vel = rb.velocity;
        vel.z = moveSpeed;
        rb.velocity = vel;

        //anim.SetFloat("VelocityY", rb.velocity.y);
    }

    void HandleJump() {
        if (jumpBuffered && jumpsRemaining > 0) {
            Vector3 vel = rb.velocity;
            vel.y = jumpForce;
            rb.velocity = vel;
            jumpsRemaining--;
            alreadySomersault = false;
        }

        if (jumpBuffered && jumpsRemaining == 0 && !alreadySomersault) {
            StartSomersault();
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
    #endregion

    #region Extra methods
    private void StartSomersault() {
        if (somersaultCoroutine != null) StopCoroutine(somersaultCoroutine);
        somersaultCoroutine = StartCoroutine(DoSomersault());
        alreadySomersault = true;
    }

    private IEnumerator DoSomersault() {
        if (hipsRotationConstraint == null || spineRotationConstraint == null) yield break;

        Transform hips = hipsRotationConstraint.GetSource(0).sourceTransform;
        Transform spine = spineRotationConstraint.GetSource(0).sourceTransform;

        hipsOrig = hips.localEulerAngles;
        spineOrig = spine.localEulerAngles;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, hipsDuration);
        float peakT = Mathf.Clamp01(spinePeakTime);

        hipsRotationConstraint.constraintActive = true;
        spineRotationConstraint.constraintActive = true;

        while (elapsed < duration) {
            float t = elapsed / duration; // 0..1

            float hipsT = hipsCurve.Evaluate(t);
            float spineT = spineCurve.Evaluate(t);

            // HIPS: 0 -> 360 (smooth)
            float hipsAngle = Mathf.Lerp(0f, 360f, hipsT);

            // SPINE: Increase to spinePeakAngle in the first phase (0..peakT), then return to 0.
            float spineAngle;
            if (t <= peakT) {
                float localT = (peakT <= 0f) ? 1f : (t / peakT);
                localT = Mathf.SmoothStep(0f, 1f, localT) * spineCurve.Evaluate(t);
                spineAngle = Mathf.Lerp(0f, spinePeakAngle, localT);
            }
            else {
                float localT = (1f - peakT <= 0f) ? 1f : ((t - peakT) / (1f - peakT));
                localT = Mathf.SmoothStep(0f, 1f, localT) * spineCurve.Evaluate(t);
                spineAngle = Mathf.Lerp(spinePeakAngle, 0f, localT);
            }

            // Apply rotations
            hips.localRotation = Quaternion.Euler(hipsAngle, hipsOrig.y, hipsOrig.z);
            spine.localRotation = Quaternion.Euler(spineAngle, spineOrig.y, spineOrig.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset values to original state.
        ResetSomersaultRotations();

        somersaultCoroutine = null;
    }

    void CameraFovOnFall() {
        if (isGrounded && (bigFall || mainCam.fieldOfView != minFov)) {
            bigFall = false;
            return;
        }

        if (bigFall && !isGrounded)
            targetFov = maxFov;
        else
            targetFov = minFov;

        float smoothTime = (targetFov > mainCam.fieldOfView) ? approachTime : returnTime;
        float fov;

        // Smooths the FOV using SmoothDamp (avoids overshoot and is frame-rate independent).
        fov = Mathf.SmoothDamp(mainCam.fieldOfView, targetFov, ref fovVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
        backgroundOverlayCamera.fieldOfView = mainCam.fieldOfView = fov;
    }
    #endregion

    // -- Input ------------------------------------------
    #region Input related methods
    public void SwitchColor() {
        currentColor = currentColor == ColorType.ColorA ? ColorType.ColorB : ColorType.ColorA;
        UpdateVisuals();

        CheckPlatformColorMismatch();
    }

    void UpdateVisuals() {
        propBlock.SetColor(colorProp, levelManager.colorManager.colorSettings.GetColor(currentColor));
        foreach (Renderer r in pRenderers) {
            r.SetPropertyBlock(propBlock);
        }

        MaterialPropertyBlock tmp = new MaterialPropertyBlock();
        pRenderers[0].GetPropertyBlock(tmp);
        ParticleSystem.MainModule main = runningDust_vfx.main;
        main.startColor = tmp.GetColor(colorProp);
    }

    public void Jump() {
        jumpBuffered = true;
        jumpHeld = true;
        anim.SetTrigger("JumpTrigger");
    }

    public void JumpReleased() {
        jumpHeld = false;
    }

    #endregion

    // -- Checkers ------------------------------------------

    #region Reset methods
    // Called by Trigger Zone when reach the end of the test level.
    public void resetPlayer() {
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

        if (somersaultCoroutine != null) {
            StopCoroutine(somersaultCoroutine);
            somersaultCoroutine = null;
        }

        alreadySomersault = false;
        ResetSomersaultRotations();

        // Reset camera values
        bigFall = false;
        mainCam.fieldOfView = minFov;

        // Reset animator
        GeneralUtilities.ResetAnimators(new List<Animator>() { anim }, this);
    }

    void ResetSomersaultRotations() {
        hipsRotationConstraint.GetSource(0).sourceTransform.localRotation = Quaternion.Euler(0f, hipsOrig.y, hipsOrig.z);
        spineRotationConstraint.GetSource(0).sourceTransform.localRotation = Quaternion.Euler(0f, spineOrig.y, spineOrig.z);
        hipsRotationConstraint.constraintActive = false;
        spineRotationConstraint.constraintActive = false;
    }

    #endregion

    #region Checkers methods
    bool CheckGround() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        anim.SetBool("IsGrounded", isGrounded);
        return isGrounded;
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

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (groundCheck == null) return;

        // Comprueba si hay colisión con la misma llamada que usas en runtime
        bool onGround = isGrounded;

        // Color: verde si hay colisión, rojo si no
        Gizmos.color = onGround ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);

        // Dibuja esfera sólida semitransparente y contorno
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
    #endregion
}