using System.Collections;
using UnityEngine;

public class TomatoInteraction : MonoBehaviour
{
    public static bool AnyInspecting { get; private set; }

    [Header("References")]
    public Transform plantSocket;         // empty on the plant
    public Transform playerHoldPoint;     // empty in the hand (child of Main Camera)
    public Transform inspectPoint;        // empty in the center (child of Main Camera)
    public PlayerMovement playerMovement; // script on First Person Player
    public MouseLook cameraLook;          // script on Main Camera

    [Header("Settings")]
    public float interactDistance = 3f;
    public float inspectRotateSpeed = 4f;     // mouse sensitivity in inspect
    public float inspectRotateSmooth = 10f;   // smoothing of rotation
    public float transitionDuration = 0.25f;  // time to move hand <-> center

    private Rigidbody rb;
    private bool isHeld = false;
    private bool isInspecting = false;
    private bool isTransitioning = false;

    // inspect rotation state
    private float inspectYaw;
    private float inspectPitch;

    // store constant world scale (size) of the tomato
    private Vector3 originalWorldScale;

    private Coroutine transitionRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store the tomato world scale once (even if parent is scaled)
        originalWorldScale = transform.lossyScale;
        AnyInspecting = false;

        AttachToPlant();
        LockCursor();
    }

    void Update()
    {
        if (isTransitioning) return;

        // E = pick up / put back (only when looking at tomato)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // F = toggle inspect (only when held)
        if (isHeld && Input.GetKeyDown(KeyCode.F))
        {
            if (!isInspecting)
                StartInspect();
            else
                EndInspect();
        }

        // Q = drop (only when held)
        if (isHeld && Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    void LateUpdate()
    {
        if (!isInspecting || isTransitioning)
            return;

        // Keep tomato exactly at center of inspect point
        transform.localPosition = Vector3.zero;

        // Mouse deltas
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update target angles (local to inspectPoint)
        inspectYaw   += mouseX * inspectRotateSpeed;
        inspectPitch -= mouseY * inspectRotateSpeed;
        inspectPitch = Mathf.Clamp(inspectPitch, -80f, 80f);

        Quaternion targetRot = Quaternion.Euler(inspectPitch, inspectYaw, 0f);

        // Smooth local rotation (gives "delay" feeling)
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            inspectRotateSmooth * Time.deltaTime
        );
    }

    // ---------------- Core interaction ----------------

    void TryInteract()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            return;

        if (hit.collider.gameObject != gameObject)
            return;

        if (!isHeld && !isInspecting)
        {
            PickUp();
        }
        else if (isHeld && !isInspecting)
        {
            PlaceBack();
        }
        // If inspecting, E does nothing (use F or Q).
    }

    void AttachToPlant()
    {
        rb.isKinematic = true;
        isHeld = false;
        isInspecting = false;
        isTransitioning = false;
        AnyInspecting = false;

        // Parent to plant socket, keep world size
        SetParentKeepWorldScale(plantSocket);
        transform.position = plantSocket.position;
        transform.rotation = plantSocket.rotation;
    }

    void PickUp()
    {
        rb.isKinematic = true;
        isHeld = true;
        isInspecting = false;
        isTransitioning = false;

        // Parent to hold point (under camera), keep world size
        SetParentKeepWorldScale(playerHoldPoint);
        transform.position = playerHoldPoint.position;
        transform.rotation = playerHoldPoint.rotation;
        transform.localPosition = Vector3.zero; // exactly at HoldPoint

        LockCursor();
    }

    void PlaceBack()
    {
        AttachToPlant();
        RestoreControls();
        LockCursor();
    }

    void StartInspect()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        // Freeze player movement / camera look
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraLook != null)     cameraLook.enabled     = false;
        LockCursor();

        // Animate from hand point to inspect point
        transitionRoutine = StartCoroutine(AnimateBetween(playerHoldPoint, inspectPoint, toInspect: true));
    }

    void EndInspect()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        // Animate from inspect point back to hand
        transitionRoutine = StartCoroutine(AnimateBetween(inspectPoint, playerHoldPoint, toInspect: false));
    }

    void Drop()
    {
        // If we were inspecting, restore controls
        if (isInspecting)
        {
            isInspecting = false;
            AnyInspecting = false;
            RestoreControls();
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        isTransitioning = false;

        // Detach and keep world size
        transform.SetParent(null, worldPositionStays: true);
        AdjustLocalScaleForParent(null);

        rb.isKinematic = false;
        isHeld = false;

        LockCursor();
    }

    // ---------------- Animation coroutine ----------------

    IEnumerator AnimateBetween(Transform from, Transform to, bool toInspect)
    {
        isTransitioning = true;
        rb.isKinematic = true;

        // Work fully in world space during animation
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = to.position;
        Quaternion endRot = to.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float u = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, endPos, u);
            transform.rotation = Quaternion.Slerp(startRot, endRot, u);

            yield return null;
        }

        // Now snap to final parent but keep world position/rotation
        transform.position = endPos;
        transform.rotation = endRot;
        transform.SetParent(to, worldPositionStays: true);
        AdjustLocalScaleForParent(to); // keep world size equal to original

        isTransitioning = false;

        if (toInspect)
        {
            // We are now in inspect mode
            transform.localPosition = Vector3.zero;

            Vector3 e = transform.localEulerAngles;
            inspectYaw = e.y;
            inspectPitch = e.x;

            isInspecting = true;
            AnyInspecting = true;
        }
        else
        {
            // Back in the hand
            isInspecting = false;
            isHeld = true;
            RestoreControls();
            LockCursor();
        }
    }

    // ---------------- Helpers ----------------

    /// <summary>
    /// Parents the tomato and adjusts localScale so that world (lossy) scale
    /// stays equal to originalWorldScale.
    /// </summary>
    void SetParentKeepWorldScale(Transform newParent)
    {
        // Move under new parent but keep world position/rotation/scale for now
        transform.SetParent(newParent, worldPositionStays: true);
        AdjustLocalScaleForParent(newParent);
    }

    /// <summary>
    /// Recomputes localScale based on parent's lossyScale so that
    /// transform.lossyScale == originalWorldScale.
    /// </summary>
    void AdjustLocalScaleForParent(Transform parent)
    {
        Vector3 parentScale = (parent != null) ? parent.lossyScale : Vector3.one;

        // Avoid division by zero
        float sx = parentScale.x == 0 ? 1f : parentScale.x;
        float sy = parentScale.y == 0 ? 1f : parentScale.y;
        float sz = parentScale.z == 0 ? 1f : parentScale.z;

        Vector3 newLocalScale = new Vector3(
            originalWorldScale.x / sx,
            originalWorldScale.y / sy,
            originalWorldScale.z / sz
        );

        transform.localScale = newLocalScale;
    }

    void RestoreControls()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraLook != null)     cameraLook.enabled     = true;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}