using UnityEngine;
using UnityEngine.UI;

public class InteractionPointer : MonoBehaviour
{
    public Camera playerCamera;
    public Image pointerImage;

    public float maxDistance = 3f;

    // Size settings
    public Vector2 normalSize = new Vector2(8f, 8f);
    public Vector2 highlightSize = new Vector2(16f, 16f);
    public float sizeLerpSpeed = 10f;   // how fast it animates

    private Vector2 currentSize;
    private Vector2 targetSize;

    void Start()
    {
        if (pointerImage == null) return;

        currentSize = normalSize;
        targetSize  = normalSize;

        // Initial size
        pointerImage.rectTransform.sizeDelta = currentSize;
        // Color stays whatever you set in the Image component
    }

    void Update()
    {
        if (playerCamera == null || pointerImage == null)
            return;

        // Hide pointer while inspecting the tomato
        if (TomatoInteraction.AnyInspecting)
        {
            pointerImage.enabled = false;
            return;
        }
        else
        {
            pointerImage.enabled = true;
        }

        // 1. Check if we are aiming at something interactable
        bool canInteract = false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.GetComponent<TomatoInteraction>() != null)
            {
                canInteract = true;
            }
        }

        // 2. Set target size (NO color change)
        targetSize = canInteract ? highlightSize : normalSize;

        // 3. Animate size smoothly
        currentSize = Vector2.Lerp(
            currentSize,
            targetSize,
            sizeLerpSpeed * Time.deltaTime
        );

        pointerImage.rectTransform.sizeDelta = currentSize;
    }
}

