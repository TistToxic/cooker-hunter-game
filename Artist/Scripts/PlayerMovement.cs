using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [SerializeField]
    private float forceMagnitude;

    private Vector3 netForce;

    // Update is called once per frame
    void Update()
    {
        // Get direction of player
        netForce.x = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
        netForce.y = 0.0f;
        netForce.z = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));

        // Normalize net force
        if (netForce.magnitude > 0)
        {
            netForce /= netForce.magnitude; // <--- This might not be the magnitude we want, so we normalize the vector then scale it by the magnitude we want
        }
        netForce *= forceMagnitude * Time.fixedDeltaTime;

        // Apply force to player
        rb.AddForce(netForce);
    }
}
