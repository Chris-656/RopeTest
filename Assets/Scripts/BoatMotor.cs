using UnityEngine;

public class BoatMotor : MonoBehaviour
{
    public float motorForce = 400; // Newton
    public Transform startGo; // Transform to apply the force at (assign in Inspector)
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb != null && startGo != null && motorForce != 0)
        {
            // Kraft entlang der X-Achse anwenden an der Position von `startGo`
            Vector3 force = startGo.forward * motorForce;
            rb.AddForceAtPosition(force, startGo.position, ForceMode.Force);
            Debug.Log($"Applied force: {force} at position: {startGo.position}");
        }
    }
}
