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
    public double fruit_redness = 0.5f;
    public double fruit_greenness = 0.2f;
    public double leaf_health = 0.9f;
    public double spot_count = 0f;
    public double spot_darkness = 0f;
    public double surface_texture = 0.3f;
    public double size = 1.0f;
    public double stem_brownness = 0.1f;
    public int x_coordinate = 0;
    public int y_coordinate = 0;

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
            stem_brownness = stem_brownness,
            x_coordinate = x_coordinate,
            y_coordinate = y_coordinate
        };

        APIManager.Instance.AnalyzeTomato(req, OnSuccess, OnError);
    }

    private void OnSuccess(APIManager.ResponseData res)
    {
        Debug.Log($"[TestAPIClient] Success - x coordinate: {res.x_coordinate}, y coordinate: {res.y_coordinate}, probability: {res.probability}, cut_decision: {res.cut_decision}");

    }

    private void OnError(string err)
    {
        Debug.LogError($"[TestAPIClient] Error: {err}");
    }
}

