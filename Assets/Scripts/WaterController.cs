using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public float waveSpeed = 1.0f;
    public float waveHeight = 0.1f;
    public float waveLength = 1.0f; 

    private Vector3[] _baseHeight;
    private MeshFilter _meshFilter;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        var mesh = GetComponent<MeshFilter>().mesh;
        _baseHeight = mesh.vertices;
    }

    private void Update()
    {
        var mesh = _meshFilter.mesh;
        var vertices = mesh.vertices;

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            vertex.y = _baseHeight[i].y + Mathf.Sin((Time.time * waveSpeed + _baseHeight[i].x) * 2 * Mathf.PI / waveLength) * waveHeight;
            vertices[i] = vertex;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}