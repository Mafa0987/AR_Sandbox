using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorGPU : MonoBehaviour
{
    Mesh mesh;

    public ComputeShader computeShader;

    Vector3[] vertices;

    float[] heightmap;
    int[] triangles;

    Color[] colors;

    public int xSize = 500;
    public int zSize = 500;

    public Gradient gradient;

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
        heightmap = new float[(xSize + 1) * (zSize + 1)];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
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
        heightmap = new float[501*501];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
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

        CreateShapeGPU();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        CreateShapeGPU();
        UpdateMesh();
    }

    void CreateShapeGPU()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[vertices.Length];

        ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        ComputeBuffer heightBuffer = new ComputeBuffer(heightmap.Length, sizeof(float));
        ComputeBuffer colorsBuffer = new ComputeBuffer(colors.Length, sizeof(float) * 4);
        heightBuffer.SetData(heightmap);
        verticesBuffer.SetData(vertices);
        colorsBuffer.SetData(colors);

        computeShader.SetBuffer(0, "heightmap", heightBuffer);
        computeShader.SetBuffer(0, "vertices", verticesBuffer);
        computeShader.SetBuffer(0, "colors", colorsBuffer);
        computeShader.SetBuffer(1, "heightmap", heightBuffer);
        computeShader.SetBuffer(1, "vertices", verticesBuffer);
        computeShader.SetBuffer(1, "colors", colorsBuffer);
        computeShader.SetFloat("maxTerrainHeight", maxTerrainHeight);
        computeShader.SetFloat("minTerrainHeight", minTerrainHeight);

        computeShader.Dispatch(0, 512/8, 512/8, 1);
        verticesBuffer.GetData(vertices);
        computeShader.Dispatch(1, 126/9, 126/9, 1);
        colorsBuffer.GetData(colors);

        verticesBuffer.Dispose();
        heightBuffer.Dispose();
        colorsBuffer.Dispose();
    }
    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.colors = colors;
    }
}
