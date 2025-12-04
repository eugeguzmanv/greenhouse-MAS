using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System; // Required for 'Action' callbacks

public class APIManager : MonoBehaviour
{
    // SINGLETON SETUP (dont touch!!!!)
    public static APIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
    }

    // --- 2. CONFIGURATION 
    [Header("Server Configuration")]
    [Tooltip("Use 127.0.0.1 instead of localhost for better compatibility")]
    public string baseUrl = "http://127.0.0.1:8000"; 
    public string endpoint = "/predict"; // FastAPI endpoint path
    [Tooltip("Request timeout in seconds")]
    public int timeoutSeconds = 10;

    // --- 2B. CUT RESULTS STORAGE ---
    private List<ResponseData> cutResults = new List<ResponseData>();
    // Event fired whenever the cutResults list is updated (after each API response)
    // Subscribers receive a copy of the current list.
    public event Action<List<ResponseData>> CutResultsUpdated;

    // --- Agent readiness coordination ---
    [Header("Coordination")]
    [Tooltip("Number of scout agents that must be ready before dispatching cut results")]
    public int expectedAgents = 2;
    [Tooltip("Seconds to wait for all agents before forcing dispatch")]
    public float readyTimeoutSeconds = 10f;

    private HashSet<string> readyAgents = new HashSet<string>();
    private Coroutine readyTimeoutCoroutine = null;

    // Getter para acceder a los resultados de corte
    public List<ResponseData> GetCutResults()
    {
        return new List<ResponseData>(cutResults); // Retorna una copia
    }

    // Método para obtener cantidad de tomates para cortar
    public int GetCutResultsCount()
    {
        return cutResults.Count;
    }

    // Método para limpiar el historial de cortes
    public void ClearCutResults()
    {
        cutResults.Clear();
        Debug.Log("Historial de cortes limpiado.");
    }

    // Public helper to notify subscribers with the current cut results.
    // Useful when another system (e.g. a patrolling agent) wants to request
    // the latest list manually (for example when it finishes its route).
    public void NotifyCutResultsUpdated()
    {
        try
        {
            CutResultsUpdated?.Invoke(GetCutResults());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error invoking CutResultsUpdated (manual notify): {e.Message}");
        }
    }

    // Register an agent as ready. When the number of unique ready agents reaches
    // expectedAgents, the cut results are dispatched to subscribers. If not all
    // agents arrive within readyTimeoutSeconds, the list is dispatched anyway.
    public void RegisterAgentReady(string agentId)
    {
        if (string.IsNullOrEmpty(agentId)) agentId = "agent";
        if (!readyAgents.Contains(agentId))
        {
            readyAgents.Add(agentId);
            Debug.Log($"APIManager: Agent registered ready: {agentId} ({readyAgents.Count}/{expectedAgents})");
        }

        // Start/restart timeout coroutine
        if (readyTimeoutCoroutine != null) StopCoroutine(readyTimeoutCoroutine);
        readyTimeoutCoroutine = StartCoroutine(ReadyTimeoutCoroutine());

        if (readyAgents.Count >= expectedAgents)
        {
            // All required agents are ready — dispatch and reset
            Debug.Log("APIManager: All expected agents ready, dispatching cut results.");
            NotifyCutResultsUpdated();
            readyAgents.Clear();
            if (readyTimeoutCoroutine != null) { StopCoroutine(readyTimeoutCoroutine); readyTimeoutCoroutine = null; }
        }
    }

    private IEnumerator ReadyTimeoutCoroutine()
    {
        float t = 0f;
        while (t < readyTimeoutSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Debug.LogWarning($"APIManager: Ready timeout reached ({readyTimeoutSeconds}s). Dispatching cut results with {readyAgents.Count}/{expectedAgents} agents.");
        NotifyCutResultsUpdated();
        readyAgents.Clear();
        readyTimeoutCoroutine = null;
    }

    // Método para loguear todos los cortes registrados
    private void LogCutResults()
    {
        if (cutResults.Count == 0)
        {
            Debug.Log("=== LISTA DE CORTES === \nNo hay tomates marcados para cortar.");
            return;
        }

        string logMessage = "=== LISTA DE CORTES ===\n";
        for (int i = 0; i < cutResults.Count; i++)
        {
            ResponseData cut = cutResults[i];
            logMessage += $"[{i + 1}] Coordenadas: ({cut.x_coordinate}, {cut.y_coordinate}) | " +
                         $"Decisión: {cut.cut_decision} | Probabilidad: {cut.probability:F2}\n";
        }
        logMessage += $"Total de tomates para cortar: {cutResults.Count}\n" +
                     $"=====================";
        Debug.Log(logMessage);
    }

    // --- 3. DATA MODELS (Must match your Python Pydantic Models) ---
    
    // The data we SEND to the server
    [System.Serializable]
    public class RequestData 
    {
        // Inputs required by API
        public double fruit_redness;
        public double fruit_greenness;
        public double leaf_health;
        public double spot_count;
        public double spot_darkness;
        public double surface_texture;
        public double size;
        public double stem_brownness;
        public int x_coordinate;
        public int y_coordinate;

    }

    // The data we RECEIVE from the server
    [System.Serializable]
    public class ResponseData
    {
        // EDIT HERE: Add your specific output parameters
        public int x_coordinate;
        public int y_coordinate;
        public float probability; 
        public string cut_decision;
    }

    // --- 4. PUBLIC METHODS (Agents call these) ---

    public void AnalyzeTomato(RequestData data, Action<ResponseData> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PostRequest(baseUrl + endpoint, data, onSuccess, onError));
    }

    // --- 5. INTERNAL NETWORKING LOGIC (The Engine) ---

    private IEnumerator PostRequest(string url, RequestData data, Action<ResponseData> onSuccess, Action<string> onError)
    {
        // Convert data object to JSON string
        string jsonData = JsonUtility.ToJson(data);

        // Setup the Unity Web Request
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            // Apply timeout (seconds)
            request.timeout = timeoutSeconds;
            
            // Important Headers
            request.SetRequestHeader("Content-Type", "application/json");

            // Send and Wait
            yield return request.SendWebRequest();

            // Handle Result
            if (request.result != UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler != null ? request.downloadHandler.text : "";
                long code = request.responseCode;
                string err = request.error;
                Debug.LogError($"API Error: HTTP {code} - {err} - Body: {responseBody}");
                // If the agent provided an error callback, run it
                if (onError != null) onError.Invoke($"HTTP {code}: {err} - {responseBody}");
            }
            else
            {
                // Parse the JSON response
                try 
                {
                    ResponseData result = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                    
                    // Si el corte es necesario, guardar en el historial
                    if (result.cut_decision == "cut_plant" || result.cut_decision == "cut_neighbors")
                    {
                        cutResults.Add(result);
                        Debug.Log($"[CUT RECORDED] {result.cut_decision} at ({result.x_coordinate}, {result.y_coordinate}) - Probability: {result.probability}");
                    }
                    
                    // Loguear la lista actualizada de cortes
                    LogCutResults();
                    
                    // Trigger the success callback
                    onSuccess?.Invoke(result); 
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON Parse Error: {e.Message}");
                    if (onError != null) onError.Invoke(e.Message);
                }
            }
        }
    }
}