using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGen : MonoBehaviour
{
    public Material material;
    public ComputeShader computeShader;
    public RenderTexture colors;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    ComputeBuffer verticesBuffer;
    ComputeBuffer heightBuffer;


    public float[] heightmap;
    public int numVerticesX = 501;
    public int numVerticesZ = 501;

    public float amplitude1 = 1f;
    public float amplitude2 = 1f;
    public float amplitude3 = 1f;
    public float frequency1 = 1f;
    public float frequency2 = 1f;
    public float frequency3 = 1f;
    public float noiseStrength = 1f;

    float minTerrainHeight;
    float maxTerrainHeight;

    // Start is called before the first frame update
    void Start()
    {
        CreateTriangles();
        CreateHeightmap();
        CreateUV();
        InitShader();
        CreateShapeGPU();
        InitMesh();
    }

    // Update is called once per frame
    void Update()
    {
        CreateShapeGPU();
        UpdateMesh();
    }

    void CreateShapeGPU()
    {
        heightBuffer.SetData(heightmap);
        computeShader.SetFloat("maxTerrainHeight", maxTerrainHeight);
        computeShader.SetFloat("minTerrainHeight", minTerrainHeight);

        computeShader.Dispatch(0, 512/8, 512/8, 1);
        verticesBuffer.GetData(vertices);
        computeShader.Dispatch(1, 126/9, 126/9, 1);
    }
    void UpdateMesh()
    {
        mesh.vertices = vertices;
        material.mainTexture = colors;
    }

    void InitShader()
    {
        vertices = new Vector3[numVerticesX * numVerticesZ];
        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        heightBuffer = new ComputeBuffer(heightmap.Length, sizeof(float));
        colors = new RenderTexture(numVerticesX, numVerticesZ, 24);
        colors.enableRandomWrite = true;
        colors.Create();

        computeShader.SetBuffer(0, "heightmap", heightBuffer);
        computeShader.SetBuffer(0, "vertices", verticesBuffer);
        computeShader.SetTexture(0, "colors", colors);
        computeShader.SetBuffer(1, "heightmap", heightBuffer);
        computeShader.SetBuffer(1, "vertices", verticesBuffer);
        computeShader.SetTexture(1, "colors", colors);
        computeShader.SetInt("numVerticesX", numVerticesX);
        computeShader.SetInt("numVerticesZ", numVerticesZ);
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

    void CreateHeightmap()
    {
        heightmap = new float[numVerticesX * numVerticesZ];
        for (int i = 0, z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                float y =
                    amplitude1 * Mathf.PerlinNoise(x * frequency1,z * frequency1)
                    + amplitude2 * Mathf.PerlinNoise(x * frequency2, z * frequency2)
                    + amplitude3 * Mathf.PerlinNoise(x * frequency3, z * frequency3)
                        * noiseStrength;
                heightmap[i] = y;
                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;
                i++;
            }
        }
    }

    void CreateUV()
    {
        uvs = new Vector2[numVerticesX * numVerticesZ];
        for (int i = 0, z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                uvs[i] = new Vector2((float)x / (numVerticesX-1), (float)z / (numVerticesZ-1));
                i++;
            }
        }
    }
    
    void InitMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        material.mainTexture = colors;
        mesh.RecalculateNormals();
    }

    void OnDestroy()
    {
        verticesBuffer.Release();
        heightBuffer.Release();
    }

}