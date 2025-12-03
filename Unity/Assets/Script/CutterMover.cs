using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CutterMover: Se suscribe al evento de APIManager y mueve el objeto a cada coordenada recibida en la lista de cortes.
/// Interpreta x_coordinate como X y y_coordinate como Z en Unity.
/// </summary>
public class CutterMover : MonoBehaviour
{
    [Tooltip("Altura fija para el cortador (Y en Unity)")]
    public float fixedY = 0.5f;
    [Tooltip("Tiempo en segundos entre cada movimiento")]
    public float moveDelay = 0f;
    [Tooltip("Velocidad de movimiento")]
    public float moveSpeed = 12.0f;
    [Tooltip("Velocidad de rotación")]
    public float rotateSpeed = 360f;

    private Coroutine moveRoutine;

    void OnEnable()
    {
        if (APIManager.Instance != null)
            APIManager.Instance.CutResultsUpdated += OnCutResultsReceived;
    }

    void OnDisable()
    {
        if (APIManager.Instance != null)
            APIManager.Instance.CutResultsUpdated -= OnCutResultsReceived;
    }

    private void OnCutResultsReceived(List<APIManager.ResponseData> cuts)
    {
        if (cuts == null || cuts.Count == 0) return;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveToCutPoints(cuts));
    }

    private IEnumerator MoveToCutPoints(List<APIManager.ResponseData> cuts)
    {
        foreach (var cut in cuts)
        {
            Vector3 current = transform.position;
            Vector3 target = new Vector3((float)cut.x_coordinate, fixedY, (float)cut.y_coordinate);

            // Mover en X primero
            Vector3 xTarget = new Vector3(target.x, fixedY, current.z);
            yield return StartCoroutine(RotateTowards(xTarget - current));
            yield return StartCoroutine(MoveToPosition(xTarget));
            yield return new WaitForSeconds(moveDelay);

            // Luego mover en Z
            Vector3 zTarget = new Vector3(target.x, fixedY, target.z);
            yield return StartCoroutine(RotateTowards(zTarget - xTarget));
            yield return StartCoroutine(MoveToPosition(zTarget));
            yield return new WaitForSeconds(moveDelay);

            // Cortar según el tipo de decisión
            if (cut.cut_decision == "cut_plant")
            {
                TryDestroyPlantAtPosition(zTarget.x, zTarget.z);
            }
            else if (cut.cut_decision == "cut_neighbors")
            {
                // Cortar la planta principal
                TryDestroyPlantAtPosition(zTarget.x, zTarget.z);
                // Cortar vecinos moviéndose a cada uno
                yield return StartCoroutine(MoveAndCutNeighbors(zTarget.x, zTarget.z));
            }
        }
        Debug.Log("CutterMover: Finished moving to all cut points.");
    }

    // Mueve el cortador a cada vecino válido y lo corta uno por uno
    private IEnumerator MoveAndCutNeighbors(float x, float z)
    {
        float xRange = 3f;
        float zRange = 5f;
        float tolerance = 0.1f;
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        List<Vector3> neighborPositions = new List<Vector3>();
        List<APIManager.ResponseData> cutsList = APIManager.Instance != null ? APIManager.Instance.GetCutResults() : null;

        // Buscar posiciones de vecinos válidos
        foreach (GameObject plant in plants)
        {
            Vector3 pos = plant.transform.position;
            // Ignorar la planta principal (ya cortada)
            if (Mathf.Abs(pos.x - x) < tolerance && Mathf.Abs(pos.z - z) < tolerance)
                continue;
            // Verificar si es vecino (no diagonal)
            if (Mathf.Abs(pos.x - x) <= xRange && Mathf.Abs(pos.z - z) <= zRange)
            {
                // Excluir la planta diagonal exacta (x+-3, z+-3)
                if (Mathf.Abs(pos.x - x) == xRange && Mathf.Abs(pos.z - z) == zRange)
                    continue;
                // Revisar si el vecino está en la lista de cortes
                bool isScheduledForCut = false;
                if (cutsList != null)
                {
                    foreach (var cut in cutsList)
                    {
                        if (Mathf.Abs(cut.x_coordinate - pos.x) < tolerance && Mathf.Abs(cut.y_coordinate - pos.z) < tolerance)
                        {
                            isScheduledForCut = true;
                            break;
                        }
                    }
                }
                if (isScheduledForCut)
                    continue;
                neighborPositions.Add(new Vector3(pos.x, fixedY, pos.z));
            }
        }

        // Moverse a cada vecino y cortarlo
        foreach (var npos in neighborPositions)
        {
            yield return StartCoroutine(RotateTowards(npos - transform.position));
            yield return StartCoroutine(MoveToPosition(npos));
            yield return new WaitForSeconds(moveDelay);
            TryDestroyPlantAtPosition(npos.x, npos.z);
        }
        Debug.Log($"CutterMover: Total neighbors cut: {neighborPositions.Count} for ({x}, {z})");
    }

        // Busca y destruye los vecinos de la planta en la posición x/z indicada
    // Vecinos: plantas con una separación de +-3 en X y +-5 en Z
    private void TryDestroyNeighborPlants(float x, float z)
    {
        float xRange = 3f;
        float zRange = 5f;
        float tolerance = 0.1f;
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        int neighborCount = 0;

        // Obtener la lista de cortes actual (coordenadas a ignorar)
        List<APIManager.ResponseData> cutsList = APIManager.Instance != null ? APIManager.Instance.GetCutResults() : null;

        foreach (GameObject plant in plants)
        {
            Vector3 pos = plant.transform.position;
            // Ignorar la planta principal (ya cortada)
            if (Mathf.Abs(pos.x - x) < tolerance && Mathf.Abs(pos.z - z) < tolerance)
                continue;
            // Verificar si es vecino (no diagonal)
            if (Mathf.Abs(pos.x - x) <= xRange && Mathf.Abs(pos.z - z) <= zRange)
            {
                // Excluir la planta diagonal exacta (x+-3, z+-3)
                if (Mathf.Abs(pos.x - x) == xRange && Mathf.Abs(pos.z - z) == zRange)
                {
                    Debug.Log($"CutterMover: Skipping diagonal neighbor at ({pos.x}, {pos.z})");
                    continue;
                }
                // Revisar si el vecino está en la lista de cortes
                bool isScheduledForCut = false;
                if (cutsList != null)
                {
                    foreach (var cut in cutsList)
                    {
                        if (Mathf.Abs(cut.x_coordinate - pos.x) < tolerance && Mathf.Abs(cut.y_coordinate - pos.z) < tolerance)
                        {
                            isScheduledForCut = true;
                            break;
                        }
                    }
                }
                if (isScheduledForCut)
                {
                    Debug.Log($"CutterMover: Neighbor plant at ({pos.x}, {pos.z}) is scheduled for main cut, skipping.");
                    continue;
                }
                Debug.Log($"CutterMover: Destroying neighbor plant at ({pos.x}, {pos.z})");
                Destroy(plant);
                neighborCount++;
            }
        }
        Debug.Log($"CutterMover: Total neighbors cut: {neighborCount} for ({x}, {z})");
    }

    // Busca y destruye el objeto con tag "Plant" en la posición x/z indicada
    private void TryDestroyPlantAtPosition(float x, float z)
    {
        float tolerance = 0.1f; // margen de error para comparar posiciones
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        foreach (GameObject plant in plants)
        {
            Vector3 pos = plant.transform.position;
            if (Mathf.Abs(pos.x - x) < tolerance && Mathf.Abs(pos.z - z) < tolerance)
            {
                Debug.Log($"CutterMover: Destroying plant at ({pos.x}, {pos.z})");
                Destroy(plant);
                return;
            }
        }
        Debug.LogWarning($"CutterMover: No plant found at ({x}, {z})");
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator RotateTowards(Vector3 direction)
    {
        if (direction == Vector3.zero) yield break;
        // El frente del cortador es -Z, así que queremos que -transform.forward apunte a direction
        Vector3 lookDir = direction.normalized;
        Quaternion targetRot = Quaternion.LookRotation(-lookDir, Vector3.up);
        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }
}
