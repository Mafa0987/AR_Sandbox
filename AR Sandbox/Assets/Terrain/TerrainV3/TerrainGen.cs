using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenV3 : MonoBehaviour
{
    public Material material;
    public ComputeShader computeShader;
    public MultiSourceManager msm;
    public RenderTexture colors;
    public int xCut = 160;
    public int zCut = 100;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    ComputeBuffer verticesBuffer;
    ComputeBuffer heightBuffer;


    //public float[] heightmap;
    public ushort[] heightmapShort;
    public int[] heightmap;
    public int xSize = 512;
    public int zSize = 424;

    public int kinectheight = 2000;
    public int maxHeight = 1000;
    public int minHeight = 0;



    public float amplitude1 = 1f;
    public float frequency1 = 1f;

    float minTerrainHeight = 1200;
    float maxTerrainHeight = 2000;

    // Start is called before the first frame update
    void Start()
    {
        xSize -= xCut * 2;
        zSize -= zCut * 2;
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
        CreateHeightmap();
        heightBuffer.SetData(heightmap);
        computeShader.SetFloat("maxTerrainHeight", maxTerrainHeight);
        computeShader.SetFloat("minTerrainHeight", minTerrainHeight);

        computeShader.Dispatch(0, 512/8, 424/8, 1);
        verticesBuffer.GetData(vertices);
        computeShader.Dispatch(1, 128/8, 105/7, 1);
    }
    void UpdateMesh()
    {
        mesh.vertices = vertices;
        material.mainTexture = colors;
    }

    void InitShader()
    {
        vertices = new Vector3[xSize * zSize];
        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        heightBuffer = new ComputeBuffer(heightmap.Length, sizeof(int));
        colors = new RenderTexture(xSize*4, zSize*4, 24);
        colors.enableRandomWrite = true;
        colors.Create();

        computeShader.SetBuffer(0, "heightmap", heightBuffer);
        computeShader.SetBuffer(0, "vertices", verticesBuffer);
        computeShader.SetTexture(0, "colors", colors);
        computeShader.SetBuffer(1, "heightmap", heightBuffer);
        computeShader.SetBuffer(1, "vertices", verticesBuffer);
        computeShader.SetTexture(1, "colors", colors);
        computeShader.SetInt("xSize", xSize);
        computeShader.SetInt("zSize", zSize);
        computeShader.SetInt("xCut", xCut);
        computeShader.SetInt("zCut", zCut);
    }

    void CreateTriangles()
    {
        triangles = new int[(xSize-1) * (zSize-1) * 6];
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

    void CreateHeightmap()
    {
        heightmapShort = msm.GetDepthData();
        heightmap = new int[(512-xCut*2) * (424-zCut*2)];

        int j = 0;
        for(int i = 0; i < heightmapShort.Length; i++)
        {
            int x = i % 512;
            int y = i / 424;

            if (x < xCut | x > 512 - xCut | y < zCut | y > 424 - zCut)
            {
                int u = 0;
            }
            else
            {
                heightmap[j] = heightmapShort[i];
                j++;
            }
        }
        int test = 0;

        // heightmap = new float[xSize * zSize];
        // for (int i = 0, z = 0; z < zSize; z++)
        // {
        //     for (int x = 0; x < xSize; x++)
        //     {
        //         float y = amplitude1 * Mathf.PerlinNoise(x * frequency1,z * frequency1) + 1000;
        //         y = (float)Math.Round(y, 0);
        //         heightmap[i] = y;
        //         if (x == 0 & z == 0)
        //         {
        //             minTerrainHeight = y;
        //             maxTerrainHeight = y;
        //         }
        //         if (y > maxTerrainHeight)
        //             maxTerrainHeight = y;
        //         if (y < minTerrainHeight)
        //             minTerrainHeight = y;
        //         i++;
        //     }
        // }
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
