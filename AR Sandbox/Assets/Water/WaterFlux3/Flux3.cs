using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Flux3 : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    float[] heightMap;
    float[] depthMap;
    Vector4[] fluxMap;

    public TerrainGen terrain;

    int[] triangles;
    public int xSize = 500;
    public int zSize = 500;
    float wavespeed = 2f;
    float spacing = 1f;
    float posDamping = 1f;
    float velDamping = 0.3f;
    float dt;
    public float c = 1f;
    public float a = 1f;
    float pd;
    float vd;

    public ComputeShader WaterCS;
    ComputeBuffer verticesBuffer;
    ComputeBuffer velocityBuffer;
    ComputeBuffer hSumsBuffer;
    ComputeBuffer fluxMapBuffer;
    ComputeBuffer heightMapBuffer;
    ComputeBuffer depthMapBuffer;

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
        pd = Mathf.Min(posDamping * Time.deltaTime, 1.0f);
        vd = Mathf.Max(0f, 1f - velDamping * Time.deltaTime);

        WaterCS.SetFloat("dt", dt);
        WaterCS.SetFloat("c", c);
        WaterCS.SetFloat("pd", pd);
        WaterCS.SetFloat("vd", vd);
        WaterCS.SetFloat("xSize", xSize);
        WaterCS.SetFloat("zSize", zSize);
        WaterCS.SetFloat("a", a);

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
        fluxMap = new Vector4[(xSize + 1) * (zSize + 1)];
        depthMap = new float[(xSize + 1) * (zSize + 1)];
        heightMap = new float[(xSize + 1) * (zSize + 1)];

        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        velocityBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        hSumsBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        fluxMapBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 4);
        heightMapBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        depthMapBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

        WaterCS.SetBuffer(0, "vertices", verticesBuffer);
        WaterCS.SetBuffer(1, "vertices", verticesBuffer);
        WaterCS.SetBuffer(0, "velocity", velocityBuffer);
        WaterCS.SetBuffer(1, "velocity", velocityBuffer);
        WaterCS.SetBuffer(0, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(1, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(0, "heightMap", heightMapBuffer);
        WaterCS.SetBuffer(1, "heightMap", heightMapBuffer);
        WaterCS.SetBuffer(0, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(1, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(0, "hSums", hSumsBuffer);
        WaterCS.SetBuffer(1, "hSums", hSumsBuffer);

        heightMap = terrain.heightmap;
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                fluxMap[i] = new Vector4(0, 0, 0, 0);
                depthMap[i] = 0;
                //heightMap[i] = x/10f;
                vertices[i].y = heightMap[i] + depthMap[i];
                i++;
            }
        }

        CreateTriangles();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        verticesBuffer.SetData(vertices);
        fluxMapBuffer.SetData(fluxMap);
        heightMapBuffer.SetData(heightMap);
        depthMapBuffer.SetData(depthMap);
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
        float sigma = 30;  // Standard deviation of Gaussian bump
        float centerX = xSize / 2f;  // X-coordinate of center of Gaussian bump
        float centerZ = zSize / 2f;  // Z-coordinate of center of Gaussian bump
        
        depthMapBuffer.GetData(depthMap);
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));
                float height = amplitude * Mathf.Exp(-distance * distance / (2f * sigma * sigma));
                depthMap[i] += height;
                i++;
            }
        }
        
        // for (int z = 200; z < 300; z++)
        // {
        //     for (int x = 200; x < 300 ; x++)
        //     {
        //         depthMap[x + z * (xSize + 1)] += 20f;
        //     }
        // }
        depthMapBuffer.SetData(depthMap);
    }

    void OnDestroy()
    {
        verticesBuffer.Release();
        velocityBuffer.Release();
        hSumsBuffer.Release();
        fluxMapBuffer.Release();
        heightMapBuffer.Release();
        depthMapBuffer.Release();
    }
}
