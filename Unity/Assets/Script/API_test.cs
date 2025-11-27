using UnityEngine;
using System;

/// <summary>
/// Small helper to test the FastAPI <-> Unity connection.
/// Attach to any GameObject, set `autoSend` to true (or use the context menu),
/// then run Play in the Editor to send one test request.
/// </summary>
public class TestAPIClient : MonoBehaviour
{
    [Tooltip("Send automatically on Start (Editor Play)")]
    public bool autoSend = true;

    // Example sample values you can edit in the Inspector
    public float fruit_redness = 0.5f;
    public float fruit_greenness = 0.2f;
    public float leaf_health = 0.9f;
    public float spot_count = 0f;
    public float spot_darkness = 0f;
    public float surface_texture = 0.3f;
    public float size = 1.0f;
    public float stem_brownness = 0.1f;

    private void Start()
    {
        if (autoSend)
        {
            SendTestRequest();
        }
    }

    [ContextMenu("Send Test Request")]
    public void SendTestRequest()
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager.Instance is null - make sure an APIManager component exists in the scene.");
            return;
        }

        var req = new APIManager.RequestData()
        {
            fruit_redness = fruit_redness,
            fruit_greenness = fruit_greenness,
            leaf_health = leaf_health,
            spot_count = spot_count,
            spot_darkness = spot_darkness,
            surface_texture = surface_texture,
            size = size,
            stem_brownness = stem_brownness
        };

        APIManager.Instance.AnalyzeTomato(req, OnSuccess, OnError);
    }

    private void OnSuccess(APIManager.ResponseData res)
    {
        Debug.Log($"[TestAPIClient] Success - probability: {res.probability}, cut_decision: {res.cut_decision}");
    }

    private void OnError(string err)
    {
        Debug.LogError($"[TestAPIClient] Error: {err}");
    }
}

