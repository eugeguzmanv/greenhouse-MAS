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

    private float inspectYaw;
    private float inspectPitch;

    private Vector3 originalWorldScale;

    private Coroutine transitionRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalWorldScale = transform.lossyScale;

        AttachToPlant();
        LockCursor();
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();

        if (isHeld && Input.GetKeyDown(KeyCode.F))
        {
            if (!isInspecting) StartInspect();
            else EndInspect();
        }

        if (isHeld && Input.GetKeyDown(KeyCode.Q))
            Drop();
    }

    void LateUpdate()
    {
        if (!isInspecting || isTransitioning) return;

        transform.localPosition = Vector3.zero;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        inspectYaw += mouseX * inspectRotateSpeed;
        inspectPitch -= mouseY * inspectRotateSpeed;
        inspectPitch = Mathf.Clamp(inspectPitch, -80f, 80f);

        Quaternion targetRot = Quaternion.Euler(inspectPitch, inspectYaw, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            inspectRotateSmooth * Time.deltaTime
        );
    }

    // ---------- Interaction Logic ----------

    void TryInteract()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance)) return;
        if (hit.collider.gameObject != gameObject) return;

        if (!isHeld && !isInspecting) PickUp();
        else if (isHeld && !isInspecting) PlaceBack();
    }

    void AttachToPlant()
    {
        rb.isKinematic = true;
        isHeld = false;
        isInspecting = false;
        isTransitioning = false;
        AnyInspecting = false;

        SetParentExact(plantSocket);
    }

    void PickUp()
    {
        rb.isKinematic = true;
        isHeld = true;
        isInspecting = false;
        isTransitioning = false;

        SetParentExact(playerHoldPoint);
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

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraLook != null) cameraLook.enabled = false;

        LockCursor();
        transitionRoutine = StartCoroutine(AnimateBetween(playerHoldPoint, inspectPoint, toInspect: true));
    }

    void EndInspect()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(AnimateBetween(inspectPoint, playerHoldPoint, toInspect: false));
    }

    void Drop()
    {
        if (isInspecting)
        {
            isInspecting = false;
            AnyInspecting = false;
            RestoreControls();
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        isTransitioning = false;

        transform.SetParent(null);
        rb.isKinematic = false;
        isHeld = false;

        LockCursor();
    }

    // ---------- Animation ----------

    IEnumerator AnimateBetween(Transform from, Transform to, bool toInspect)
    {
        isTransitioning = true;
        rb.isKinematic = true;

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

        SetParentExact(to);

        isTransitioning = false;

        if (toInspect)
        {
            transform.localPosition = Vector3.zero;
            Vector3 e = transform.localEulerAngles;
            inspectYaw = e.y;
            inspectPitch = e.x;

            isInspecting = true;
            AnyInspecting = true;
        }
        else
        {
            isInspecting = false;
            isHeld = true;
            RestoreControls();
            LockCursor();
        }
    }

    // ---------- Helpers ----------

    void SetParentExact(Transform parent)
    {
        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        AdjustLocalScaleForParent(parent);
    }

    void AdjustLocalScaleForParent(Transform parent)
    {
        Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;

        float px = parentScale.x == 0 ? 1f : parentScale.x;
        float py = parentScale.y == 0 ? 1f : parentScale.y;
        float pz = parentScale.z == 0 ? 1f : parentScale.z;

        transform.localScale = new Vector3(
            originalWorldScale.x / px,
            originalWorldScale.y / py,
            originalWorldScale.z / pz
        );
    }

    void RestoreControls()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraLook != null) cameraLook.enabled = true;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
