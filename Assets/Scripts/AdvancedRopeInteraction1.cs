using RopeToolkit;
using UnityEngine;

using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AdvancedRopeInteraction1 : MonoBehaviour

{
    public List<Camera> cameras;


    [Tooltip("The mesh to show on the picked particle position. May be empty.")]
    public Mesh pickedMesh;

    [Tooltip("The mesh to show on the target position. May be empty.")]
    public Mesh targetMesh;

    [Tooltip("The material to use for the picked mesh")]
    public Material pickedMaterial;

    [Tooltip("The material to use for the target mesh")]
    public Material targetMaterial;

    [Tooltip("The maximum distance a rope can be picked from")]
    public float maxPickDistance = 2.0f;

    [Tooltip("The max allowable impulse strength to use. If zero, no limit is applied.")]
    public float maxImpulseStrength = 3.0f;

    [Tooltip("The mass multiplier to apply to the pulled rope particle. Increasing the mass multiplier for a particle increases its influence on neighboring particles. As this script pulls a single particle at a time only, it is beneficial to set the mass multiplier above 1 to improve the stability of the overall rope simulation.")]
    public float leverage = 10.0f;

    [Tooltip("The keyboard key to use to split a picked rope. May be set to None to disable this feature.")]
    public KeyCode splitPickedRopeOnKey = KeyCode.Space;


    public float step = 0.1f;
    public List<Rope> ropes;


    protected bool ready;
    protected Rope rope;
    protected int particle;
    protected float distance;
    protected float3 pickedPosition;
    protected float3 targetPosition;

    private bool isDragging = false;
    private InputSystem_Actions controls;
    void OnEnable()
    {
        controls = new InputSystem_Actions();

        if (controls != null)
        {
            controls.Ropes.RopeClick.performed += ctx => OnMouseClicked(ctx);
            controls.Ropes.RopeClick.canceled += ctx => OnMouseReleased();
            controls.Ropes.RopeDragged.performed += ctx => OnMouseDragged(ctx);
            controls.Ropes.Enable();
        }
    }


    void OnDisable()
    {
        if (controls != null)
        {
            controls.Ropes.RopeClick.performed -= ctx => OnMouseClicked(ctx);
            controls.Ropes.RopeClick.canceled -= ctx => OnMouseReleased();
            //controls.Ropes.RopeClick.Disable();
        }
    }
    private void OnMouseDragged(InputAction.CallbackContext ctx)
    {
        // Hole die aktuelle Mausposition
        Vector2 mousePos2D = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f);

        if (rope != null)
        {
            var ray = GetActiveCamera().ScreenPointToRay(mousePos);
            pickedPosition = rope.GetPositionAt(particle);
            targetPosition = ray.GetPoint(distance);
            //PullingRope(mousePos);
            AdjustRopeLength(); // Delta wird hier nicht verwendet
            isDragging = true;
        }
    }

    public void AdjustRopeLength()
    {
        if (rope == null)
            return;

        // Verwende nur die X-Distanz
        float distX = targetPosition.x - pickedPosition.x;
        // Positive Werte straffen, negative strecken
        float lengthStep = step * distX;
        var sim = rope.simulation;
        sim.lengthMultiplier = Mathf.Clamp(sim.lengthMultiplier + lengthStep, 0.1f, 2.0f);
        rope.simulation = sim;
    }

    void PullingRope(Vector3 mousePos)
    {
        rope.SetPositionAt(particle, targetPosition, maxImpulseStrength);

        if (maxImpulseStrength == 0.0f)
        {
            rope.SetMassMultiplierAt(particle, 0.0f);
        }
        else
        {
            rope.SetMassMultiplierAt(particle, leverage);
        }
        rope.SetPositionAt(particle, targetPosition, maxImpulseStrength);
    }

    private void OnMouseReleased()
    {
        Debug.Log($"AdvancedRopeInteraction: OnMouseReleased called");

        rope = null;
        ready = true;
        isDragging = false;
    }
    void Start()
    {
        ready = true;           // Initial state, bereit zum Picken
        rope = null;           // Kein Seil gepickt
    }

    private void OnMouseClicked(InputAction.CallbackContext ctx)
    {
        Debug.Log($"AdvancedRopeInteraction: Maus clicked");
        var cam = GetActiveCamera();

        var mousePos2D = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        var mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f);
        var ray = cam.ScreenPointToRay(mousePos);

        if (ready && rope == null)
        {
            var closestRope = GetClosestRope(ray, out int closestParticleIndex, out float closestDistanceAlongRay);
            if (closestRope != null && closestParticleIndex != -1 && closestRope.GetMassMultiplierAt(closestParticleIndex) > 0.0f)
            {
                rope = closestRope;
                particle = closestParticleIndex;
                distance = closestDistanceAlongRay;
                ready = false;
            }
        }
    }

    public void SplitPickedRope()
    {
        if (rope == null)
        {
            return;
        }

        ropes.Remove(rope);

        var newRopes = new Rope[2];
        rope.SplitAt(particle, newRopes);
        if (newRopes[0] != null) ropes.Add(newRopes[0]);
        if (newRopes[1] != null) ropes.Add(newRopes[1]);

        rope = null;
    }

    Camera GetActiveCamera()
    {
        foreach (var camera in cameras)
        {
            if (camera != null && camera.enabled)
                return camera;
        }

        return Camera.main; // Fallback
    }



    protected Rope GetClosestRope(Ray ray, out int closestParticleIndex, out float closestDistanceAlongRay)
    {
        closestParticleIndex = -1;
        closestDistanceAlongRay = 0.0f;

        var closestRopeIndex = -1;
        var closestDistance = 0.0f;
        foreach (var ropeItem in ropes)
        {
            // Schneller Distanz-Check: kürzeste Entfernung vom Rope-Transform zum Ray
            Vector3 ropePos = ropeItem.transform.position;
            Vector3 rayOrigin = ray.origin;
            Vector3 rayDir = ray.direction.normalized;
            Vector3 toRope = ropePos - rayOrigin;
            float proj = Vector3.Dot(toRope, rayDir);
            Vector3 closestPoint = rayOrigin + rayDir * proj;
            float distToRay = Vector3.Distance(ropePos, closestPoint);
            float maxAllowedDistance = 10f; // Grenzwert, anpassbar
            if (distToRay > maxAllowedDistance)
                continue;

            ropeItem.GetClosestParticle(ray, out int particleIndex, out float distance, out float distanceAlongRay);

            if (distance > maxPickDistance)
                continue;

            if (closestRopeIndex != -1 && distance > closestDistance)
                continue;

            closestRopeIndex = ropes.IndexOf(ropeItem);
            closestParticleIndex = particleIndex;
            closestDistance = distance;
            closestDistanceAlongRay = distanceAlongRay;
        }

        return closestRopeIndex != -1 ? ropes[closestRopeIndex] : null;
    }

    public void Update()
    {

        if (rope != null)
        {
            if (pickedMesh != null && pickedMaterial != null)
            {
                Graphics.DrawMesh(pickedMesh, Matrix4x4.TRS(pickedPosition, Quaternion.identity, Vector3.one * 0.25f), pickedMaterial, 0);
            }
        }
    }
}

