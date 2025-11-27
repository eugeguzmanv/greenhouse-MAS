using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
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

    // --- 3. DATA MODELS (Must match your Python Pydantic Models) ---
    
    // The data we SEND to the server
    [System.Serializable]
    public class RequestData 
    {
        // Inputs required by API
        public float fruit_redness;
        public float fruit_greenness;
        public float leaf_health;
        public float spot_count;
        public float spot_darkness;
        public float surface_texture;
        public float size;
        public float stem_brownness;

    }

    // The data we RECEIVE from the server
    [System.Serializable]
    public class ResponseData
    {
        // EDIT HERE: Add your specific output parameters
        public float probability; 
        public bool cut_decision;
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