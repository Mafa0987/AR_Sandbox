using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Water : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;

    int[] triangles;
    public int xSize = 500;
    public int zSize = 500;
    public float wavespeed = 2f;
    public float spacing = 1f;
    public float posDamping = 1f;
    public float velDamping = 0.3f;
    float dt;
    float c;
    float pd;
    float vd;

    public ComputeShader WaterCS;
    ComputeBuffer verticesBuffer;
    ComputeBuffer velocityBuffer;
    ComputeBuffer hSumsBuffer;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        InitMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddBump();
        }
        UpdateHeights();
        UpdateMesh();
    }

    void UpdateHeights()
    {
        dt = Time.deltaTime;
        c = wavespeed * wavespeed / spacing / spacing;
        pd = Mathf.Min(posDamping * Time.deltaTime, 1.0f);
        vd = Mathf.Max(0f, 1f - velDamping * Time.deltaTime);

        WaterCS.SetFloat("dt", dt);
        WaterCS.SetFloat("c", c);
        WaterCS.SetFloat("pd", pd);
        WaterCS.SetFloat("vd", vd);
        WaterCS.SetFloat("xSize", xSize);
        WaterCS.SetFloat("zSize", zSize);

        WaterCS.Dispatch(0, 512/8, 512/8, 1);
        WaterCS.Dispatch(1, 512/8, 512/8, 1);
        verticesBuffer.GetData(vertices);
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void InitMesh()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        velocityBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        hSumsBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

        WaterCS.SetBuffer(0, "vertices", verticesBuffer);
        WaterCS.SetBuffer(1, "vertices", verticesBuffer);
        WaterCS.SetBuffer(0, "velocity", velocityBuffer);
        WaterCS.SetBuffer(1, "velocity", velocityBuffer);
        WaterCS.SetBuffer(0, "hSums", hSumsBuffer);
        WaterCS.SetBuffer(1, "hSums", hSumsBuffer);

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x, 1, z);
                i++;
            }
        }

        CreateTriangles();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        verticesBuffer.SetData(vertices);
    }

    void CreateTriangles()
    {
        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for(int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void AddBump()
    {
        float amplitude = 50;  // Amplitude of Gaussian bump
        float sigma = 20;  // Standard deviation of Gaussian bump
        float centerX = xSize / 2f;  // X-coordinate of center of Gaussian bump
        float centerZ = zSize / 2f;  // Z-coordinate of center of Gaussian bump
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));
                float height = amplitude * Mathf.Exp(-distance * distance / (2f * sigma * sigma));
                vertices[i].y += height;
                i++;
            }
        }
        verticesBuffer.SetData(vertices);
    }

    void OnDestroy()
    {
        verticesBuffer.Release();
        velocityBuffer.Release();
        hSumsBuffer.Release();
    }
}
