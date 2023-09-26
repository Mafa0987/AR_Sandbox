using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterFlux : MonoBehaviour
{
    Mesh mesh;
    Vector3[] waterSurface;
    Vector4[] fluxMap;
    float[] waterHeights;
    float[] terrainHeights;
    public TerrainGen terrain;
    int[] triangles;
    public int numVerticesX = 250;
    public int numVerticesZ = 250;
    public float pipeLength = 1f / 250;
    public Vector2 cellSize = new Vector2(1f / 250, 1f / 250);
    public float pipeArea = 20;
    public float g = 9.81f;
    float dt;

    public ComputeShader WaterCS;
    ComputeBuffer waterSurfaceBuffer;
    ComputeBuffer terrainHeightsBuffer;
    ComputeBuffer waterHeightsBuffer;
    ComputeBuffer fluxMapBuffer;

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

        WaterCS.SetFloat("dt", dt);

        WaterCS.Dispatch(0, 264/8, 264/8, 1);
        WaterCS.Dispatch(1, 264/8, 264/8, 1);
        waterSurfaceBuffer.GetData(waterSurface);
    }

    void UpdateMesh()
    {
        mesh.vertices = waterSurface;
        mesh.RecalculateNormals();
    }

    void InitMesh()
    {
        waterSurface = new Vector3[numVerticesX * numVerticesZ];
        waterHeights = new float[numVerticesX * numVerticesZ];
        terrainHeights = new float[numVerticesX * numVerticesZ];
        fluxMap = new Vector4[numVerticesX * numVerticesZ];

        waterSurfaceBuffer = new ComputeBuffer(waterSurface.Length, sizeof(float) * 3);
        waterHeightsBuffer = new ComputeBuffer(waterHeights.Length, sizeof(float));
        terrainHeightsBuffer = new ComputeBuffer(waterSurface.Length, sizeof(float));
        fluxMapBuffer = new ComputeBuffer(fluxMap.Length, sizeof(float) * 4);

        WaterCS.SetBuffer(0, "waterSurface", waterSurfaceBuffer);
        WaterCS.SetBuffer(1, "waterSurface", waterSurfaceBuffer);
        WaterCS.SetBuffer(0, "waterHeights", waterHeightsBuffer);
        WaterCS.SetBuffer(1, "waterHeights", waterHeightsBuffer);
        WaterCS.SetBuffer(0, "terrainHeights", terrainHeightsBuffer);
        WaterCS.SetBuffer(1, "terrainHeights", terrainHeightsBuffer);
        WaterCS.SetBuffer(0, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(1, "fluxMap", fluxMapBuffer);
        WaterCS.SetFloat("numVerticesX", numVerticesX);
        WaterCS.SetFloat("numVerticesZ", numVerticesZ);
        WaterCS.SetFloat("g", g);
        WaterCS.SetFloat("pipeLength", pipeLength);
        WaterCS.SetFloat("pipeArea", pipeArea);
        WaterCS.SetVector("cellSize", cellSize);


        for (int i = 0, z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                waterSurface[i] = new Vector3(x, 0, z);
                waterHeights[i] = 50;
                terrainHeights[i] = 0;
                i++;
            }
        }
        waterSurfaceBuffer.SetData(waterSurface);
        waterHeightsBuffer.SetData(waterHeights);
        terrainHeightsBuffer.SetData(terrainHeights);

        CreateTriangles();
        mesh.vertices = waterSurface;
        mesh.triangles = triangles;
    }

    void CreateTriangles()
    {
        triangles = new int[(numVerticesX-1) * (numVerticesZ-1) * 6];
        int vert = 0;
        int tris = 0;
        for(int z = 0; z < numVerticesZ-1; z++)
        {
            for (int x = 0; x < numVerticesX-1; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + numVerticesX;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + numVerticesX;
                triangles[tris + 5] = vert + numVerticesX + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void AddBump()
    {
        float amplitude = 5;  // Amplitude of Gaussian bump
        float sigma = 5;  // Standard deviation of Gaussian bump
        float centerX = numVerticesX / 2f;  // X-coordinate of center of Gaussian bump
        float centerZ = numVerticesZ / 2f;  // Z-coordinate of center of Gaussian bump
        
        for (int i = 0, z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));
                float height = amplitude * Mathf.Exp(-distance * distance / (2f * sigma * sigma));
                waterHeights[i] += height;
                i++;
            }
        }
        waterHeightsBuffer.SetData(waterHeights);
    }

    void OnDestroy()
    {
        waterSurfaceBuffer.Release();
        waterHeightsBuffer.Release();
        terrainHeightsBuffer.Release();
        fluxMapBuffer.Release();
    }
}
