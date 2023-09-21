using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorCPU : MonoBehaviour
{
    Mesh mesh;

    public ComputeShader computeShader;

    Vector3[] vertices;

    Vector3[] vertices_shader;

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
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }

    // Update is called once per frame
    void Update()
    {
        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        maxTerrainHeight = 0;
        minTerrainHeight = 0;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                // float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                float y =
                    amplitude1 * Mathf.PerlinNoise(x * frequency1,z * frequency1)
                    + amplitude2 * Mathf.PerlinNoise(x * frequency2, z * frequency2)
                    + amplitude3 * Mathf.PerlinNoise(x * frequency3, z * frequency3)
                        * noiseStrength;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;

                i++;
            }
        }

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

        colors = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = (vertices[i].y-minTerrainHeight)/maxTerrainHeight;
                colors[i] = gradient.Evaluate(height);
                for (float line = 0, j = 0; j < 5; j++)
                {
                    if (height > line && height < line + 0.015f)
                    {
                        //colors[i] = Color.black;
                        break;
                    }
                    line += 0.2f;
                }
                i++;
            }
        }

    }

    void UpdateMesh()
    {
        mesh.Clear();
        for (int i = 0; i < 10; i++)
        {
            drawContour(i/10f);
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    void DrawLine(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;
        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
            return;
        int y = 0;
        int x = 0;
        for (int i = 0; i <= steps; i++)
        {
            colors[x1 + x + (y1 + y) * 501] = Color.black;
            x += dx/steps;
            y += dy/steps;
        }
    }

    int GetState(int a, int b, int c, int d)
    {
        return a * 8 + b * 4 + c * 2 + d * 1;
    }

    int checkHeight(float height, float limit)
    {
        if(height > limit){
            return 1;
        }
        else{
            return 0;
        }
    }

    void drawContour(float limit)
    {
        int state;
        for(int y = 0; y < 125; y++){
            for(int x = 0; x < 125; x++){
                int[] a = {x*4+2, y*4};
                int[] b = {x*4+4, y*4+2};
                int[] c = {x*4+2, y*4+4};
                int[] d = {x*4, y*4+2};
                state = GetState(checkHeight((vertices[x*4+y*4*501].y-minTerrainHeight)/maxTerrainHeight, limit), 
                checkHeight((vertices[x*4+4+y*4*501].y-minTerrainHeight)/maxTerrainHeight, limit), 
                checkHeight((vertices[x*4+4+(y*4+4)*501].y-minTerrainHeight)/maxTerrainHeight, limit), 
                checkHeight((vertices[x*4+(y*4+4)*501].y-minTerrainHeight)/maxTerrainHeight, limit));
                switch(state)
                {   case 1:
                        DrawLine(c[0], c[1], d[0], d[1]);
                        break;
                    case 2:
                        DrawLine(b[0], b[1], c[0], c[1]);
                        break;
                    case 3:
                        DrawLine(b[0], b[1], d[0], d[1]);
                        break;
                    case 4:
                        DrawLine(a[0], a[1], b[0], b[1]);
                        break;
                    case 5:
                        DrawLine(a[0], a[1], d[0], d[1]);
                        DrawLine(b[0], b[1], c[0], c[1]);
                        break;
                    case 6:
                        DrawLine(a[0], a[1], c[0], c[1]);
                        break;
                    case 7:
                        DrawLine(a[0], a[1], d[0], d[1]);
                        break;
                    case 8:
                        DrawLine(a[0], a[1], d[0], d[1]);
                        break;
                    case 9:
                        DrawLine(a[0], a[1], c[0], c[1]);
                        break;
                    case 10:
                        DrawLine(a[0], a[1], b[0], b[1]);
                        DrawLine(c[0], c[1], d[0], d[1]);
                        break;
                    case 11:
                        DrawLine(a[0], a[1], b[0], b[1]);
                        break;
                    case 12:
                        DrawLine(b[0], b[1], d[0], d[1]);
                        break;
                    case 13:
                        DrawLine(b[0], b[1], c[0], c[1]);
                        break;
                    case 14:
                        DrawLine(d[0], d[1], c[0], c[1]);
                        break;
                }
            }
        }
    }
}
