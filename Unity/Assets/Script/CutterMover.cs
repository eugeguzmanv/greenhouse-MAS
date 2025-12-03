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
    public float moveDelay = 1.0f;
    [Tooltip("Velocidad de movimiento")]
    public float moveSpeed = 5.0f;

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
            Vector3 target = new Vector3((float)cut.x_coordinate, fixedY, (float)cut.y_coordinate);
            Debug.Log($"CutterMover: Moving to ({target.x}, {target.y}, {target.z}) for cut: {cut.cut_decision}");
            yield return StartCoroutine(MoveToPosition(target));
            yield return new WaitForSeconds(moveDelay);
        }
        Debug.Log("CutterMover: Finished moving to all cut points.");
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
}
