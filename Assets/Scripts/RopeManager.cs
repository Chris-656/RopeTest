using UnityEngine;

using System.Collections.Generic;
using RopeToolkit;

[System.Serializable]
public class RopeData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public float radius;
    public bool isLoop;
    public SimulationData simulation;
    public List<Unity.Mathematics.float3> spawnPoints;
}

[System.Serializable]
public class SimulationData
{
    public float resolution;
    public float massPerMeter;
    public float stiffness;
    public float lengthMultiplier;
    public float gravityMultiplier;
}

public class RopeManager : MonoBehaviour
{

    public Rigidbody anchorRigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject anchor = GameObject.Find("/Ropes/TestAnchorChain/Anchor");
        anchorRigidbody = anchor.GetComponent<Rigidbody>();
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchRigidbody()
    {
      
        if (anchorRigidbody != null)
        {
            anchorRigidbody.isKinematic = !anchorRigidbody.isKinematic;
        }
    }

    public void SaveAllRopesToJson()
    {
        var filePath = "Assets/Ropes/ropes.json";
        var ropes = FindObjectsByType<Rope>(FindObjectsSortMode.None);
        var ropeList = new List<RopeData>();
        foreach (var rope in ropes)
        {
            ropeList.Add(new RopeData
            {
                name = rope.name,
                position = rope.transform.position,
                rotation = rope.transform.rotation,
                radius = rope.radius,
                isLoop = rope.isLoop,
                simulation = new SimulationData
                {
                    resolution = rope.simulation.resolution,
                    massPerMeter = rope.simulation.massPerMeter,
                    stiffness = rope.simulation.stiffness,
                    lengthMultiplier = rope.simulation.lengthMultiplier,
                    gravityMultiplier = rope.simulation.gravityMultiplier
                },
                spawnPoints = rope.spawnPoints
            });
        }
        string json = JsonUtility.ToJson(new RopeListWrapper { ropes = ropeList }, true);

        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"Alle Ropes wurden als JSON gespeichert: {filePath}");
    }
}

[System.Serializable]
public class RopeListWrapper
{
    public List<RopeData> ropes;
}


