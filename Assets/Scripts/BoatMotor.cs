using UnityEngine;

public class BoatMotor : MonoBehaviour
{
    public float motorForce = 400; // Newton
    public Vector3 forceOffset = new Vector3(-1f, 0f, 0f); // local offset where force is applied (meters)
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Kraft entlang der X-Achse anwenden, 1 m links versetzt (Hebelwirkung)
            Vector3 force = Vector3.right * motorForce;
            Vector3 applicationPoint = transform.TransformPoint(forceOffset);
            rb.AddForceAtPosition(force, applicationPoint, ForceMode.Force);
        }
    }
}
