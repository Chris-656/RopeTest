using RopeToolkit;
using UnityEngine;

public class RopeController : MonoBehaviour
{
    public Rope rope;
    public float ropeWeight = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rope == null)
        {
            rope = GetComponent<Rope>();
        }

        // int lastIndex = rope.measurements.particleCount - 1;
        // rope.SetMassMultiplierAt(lastIndex, 0f);
        //  rope.SetMassMultiplierAt(0, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Angenommen, rope ist dein Rope-Objekt
        // int lastIndex = rope.measurements.particleCount - 1; // letztes Partikel (am Boot)
        //rope.SetMassMultiplierAt(lastIndex, 0f); // oder z.B. 0.1f für „leicht“

        // Optional: Das andere Ende (z.B. am Poller) fixieren
        // rope.SetMassMultiplierAt(0, 0f);
    }
}
