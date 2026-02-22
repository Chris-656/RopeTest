using UnityEngine;
using RopeToolkit;
using Unity.Mathematics;

/// <summary>
/// Switches a RopeConnection between two-way coupling and a pin based on slack (distance between rope particle and connection point).
/// Attach this to the same GameObject that has the `RopeConnection` you want to control (the TwoWay connection on the boat).
/// </summary>
public class RopeSlackSwitcher : MonoBehaviour
{
    [Tooltip("If left empty, finds all RopeConnection components on this GameObject.")]
    public RopeConnection[] connections;

    [Tooltip("Number of rope segments before the connection to check for stretch (uses same principle as Rope.cs).")]
    public int checkSegments = 5;

    [Tooltip("Stretch tolerance multiplier (e.g. 1.0001 = 0.01% stretch).")]
    public float stretchTolerance = 1.0001f;

    class ConnState
    {
        public RopeConnection conn;
        public Rope rope;
        public Rigidbody originalBody;
        public Transform originalTransform;
        public RopeConnectionType originalType;
        public bool isTaut = false;
    }

    ConnState[] states;

    void Awake()
    {
        if (connections == null || connections.Length == 0)
        {
            connections = GetComponents<RopeConnection>();
        }

        if (connections == null || connections.Length == 0)
        {
            Debug.LogError("RopeSlackSwitcher: no RopeConnection found on this GameObject.");
            enabled = false;
            return;
        }

        states = new ConnState[connections.Length];
        for (int i = 0; i < connections.Length; i++)
        {
            var c = connections[i];
            var s = new ConnState();
            s.conn = c;
            s.rope = c.GetComponent<Rope>();
            s.originalType = c.type;
            s.originalBody = c.rigidbodySettings.body;
            s.originalTransform = c.transformSettings.transform;
            states[i] = s;
        }
    }

    void FixedUpdate()
    {
        if (states == null)
            return;

        foreach (var s in states)
        {
            var connection = s.conn;
            var rope = s.rope;
            if (connection == null || rope == null)
                continue;

            // compute attach point (prefer original body transform, otherwise original transform, otherwise current connectionPoint)
            Vector3 attachPointWorld = Vector3.zero;
            if (s.originalBody != null)
            {
                attachPointWorld = s.originalBody.transform.TransformPoint((Vector3)connection.localConnectionPoint);
            }
            else if (s.originalTransform != null)
            {
                attachPointWorld = s.originalTransform.TransformPoint((Vector3)connection.localConnectionPoint);
            }
            else
            {
                var cp = connection.connectionPoint;
                attachPointWorld = new Vector3(cp.x, cp.y, cp.z);
            }

            rope.GetClosestParticle(attachPointWorld, out int particleIndex, out float _);

            bool isStretched = false;
            int checkCount = Mathf.Min(checkSegments, Mathf.Max(1, rope.measurements.particleCount - 1));
            int startIdx = Mathf.Max(0, particleIndex - checkCount);

            for (int idx = startIdx; idx < particleIndex && idx < rope.measurements.particleCount - 1; idx++)
            {
                var p0 = rope.GetPositionAt(idx, true);
                var p1 = rope.GetPositionAt(idx + 1, true);
                float3 delta = new float3(p1.x - p0.x, p1.y - p0.y, p1.z - p0.z);
                float segmentLength = math.length(delta);
                float desiredLength = rope.measurements.particleSpacing;

                if (segmentLength > desiredLength * stretchTolerance)
                {
                    isStretched = true;
                    break;
                }
            }

            if (isStretched != s.isTaut)
            {
                s.isTaut = isStretched;
                if (isStretched)
                {
                    connection.type = RopeConnectionType.TwoWayCouplingBetweenRigidbodyAndRope;
                    var rs = connection.rigidbodySettings;
                    rs.body = s.originalBody;
                    connection.rigidbodySettings = rs;
                }
                else
                {
                    connection.type = RopeConnectionType.PinRopeToTransform;
                    var ts = connection.transformSettings;
                    ts.transform = s.originalTransform != null ? s.originalTransform : (s.originalBody != null ? s.originalBody.transform : null);
                    connection.transformSettings = ts;

                    var rs = connection.rigidbodySettings;
                    rs.body = null;
                    connection.rigidbodySettings = rs;
                }
            }
        }
    }
}
