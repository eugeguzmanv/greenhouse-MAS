using UnityEngine;
using System.Collections;

public class RayCastLeft : MonoBehaviour
{
    public LayerMask quedetectar;

    private Transform ultimoDetectado = null;
    private bool enCooldown = false;

    void Update()
    {
        Raycast();
    }

    private void Raycast()
    {
        if (enCooldown) return;

        float maxDistance = 4f;
        RaycastHit hit;
        Ray Rayo = new Ray(transform.position, transform.right * -1); // tomate izquierda
        Debug.DrawRay(Rayo.origin, Rayo.direction * maxDistance, Color.green);

        if (Physics.Raycast(Rayo, out hit, maxDistance, quedetectar))
        {
            if (hit.transform == ultimoDetectado)
                return;

            ultimoDetectado = hit.transform;
            // Debug.Log("Detectado nuevo objeto: " + hit.transform.name);

            // Intentar obtener TomatoProperties (en el objeto o en sus padres)
            TomatoProperties tp = hit.transform.GetComponent<TomatoProperties>();
            if (tp == null)
                tp = hit.transform.GetComponentInParent<TomatoProperties>();

            // Leer y mostrar los datos del tomate detectado
            if (tp != null)
            {
                tp.GetTomatoData();
            }

            // Visual immediate feedback: pintar azul de escaneado (temporal)
            if (tp != null)
            {
                tp.ApplyColor(tp.scannedColor);
            }
            else
            {
                // Si no es un tomate con propiedades, sigue pintando si tiene MeshRenderer
                MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
                if (mr == null) mr = hit.transform.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    var mats = mr.materials;
                    for (int i = 0; i < mats.Length; i++) mats[i].color = Color.blue;
                    mr.materials = mats;
                }
            }

            // Pause movement (si existe Translation)
            Translation mov = GetComponent<Translation>();
            if (mov != null)
            {
                StartCoroutine(mov.PausarPor(4f));
            }
            else
            {
                Debug.LogWarning("RayCast3: Componente 'Translation' no encontrado; no se pausará el movimiento.");
            }

            // Si el objeto tiene TomatoProperties => llamar al servidor con esos valores
            if (tp != null)
            {
                // Mapear a RequestData (nombres deben coincidir con tu API Manager)
                APIManager.RequestData req = new APIManager.RequestData();
                req.fruit_redness = tp.fruit_redness;
                req.fruit_greenness = tp.fruit_greenness;
                req.leaf_health = tp.leaf_health;
                req.spot_count = tp.spot_count;
                req.spot_darkness = tp.spot_darkness;
                req.surface_texture = tp.surface_texture;
                req.size = tp.size;
                req.stem_brownness = tp.stem_brownness;
                req.x_coordinate = tp.x_coordinate;
                req.y_coordinate = tp.y_coordinate;

                // Llamada a la API (callbacks manejan respuesta)
                APIManager.Instance.AnalyzeTomato(req,
                    onSuccess: (res) =>
                    {
                        // Este callback corre en el hilo principal (porque viene de coroutine)
                        string apiResponse = $"=== API Response ===\n" +
                                            $"X Coordinate: {res.x_coordinate}\n" +
                                            $"Y Coordinate: {res.y_coordinate}\n" +
                                            $"Probability: {res.probability}\n" +
                                            $"Cut Decision: {res.cut_decision}\n" +
                                            $"====================";
                        Debug.Log(apiResponse);
                    },
                    onError: (err) =>
                    {
                        Debug.LogError($"API error al analizar tomate: {err}");
                    }
                );
            }

            // Entrar en cooldown para evitar detección inmediata
            StartCoroutine(CooldownDeteccion(0.3f));
        }
        else
        {
            if (ultimoDetectado != null)
            {
                ultimoDetectado = null;
            }
        }
    }

    IEnumerator CooldownDeteccion(float t)
    {
        enCooldown = true;
        yield return new WaitForSeconds(t);
        enCooldown = false;
    }
}
