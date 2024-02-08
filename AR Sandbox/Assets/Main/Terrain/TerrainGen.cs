using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random=UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGen : MonoBehaviour
{
    Calibration calibration;
    public Material material;
    public GameObject terrainpos;
    public GameObject waterpos;
    public ComputeShader computeShader;
    public RenderTexture colors;
    public MultiSourceManager msm;
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    ComputeBuffer verticesBuffer;
    ComputeBuffer heightBuffer;

    public Slider minTerrainSlider;
    public Slider maxTerrainSlider;

    float timer = 0;

    //test average noise reduction
    ComputeBuffer averageBuffer;
    int N = 512 * 424;
    int num_arrays = 30;
    int sampleIndex = 0;
    ComputeBuffer sampleSums;
    //test

    public float[] heightmap;
    public float[] heightmapRaw;
    public ushort[] heightmap_short;


    public int xSize = 512;
    public int zSize = 424;
    public int originalWidth = 512;
    public int originalHeight = 424;
    public int xCut = 0;
    public int zCut = 0;

    public float amplitude1 = 1f;
    public float amplitude2 = 1f;
    public float amplitude3 = 1f;
    public float frequency1 = 1f;
    public float frequency2 = 1f;
    public float frequency3 = 1f;
    public float noiseStrength = 1f;

    public float minTerrainHeight = 0f;
    public float maxTerrainHeight = 10f;
    public float rainHeight = 10f;
    public float rainOffset = 0f;

    //test
    ushort[] old;
    ComputeBuffer oldBuffer;
    uint[] ny;
    ComputeBuffer nyBuffer;
    ComputeBuffer heightmapRawBuffer;
    //test

    // Start is called before the first frame update
    void Start()
    {
        calibration = GameObject.Find("Calibration").GetComponent<Calibration>();
        xSize = originalWidth - (calibration.xCut.x + calibration.xCut.y);
        zSize = originalHeight - (calibration.zCut.x + calibration.zCut.y);
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
        if(timer >= 1/30){
            CreateShapeGPU();
            timer = 0;
        }
        else{
            timer += Time.deltaTime;
        }
        UpdateMesh();
    }

    void CreateShapeGPU()
    {
        //CreateHeightmap();
        heightmap_short = msm.GetDepthData();
        oldBuffer.SetData(heightmap_short);
        //Converts Heightmap to integer
        computeShader.Dispatch(2, 512*424/2/64, 1, 1);

        //new sample test
        if (sampleIndex == num_arrays - 1)
            sampleIndex = 0;
        else
            sampleIndex++;
        computeShader.SetInt("sampleIndex", sampleIndex);


        //Noise reduction
        //Cuts heightmap and converts to float
        computeShader.Dispatch(3, 512/8, 424/8, 1);
        heightBuffer.GetData(heightmap);
        heightmapRawBuffer.GetData(heightmapRaw);
        if (Input.GetKeyDown("space"))
        {
            SaveAsJpg();
            Debug.Log("space was pressed");
        }
        //Rest of the calculations
        // minTerrainHeight = minTerrainSlider.value;
        // maxTerrainHeight = maxTerrainSlider.value;
        // rainHeight = maxTerrainHeight + rainOffset;
        minTerrainHeight = calibration.minTerrainHeight;
        maxTerrainHeight = calibration.maxTerrainHeight;
        computeShader.SetFloat("maxTerrainHeight", maxTerrainHeight);
        computeShader.SetFloat("minTerrainHeight", minTerrainHeight);

        computeShader.Dispatch(0, 512/8, 424/8, 1);
        //verticesBuffer.GetData(vertices);
        computeShader.Dispatch(1, 128/8, 105/7, 1);
        verticesBuffer.GetData(vertices);
    }
    void UpdateMesh()
    {
        // Calibrate();
        mesh.vertices = vertices;
        material.mainTexture = colors;
        //mesh.RecalculateNormals();
    }

    void SaveAsJpg()
    {
        Texture2D tex = new Texture2D(xSize, zSize, TextureFormat.RGB24, false);
        int x = 0;
        int y = 0;
        for (int i = 0; i < heightmapRaw.Length; i++)
        {
            float height = Mathf.Clamp(heightmapRaw[i] / 4500, 0, 1);
            tex.SetPixel(x, y, GetColor(height));
            x++;
            if (x >= xSize)
            {
                x = 0;
                y++;
            }
        }
        //tex.Apply();
        byte[] bytes = tex.EncodeToJPG();
        System.IO.File.WriteAllBytes("Assets/Main/Terrain/terrain.jpg", bytes);
    }

    Color GetColor(float height)
    {
        if (height == 0)
            return new Color(0, 0, 0);
        Color[] colors = {new Color(1, 0, 0), new Color(1, 0.5f, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 0, 1)};
        for (int i = 0; i < 4; i++)
        {
            float lowerBound = i / 4f;
            float upperBound = lowerBound + 1f / 4f;
            float step = upperBound - lowerBound;
            if (height <= upperBound)
                return Color.Lerp(colors[i], colors[i+1], (height-lowerBound)/step);
        }
        if (height > 1)
            Debug.Log("error");
        return new Color(0, 0, 1);
    }

    void InitShader()
    {
        vertices = new Vector3[xSize * zSize];
        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        heightBuffer = new ComputeBuffer(heightmap.Length, sizeof(float));
        heightmapRawBuffer = new ComputeBuffer(heightmapRaw.Length, sizeof(float));
        averageBuffer = new ComputeBuffer(N*num_arrays, sizeof(uint));
        sampleSums = new ComputeBuffer(originalWidth * originalHeight, sizeof(uint));
        colors = new RenderTexture(xSize*4, zSize*4, 24);
        colors.enableRandomWrite = true;
        colors.Create();

        computeShader.SetBuffer(0, "heightmap", heightBuffer);
        computeShader.SetBuffer(0, "vertices", verticesBuffer);
        computeShader.SetTexture(0, "colors", colors);
        computeShader.SetBuffer(1, "heightmap", heightBuffer);
        computeShader.SetBuffer(3, "heightmap", heightBuffer);
        computeShader.SetBuffer(1, "vertices", verticesBuffer);
        //noise reduction
        computeShader.SetBuffer(2, "average", averageBuffer);
        //---------------
        computeShader.SetBuffer(3, "average", averageBuffer);
        computeShader.SetTexture(1, "colors", colors);
        computeShader.SetInt("xSize", xSize);
        computeShader.SetInt("zSize", zSize);
        computeShader.SetInt("originalWidth", originalWidth);
        computeShader.SetInt("originalHeight", originalHeight);
        computeShader.SetInt("xCut", calibration.xCut.x);
        computeShader.SetInt("zCut", calibration.zCut.x);
        //noise reduction
        computeShader.SetInt("N", N);
        computeShader.SetInt("num_arrays", num_arrays);
        computeShader.SetInt("sampleIndex", sampleIndex);
        //---------------

        //test
        oldBuffer = new ComputeBuffer(512 * 424 / 2, sizeof(uint));
        nyBuffer = new ComputeBuffer(512 * 424, sizeof(float));
        oldBuffer.SetData(heightmap_short);
        computeShader.SetBuffer(2, "old", oldBuffer);
        computeShader.SetBuffer(2, "ny", nyBuffer);
        computeShader.SetBuffer(3, "ny", nyBuffer);
        computeShader.SetBuffer(3, "heightmapRaw", heightmapRawBuffer);
        computeShader.SetBuffer(2, "sampleSums", sampleSums);
        computeShader.SetBuffer(3, "sampleSums", sampleSums);
        //test
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
        amplitude2 = Random.Range(0f, 1f);
        heightmap_short = new ushort[originalWidth * originalHeight];
        heightmapRaw = new float[xSize * zSize];
        heightmap = new float[xSize * zSize];
        // for (int i = 0, z = 0; z < originalHeight; z++)
        // {
        //     for (int x = 0; x < originalWidth; x++)
        //     {
        //         ushort y =
        //             (ushort)((amplitude1 * Mathf.PerlinNoise(x * frequency1,z * frequency1)
        //             + amplitude2 * Mathf.PerlinNoise(x * frequency2, z * frequency2)
        //             + amplitude3 * Mathf.PerlinNoise(x * frequency3, z * frequency3)
        //                 * noiseStrength)*300*4/25/2);
        //         heightmap_short[i] = y;
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
        oldBuffer.Release();
        nyBuffer.Release();
        heightmapRawBuffer.Release();
        averageBuffer.Release();
        sampleSums.Release();
    }

    // void Calibrate()
    // {
    //     float dt = Time.deltaTime;
    //     float speed = 20f * dt;
    //     float scaleSpeed = 0.1f * dt;
    //     float rotateSpeed = 5f * dt;

    //     if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.UpArrow))
    //     {
    //         terrainpos.transform.localScale += new Vector3(0, 0, scaleSpeed);
    //         waterpos.transform.localScale += new Vector3(0, 0, scaleSpeed);
    //     }
    //     else if (Input.GetKey(KeyCode.UpArrow))
    //     {
    //         terrainpos.transform.position += new Vector3(0, 0, speed);
    //         waterpos.transform.position += new Vector3(0, 0, speed);
    //     }

    //     if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.DownArrow))
    //     {
    //         terrainpos.transform.localScale += new Vector3(0, 0, -scaleSpeed);
    //         waterpos.transform.localScale += new Vector3(0, 0, -scaleSpeed);
    //     }
    //     else if (Input.GetKey(KeyCode.DownArrow))
    //     {
    //         terrainpos.transform.position += new Vector3(0, 0, -speed);
    //         waterpos.transform.position += new Vector3(0, 0, -speed);
    //     }

    //     if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightArrow))
    //     {
    //         terrainpos.transform.localScale += new Vector3(scaleSpeed, 0, 0);
    //         waterpos.transform.localScale += new Vector3(scaleSpeed, 0, 0);
    //     }
    //     else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightArrow))
    //     {
    //         terrainpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
    //         waterpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
    //     }
    //     else if (Input.GetKey(KeyCode.RightArrow))
    //     {
    //         terrainpos.transform.position += new Vector3(speed, 0, 0);
    //         waterpos.transform.position += new Vector3(speed, 0, 0);
    //     }

    //     if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftArrow))
    //     {
    //         terrainpos.transform.localScale += new Vector3(-scaleSpeed, 0, 0);
    //         waterpos.transform.localScale += new Vector3(-scaleSpeed, 0, 0);
    //     }
    //     else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftArrow))
    //     {
    //         terrainpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
    //         waterpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
    //     }
    //     else if (Input.GetKey(KeyCode.LeftArrow))
    //     {
    //         terrainpos.transform.position += new Vector3(-speed, 0, 0);
    //         waterpos.transform.position += new Vector3(-speed, 0, 0);
    //     }

    //     if (Input.GetKey(KeyCode.O))
    //     {
    //         maxTerrainHeight += 100f * dt;
    //         rainHeight = maxTerrainHeight + rainOffset;
    //     }
    //     else if (Input.GetKey(KeyCode.L))
    //     {
    //         maxTerrainHeight -= 100f * dt;
    //         rainHeight = maxTerrainHeight + rainOffset;
    //     }
    //     if (Input.GetKey(KeyCode.I))
    //     {
    //         minTerrainHeight += 100f * dt;
    //     }
    //     else if (Input.GetKey(KeyCode.K))
    //     {
    //         minTerrainHeight -= 100f * dt;
    //     }
    //     if (Input.GetKey(KeyCode.U))
    //     {
    //         rainOffset += 100f * dt;
    //         rainHeight = maxTerrainHeight + rainOffset;
    //     }
    //     else if (Input.GetKey(KeyCode.J))
    //     {
    //         rainOffset -= 100f * dt;
    //         rainHeight = maxTerrainHeight + rainOffset;
    //     }
    // }
}
