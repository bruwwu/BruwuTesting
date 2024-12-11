using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveVis : MonoBehaviour
{
    private MeshFilter meshFilter;
    // Start is called before the first frame update
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = WaterSim.instance.GetWaveHeight(transform.position.x + vertices[i].x);
        }
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
    }
}
