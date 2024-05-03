using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Water : MonoBehaviour
{
    Mesh mesh;
    Calibration calibration;
    public Vector3[] vertices;
    float[] heightMap;
    public float[] depthMap;
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

    public bool addRain = false;

    public bool rainEnabled = false;

    public ComputeShader WaterCS;
    ComputeBuffer verticesBuffer;
    ComputeBuffer fluxMapBuffer;
    ComputeBuffer heightmapBuffer;
    ComputeBuffer heightmapRawBuffer;
    ComputeBuffer depthMapBuffer;
    ComputeBuffer depthMapTemp;
    ComputeBuffer waterNormals;
    public Vector3[] normals;

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
        UpdateHeights();
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
        WaterCS.Dispatch(2, 512/8, 424/8, 1);
        verticesBuffer.GetData(vertices);
        depthMapBuffer.GetData(depthMap);

        UpdateMesh();
        waterNormals.SetData(normals);
        WaterCS.Dispatch(5, (int)Mathf.Ceil(xSize*4/8), (int)Mathf.Ceil(zSize*4/8), 1);
        terrain.material.mainTexture = terrain.colors;
    }

    void UpdateHeights()
    {  
        dt = Time.fixedDeltaTime;

        WaterCS.SetFloat("dt", dt);
        WaterCS.SetFloat("c", c);
        WaterCS.SetFloat("xSize", xSize);
        WaterCS.SetFloat("zSize", zSize);
        WaterCS.SetFloat("a", a);
        WaterCS.SetInt("handPositionX", nn.x_cord);
        WaterCS.SetInt("handPositionY", nn.y_cord);
        WaterCS.SetFloat("maxTerrainHeight", terrain.maxTerrainHeight);
        WaterCS.SetFloat("minTerrainHeight", terrain.minTerrainHeight);
        WaterCS.SetBool("rainEnabled", rainEnabled);
        
        if (nn.predictedLabel == "Open Hand")
        {
            if (rainEnabled)
            {
                WaterCS.Dispatch(4, 512/8, 424/8, 1);
            }
        }
        if (Input.GetKey(KeyCode.Space) || addRain)
        {
            WaterCS.SetInt("handPositionX", xSize/2);
            WaterCS.SetInt("handPositionY", zSize/2);
            WaterCS.Dispatch(4, 512/8, 424/8, 1);
        }
        WaterCS.Dispatch(0, 512/8, 424/8, 1);
        WaterCS.Dispatch(1, 512/8, 424/8, 1);
        WaterCS.Dispatch(6, 512/8, 424/8, 1);
        WaterCS.Dispatch(3, 512/8, 424/8, 1);
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        normals = mesh.normals;
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
        depthMapTemp = new ComputeBuffer(vertices.Length, sizeof(float));
        waterNormals = new ComputeBuffer(vertices.Length, sizeof(float) * 3);

        WaterCS.SetBuffer(0, "vertices", verticesBuffer);
        WaterCS.SetBuffer(1, "terrainVertices", terrain.verticesBuffer);
        WaterCS.SetBuffer(2, "terrainVertices", terrain.verticesBuffer);
        WaterCS.SetBuffer(1, "vertices", verticesBuffer);
        WaterCS.SetBuffer(0, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(1, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(6, "fluxMap", fluxMapBuffer);
        WaterCS.SetBuffer(0, "heightmap", terrain.heightBuffer);
        WaterCS.SetBuffer(1, "heightmap", terrain.heightBuffer);
        WaterCS.SetBuffer(6, "heightmap", terrain.heightBuffer);
        WaterCS.SetBuffer(0, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(1, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(3, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(5, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(6, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(1, "depthMapTemp", depthMapTemp);
        WaterCS.SetBuffer(6, "depthMapTemp", depthMapTemp);
        WaterCS.SetTexture(5, "colorsTerrain", terrain.colors);
        WaterCS.SetBuffer(5, "waterNormals", waterNormals);
        WaterCS.SetTexture(3, "colors", colors);

        WaterCS.SetBuffer(2, "vertices", verticesBuffer);
        WaterCS.SetBuffer(2, "heightmap", terrain.heightBuffer);
        WaterCS.SetBuffer(4, "heightmapRaw", heightmapRawBuffer);
        WaterCS.SetBuffer(4, "depthMap", depthMapBuffer);
        WaterCS.SetBuffer(2, "depthMap", depthMapBuffer);

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
        depthMapTemp.Release();
        waterNormals.Release();
    }
}
