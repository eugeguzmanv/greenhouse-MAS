using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CutResultsConsumer: Simple helper component that fetches the cut results
/// from APIManager and logs them. Attach this to any GameObject in the scene.
/// It will fetch and print the results on Start and then periodically every
/// `refreshInterval` seconds.
/// </summary>
public class CutResultsConsumer : MonoBehaviour
{
    [Tooltip("Seconds between automatic refreshes (0 = only on Start)")]
    public float refreshInterval = 1.0f;

    [Tooltip("If true, the consumer will only log results when the APIManager fires the CutResultsUpdated event (recommended). If false, it will poll every refreshInterval seconds.)")]
    public bool eventOnly = true;

    private float timer = 0f;

    void Start()
    {
        timer = refreshInterval;
        // Only perform an initial refresh if polling is enabled
        if (refreshInterval > 0f)
            RefreshAndLog();
    }

    void OnEnable()
    {
        // Subscribe to APIManager event if available
        if (APIManager.Instance != null)
            APIManager.Instance.CutResultsUpdated += HandleCutResultsUpdated;
    }

    void OnDisable()
    {
        if (APIManager.Instance != null)
            APIManager.Instance.CutResultsUpdated -= HandleCutResultsUpdated;
    }

    void Update()
    {
        if (eventOnly) return; // do not poll when in event-only mode
        if (refreshInterval <= 0f) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = refreshInterval;
            RefreshAndLog();
        }
    }

    /// <summary>
    /// Public method other objects can call to request the current cut results
    /// and log them. Also useful for calling from editor buttons or other scripts.
    /// </summary>
    public void RefreshAndLog()
    {
        if (APIManager.Instance == null)
        {
            Debug.LogWarning("CutResultsConsumer: APIManager.Instance is null. Make sure APIManager exists in the scene and is initialized.");
            return;
        }

        List<APIManager.ResponseData> cuts = APIManager.Instance.GetCutResults();

        if (cuts == null || cuts.Count == 0)
        {
            Debug.Log("CutResultsConsumer: No cut results available.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("CutResultsConsumer: Current cut results:");
        for (int i = 0; i < cuts.Count; i++)
        {
            var c = cuts[i];
            sb.AppendFormat("[{0}] Coord: ({1},{2}) | Decision: {3} | Prob: {4:F2}\n", i + 1, c.x_coordinate, c.y_coordinate, c.cut_decision, c.probability);
        }
        sb.AppendFormat("Total: {0}", cuts.Count);

        Debug.Log(sb.ToString());
    }

    // Event handler called by APIManager whenever the cut results list changes
    private void HandleCutResultsUpdated(List<APIManager.ResponseData> cuts)
    {
        if (cuts == null || cuts.Count == 0)
        {
            Debug.Log("CutResultsConsumer (event): No cut results available.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("CutResultsConsumer (event): Current cut results:");
        for (int i = 0; i < cuts.Count; i++)
        {
            var c = cuts[i];
            sb.AppendFormat("[{0}] Coord: ({1},{2}) | Decision: {3} | Prob: {4:F2}\n", i + 1, c.x_coordinate, c.y_coordinate, c.cut_decision, c.probability);
        }
        sb.AppendFormat("Total: {0}", cuts.Count);

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Convenience: return a copy of the cut results for other scripts.
    /// </summary>
    public List<APIManager.ResponseData> GetCutResultsCopy()
    {
        if (APIManager.Instance == null) return new List<APIManager.ResponseData>();
        return APIManager.Instance.GetCutResults();
    }
}
