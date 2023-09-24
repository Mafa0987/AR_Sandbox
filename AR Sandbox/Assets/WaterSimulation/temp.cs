using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterSimulator_temp : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    public MeshGeneratorTexture terrain;
    public ComputeShader WaterCS;
    ComputeBuffer verticesBuffer;
    float[] heightmap;
    int[] triangles;
    float[] waveHeights;
    float[] acceleration;
    float[] velocity;
    float[] heightSums;
    public int xSize = 100;
    public int zSize = 100;

    float[] depth;

    public float wavespeed = 2f;
    public float spacing = 1f;

    public float posDamping = 1f;

    public float velDamping = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        InitMesh();
        heightmap = terrain.heightmap;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float amplitude = 10;  // Amplitude of Gaussian bump
            float sigma = 10;  // Standard deviation of Gaussian bump
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
        UpdateHeights();
        UpdateMesh();
    }

    void UpdateHeights()
    {
        float dt = Time.deltaTime;
        float c = wavespeed * wavespeed / spacing / spacing;
        float pd = Mathf.Min(posDamping * Time.deltaTime, 1.0f);
        float vd = Mathf.Max(0f, 1f - velDamping * Time.deltaTime);

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float current = vertices[i].y;
                float heightSum = 0;
                
                heightSum += z < zSize ? vertices[i + xSize + 1].y : current;
                heightSum += z > 0 ? vertices[i - xSize - 1].y : current;
                heightSum += x > 0 ? vertices[i - 1].y : current;
                heightSum += x < xSize ? vertices[i + 1].y : current;

                heightSums[i] = heightSum;
                
                acceleration[i] = c * (heightSum - 4 * current);
                velocity[i] += acceleration[i] * Time.deltaTime;
                i++;
            }
        }
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i].y += (0.25f * heightSums[i] - vertices[i].y) * pd;
                velocity[i] *= vd;
                vertices[i].y += velocity[i] * Time.deltaTime;
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void InitMesh()
    {
        waveHeights = new float[(xSize + 1) * (zSize + 1)];
        acceleration = new float[(xSize + 1) * (zSize + 1)];
        velocity = new float[(xSize + 1) * (zSize + 1)];
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        WaterCS.SetBuffer(0, "vertices", verticesBuffer);
        WaterCS.SetBuffer(1, "vertices", verticesBuffer);
        depth = new float[(xSize + 1) * (zSize + 1)];
        heightSums = new float[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                waveHeights[i] = 1;
                acceleration[i] = 0;
                velocity[i] = 0;
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
}
