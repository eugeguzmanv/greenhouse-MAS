using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RayCast3 : MonoBehaviour
{
    public LayerMask quedetectar;

    // AÑADIDO: Guarda el último objeto detectado para evitar spam.
    private Transform ultimoDetectado = null;

    // AÑADIDO: Para evitar que la detección se repita inmediatamente durante la pausa.
    private bool enCooldown = false;

    void Update()
    {
        Raycast();
    }

    private void Raycast()
    {
        // AÑADIDO: Si está en cooldown (esperando), no hace nada.
        if (enCooldown) return;

        // Distancia máxima a la que el raycast va a detectar objetos.
        float maxDistance = 4f;

        // Donde se guardará la información si el rayo choca contra algo.
        RaycastHit hit;

        // Creamos un rayo que sale desde la posición del objeto
        // y avanza en la dirección hacia la derecha (transform.right).
        Ray Rayo = new Ray(transform.position, transform.right);

        // Dibuja en la escena un rayo verde para visualizarlo.
        Debug.DrawRay(Rayo.origin, Rayo.direction * maxDistance, Color.green);

        // Lanza el raycast SOLO contra el Layer dado
        if (Physics.Raycast(Rayo, out hit, maxDistance, quedetectar))
        {
            // AÑADIDO: Si es el mismo objeto que el anterior → no hacer nada.
            if (hit.transform == ultimoDetectado)
                return;

            // **NUEVA DETECCIÓN:** Guardamos la referencia.
            ultimoDetectado = hit.transform;
            Debug.Log("Detectado nuevo objeto: " + hit.transform.name);

            // --- LÓGICA DE ACCIÓN (PINTAR Y PARAR) ---

            // Intenta obtener MeshRenderer (en el objeto o sus hijos)
            MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = hit.transform.GetComponentInChildren<MeshRenderer>();

            if (mr != null)
            {
                // Recorremos todos los materiales del objeto
                foreach (Material mat in mr.materials)
                {
                    mat.color = Color.blue;  // pinta todos
                }
            }
            else
            {
                Debug.Log("Objeto detectado sin MeshRenderer.");
            }

            // AÑADIDO: Pausar movimiento.
            Translation mov = GetComponent<Translation>();
            if (mov != null)
            {
                // Comentario: Llama a la corrutina de pausa de 4 segundos, asumiendo que Translation existe.
                StartCoroutine(mov.PausarPor(4f));
            }
            else
            {
                Debug.LogError("RayCast3 requiere el componente 'Translation' en este GameObject para detenerse.");
            }

            // AÑADIDO: Entrar en cooldown para evitar detecciones inmediatas.
            StartCoroutine(CooldownDeteccion(0.3f));
        }
        else
        {
            // AÑADIDO: Si el Raycast no detecta nada, resetea el 'ultimoDetectado'.
            // Esto permite que el siguiente objeto detectado se considere 'nuevo'.
            if (ultimoDetectado != null)
            {
                ultimoDetectado = null;
                // Debug.Log("Raycast despejado. Reiniciando ultimoDetectado para el siguiente.");
            }
        }
    }

    // AÑADIDO: Corrutina de Cooldown (Copia exacta de RayCast2)
    IEnumerator CooldownDeteccion(float t)
    {
        enCooldown = true;
        yield return new WaitForSeconds(t);
        enCooldown = false;
    }
}