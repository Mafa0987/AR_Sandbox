using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Water : MonoBehaviour
{
    Mesh mesh;
    Calibration calibration;
    Vector3[] vertices;
    float[] heightMap;
    float[] depthMap;
    Vector4[] fluxMap;
    RenderTexture colors;
    Vector2[] uvs;
    public NeuralNetwork nn;

    bool clearWater = false;

    public Material waterMaterial;

    public TerrainGen terrain;

    int[] triangles;
    public int xSize = 512;
    public int zSize = 424;
    float dt;
    public float c = 1f;
    public float a = 1f;

    public ComputeShader WaterCS;
    ComputeBuffer verticesBuffer;
    ComputeBuffer fluxMapBuffer;
    ComputeBuffer heightmapBuffer;
    ComputeBuffer heightmapRawBuffer;
    ComputeBuffer depthMapBuffer;

    // Start is called before the first frame update
    void Start()
    {
        calibration = GameObject.Find("Calibration").GetComponent<Calibration>();
        xSize = terrain.xSize;
        zSize = terrain.zSize;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        InitMesh();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            AddBump();
        }
        UpdateHeights();
        UpdateMesh();
        if (clearWater)
        {
            clearWater = false;
            WaterCS.SetBool("clearWater", clearWater);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            clearWater = true;
            WaterCS.SetBool("clearWater", clearWater);
        }
        heightMap = terrain.heightmap;
        heightmapBuffer.SetData(heightMap);
        WaterCS.Dispatch(2, 512/8, 424/8, 1);
        verticesBuffer.GetData(vertices);
        UpdateMesh();
    }

    void UpdateHeights()
    {
        heightMap = terrain.heightmap;
        heightmapBuffer.SetData(heightMap);
        heightmapRawBuffer.SetData(terrain.heightmapRaw);
        
        dt = Time.fixedDeltaTime;

        WaterCS.SetFloat("dt", dt);
        WaterCS.SetFloat("c", c);
        WaterCS.SetFloat("xSize", xSize);
        WaterCS.SetFloat("zSize", zSize);
        WaterCS.SetFloat("a", a);
        WaterCS.SetFloat("rainHeight", calibration.rainHeight);
        WaterCS.SetInt("handPositionX", nn.handPosition.x);
        WaterCS.SetInt("handPositionY", nn.handPosition.y);
        
        if (nn.predictedLabel == "Open Hand" && nn.probability > 0.9)
        {
            WaterCS.Dispatch(4, 512/8, 424/8, 1);
        }
        WaterCS.Dispatch(0, 512/8, 424/8, 1);
        WaterCS.Dispatch(1, 512/8, 424/8, 1);
        verticesBuffer.GetData(vertices);
        WaterCS.Dispatch(3, 512/8, 424/8, 1);
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        waterMaterial.mainTexture = colors;
    }

    void InitMesh()
    {
        vertices = new Vector3[xSize * zSize];
        fluxMap = new Vector4[xSize * zSize];
        depthMap = new float[xSize * zSize];
        heightMap = new float[xSize * zSize];
        colors = new RenderTexture(xSize, zSize, 24);
        colors.enableRandomWrite = true;
        colors.Create();

        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        fluxMapBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 4);
        heightmapBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        heightmapRawBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        depthMapBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

        WaterCS.SetBuffer(0, "vertices", verticesBuffer);
        WaterCS.SetBuffer(1, "vertices", verticesBuffer);
        WaterCS.SetBuffer(0, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(1, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(0, "heightmap", heightmapBuffer);
        WaterCS.SetBuffer(1, "heightmap", heightmapBuffer);
        WaterCS.SetBuffer(0, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(1, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(3, "depthMap", depthMapBuffer);
        WaterCS.SetTexture(3, "colors", colors);

        //test
        WaterCS.SetBuffer(2, "vertices", verticesBuffer);
        WaterCS.SetBuffer(2, "heightmap", heightmapBuffer);
        WaterCS.SetBuffer(4, "heightmapRaw", heightmapRawBuffer);
        WaterCS.SetBuffer(4, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(2, "depthMap", depthMapBuffer);
        //

        heightMap = terrain.heightmap;
        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                fluxMap[i] = new Vector4(0, 0, 0, 0);
                depthMap[i] = 0;
                vertices[i].y = heightMap[i] + depthMap[i];
                i++;
            }
        }

        CreateTriangles();
        CreateUV();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        verticesBuffer.SetData(vertices);
        fluxMapBuffer.SetData(fluxMap);
        heightmapBuffer.SetData(heightMap);
        depthMapBuffer.SetData(depthMap);
    }

    void CreateTriangles()
    {
        triangles = new int[(xSize - 1) * (zSize - 1) * 6];
        int vert = 0;
        int tris = 0;
        for(int z = 0; z < zSize-1; z++)
        {
            for (int x = 0; x < xSize-1; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize;
                triangles[tris + 5] = vert + xSize + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void AddBump()
    {
        depthMapBuffer.GetData(depthMap);
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float circle = (x - 250) * (x - 250) + (z - 250) * (z - 250);
                if (circle < 5000)
                {
                    depthMap[x + z * xSize] += 0.5f;
                }
            }
        }
        depthMapBuffer.SetData(depthMap);
    }

    void CreateUV()
    {
        uvs = new Vector2[xSize * zSize];
        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                uvs[i] = new Vector2((float)x / (xSize-1), (float)z / (zSize-1));
                i++;
            }
        }
    }

    void OnDestroy()
    {
        verticesBuffer.Release();
        fluxMapBuffer.Release();
        heightmapBuffer.Release();
        heightmapRawBuffer.Release();
        depthMapBuffer.Release();
    }
}
