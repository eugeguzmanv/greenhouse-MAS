using UnityEngine;
using System.Collections;


public class Translation : MonoBehaviour
{
    public Vector3 a, b, c, d, e, f, g, h;

    public float speed = 2f;
    public float rotationSpeed = 5f;

    private int state = 0;

    private bool pausado = false; 

    void Update()
    {
        if (pausado) return;  //si está en pausa, NO se mueve ni rota

        if (state == 0)
        {
            MoveTowardsPoint(b);

            if (Vector3.Distance(transform.position, b) < 0.01f)
            {
                state = 1;
                RotateTowards(c);
            }
        }
        else if (state == 1)
        {
            MoveTowardsPoint(c);

            if (Vector3.Distance(transform.position, c) < 0.01f)
            {
                state = 2;
            }
        }
        else if (state == 2)
        {
            MoveTowardsPoint(d);

            if (Vector3.Distance(transform.position, d) < 0.01f)
            {
                state = 3;
            }
        }
        else if (state == 3)
        {
            MoveTowardsPoint(e);

            if (Vector3.Distance(transform.position, e) < 0.01f)
            {
                state = 4;
            }
        }
        else if (state == 4)
        {
            MoveTowardsPoint(f);

            if (Vector3.Distance(transform.position, f) < 0.01f)
            {
                state = 5;
            }
        }
        else if (state == 5)
        {
            MoveTowardsPoint(g);

            if (Vector3.Distance(transform.position, g) < 0.01f)
            {
                state = 6;
            }
        }
        else if (state == 6)
        {
            MoveTowardsPoint(h);

            if (Vector3.Distance(transform.position, h) < 0.01f)
            {
                state = 7;
                // Notify APIManager that this agent finished its route so the
                // cut results can be delivered now.
                if (APIManager.Instance != null)
                {
                    APIManager.Instance.NotifyCutResultsUpdated();
                }
                else
                {
                    Debug.LogWarning("Translation: APIManager.Instance is null, cannot notify cut results.");
                }
            }
        }
    }

    void MoveTowardsPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
        RotateTowards(point);
    }

    void RotateTowards(Vector3 targetPoint)
    {
        Vector3 dir = (targetPoint - transform.position).normalized;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // 🔥 Corrutina para pausar movimiento
    public IEnumerator PausarPor(float segundos)
    {
        pausado = true;
        yield return new WaitForSeconds(segundos);
        pausado = false;
    }
}
