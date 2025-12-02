using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RayCast2 : MonoBehaviour
{
    public LayerMask quedetectar;

    // guarda el último tomate detectado
    private Transform ultimoDetectado = null;

    // para evitar spam mientras espera
    private bool enCooldown = false;

    void Update()
    {
        Raycast();
    }

    private void Raycast()
    {
        // si está esperando, no hace nada
        if (enCooldown) return;

        float maxDistance = 4f;

        RaycastHit hit;

        // rayo hacia la derecha negativa (como lo tenías)
        Ray Rayo = new Ray(transform.position, transform.right * -1);

        Debug.DrawRay(Rayo.origin, Rayo.direction * maxDistance, Color.green);

        // si pega con algo
        if (Physics.Raycast(Rayo, out hit, maxDistance, quedetectar))
        {
            // si es el mismo tomate que el anterior → no hacer nada
            if (hit.transform == ultimoDetectado)
                return;

            // **Nuevo:** Si detecta un nuevo tomate, guarda su referencia.
            // guardar este tomate
            ultimoDetectado = hit.transform;

            Debug.Log("Detectado nuevo tomate: " + hit.transform.name);

            // pintar materiales
            MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = hit.transform.GetComponentInChildren<MeshRenderer>();

            if (mr != null)
            {
                foreach (Material mat in mr.materials)
                {
                    mat.color = Color.blue;
                }
            }

            // pausar movimiento
            Translation mov = GetComponent<Translation>();
            if (mov != null)
            {
                // **Comentario:** Esto funciona para pausar el objeto.
                StartCoroutine(mov.PausarPor(4f));
            }

            // entrar en cooldown para evitar detecciones durante la pausa
            StartCoroutine(CooldownDeteccion(0.3f));
        }
        else
        {
            // **ARREGLO:** Si el Raycast no detecta nada, resetea el 'ultimoDetectado'. 
            // Esto permite que el siguiente tomate detectado se considere 'nuevo'.
            if (ultimoDetectado != null)
            {
                // **Comentario:** Se resetea 'ultimoDetectado' solo si antes había algo detectado.
                ultimoDetectado = null;
                Debug.Log("Raycast ya no detecta el tomate anterior. Reiniciando ultimoDetectado.");
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