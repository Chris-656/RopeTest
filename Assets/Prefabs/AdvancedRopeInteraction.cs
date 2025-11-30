using RopeToolkit;
using UnityEngine;

public class AdvancedRopeInteraction : SimpleRopeInteraction
{
    public float step = 0.1f;

    // FixedUpdate überschreiben: Nur Seillänge ändern, nicht Partikel verschieben
    protected new void FixedUpdate()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
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
        else
        {
            if (rope != null)
            {
                rope.SetMassMultiplierAt(particle, 1.0f);
                rope = null;
            }
        }

        // Seil wird nicht gezogen, sondern nur gestreckt/gestrafft
        if (rope != null)
        {
             pickedPosition = rope.GetPositionAt(particle);
            float delta = Input.GetAxis("Mouse X");
            AdjustRopeLength(delta);
        }
    }

    protected new void Update()
    {
        base.Update();
        // ...optional weitere Logik...
    }

    /// <summary>
    /// Verändert die Länge des aktuellen Seils (straffen/lockern) anhand der Mausbewegung
    /// </summary>
    /// <param name="delta">Mausbewegung Y</param>
    public void AdjustRopeLength(float delta)
    {
        if (rope == null)
            return;

        float lengthStep = step * delta; // Schrittweite, kann angepasst werden
        var sim = rope.simulation;
        sim.lengthMultiplier = Mathf.Clamp(sim.lengthMultiplier + lengthStep, 0.1f, 2.0f);
        rope.simulation = sim;
    }
}
