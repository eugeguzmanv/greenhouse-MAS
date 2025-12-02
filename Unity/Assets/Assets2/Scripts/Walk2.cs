using UnityEngine;

public class Walk2 : MonoBehaviour
{
    public float Velocitymove = 5f;
    public float VelocityRotation = 100f;



    void Update()
    {
        // Movimiento hacia adelante
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Velocitymove * Time.deltaTime;
        }

        // Movimiento hacia atrás
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Velocitymove * Time.deltaTime;
        }

        // Rotación izquierda
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -VelocityRotation * Time.deltaTime);
        }

        // Rotación derecha
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, VelocityRotation * Time.deltaTime);
        }

   
    }

   
}
