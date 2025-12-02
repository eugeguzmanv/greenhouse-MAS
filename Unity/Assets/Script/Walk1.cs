using UnityEngine;

public class Walk1 : MonoBehaviour
{
    public float speed = 2f;

    // Update is called once per frame
    void Update()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3 (Horizontal, 0, Vertical);
        transform.position = transform.position + movement*speed*Time.deltaTime;


        if (movement != Vector3.zero) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 5f*Time.deltaTime);

        } // 0,0,0 


    }
}
