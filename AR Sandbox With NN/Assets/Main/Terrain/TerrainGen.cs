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
    public Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    public ComputeBuffer verticesBuffer;
    public ComputeBuffer heightBuffer;

    public Slider minTerrainSlider;
    public Slider maxTerrainSlider;

    float timer = 0;

    //test average noise reduction
    ComputeBuffer averageBuffer;
    int N = 512 * 424;
    int num_arrays = 60;
    int sampleIndex = 0;
    ComputeBuffer sampleSums;
    ComputeBuffer squaredSums;
    //test

    public float[] heightmap;
    public ushort[] heightmap_short;


    public int xSize = 512;
    public int zSize = 424;
    public int originalWidth = 512;
    public int originalHeight = 424;
    public int xCut = 0;
    public int zCut = 0;

    public float minTerrainHeight = 0f;
    public float maxTerrainHeight = 10f;

    //test
    ComputeBuffer oldBuffer;
    public ComputeBuffer nyBuffer;
    ComputeBuffer heightmapRawBuffer;

    //public Water water;
    public bool update = false;

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
            update = true;
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

        //Rest of the calculations
        minTerrainHeight = calibration.minTerrainHeight;
        maxTerrainHeight = calibration.maxTerrainHeight;
        computeShader.SetFloat("maxTerrainHeight", maxTerrainHeight);
        computeShader.SetFloat("minTerrainHeight", minTerrainHeight);
        computeShader.SetFloat("depthShiftx", calibration.depthShiftx);
        computeShader.SetFloat("indexShiftx", calibration.indexShiftx);
        computeShader.SetFloat("centerX", calibration.centerX);
        computeShader.SetFloat("depthShifty", calibration.depthShifty);
        computeShader.SetFloat("indexShifty", calibration.indexShifty);
        computeShader.SetFloat("centerY", calibration.centerY);
        computeShader.SetFloats("depthShift00", calibration.depthShiftArray[0]);
        computeShader.SetFloats("depthShift10", calibration.depthShiftArray[1]);
        computeShader.SetFloats("depthShift01", calibration.depthShiftArray[2]);
        computeShader.SetFloats("depthShift11", calibration.depthShiftArray[3]);
        computeShader.SetFloats("shift00", calibration.shiftArray[0]);
        computeShader.SetFloats("shift10", calibration.shiftArray[1]);
        computeShader.SetFloats("shift01", calibration.shiftArray[2]);
        computeShader.SetFloats("shift11", calibration.shiftArray[3]);
        computeShader.SetFloat("trueMinHeight", calibration.trueMinHeight);
        

        computeShader.Dispatch(0, 512/8, 424/8, 1);
        computeShader.Dispatch(1, 128/8, 105/7, 1);
        verticesBuffer.GetData(vertices);
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
        heightBuffer = new ComputeBuffer(heightmap.Length, sizeof(float));
        heightmapRawBuffer = new ComputeBuffer(heightmap.Length, sizeof(float));
        averageBuffer = new ComputeBuffer(N*num_arrays, sizeof(uint));
        sampleSums = new ComputeBuffer(originalWidth * originalHeight, sizeof(uint));
        squaredSums = new ComputeBuffer(originalWidth * originalHeight, sizeof(uint));
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

        oldBuffer = new ComputeBuffer(512 * 424 / 2, sizeof(uint));
        nyBuffer = new ComputeBuffer(512 * 424, sizeof(float));
        oldBuffer.SetData(heightmap_short);
        computeShader.SetBuffer(2, "old", oldBuffer);
        computeShader.SetBuffer(2, "ny", nyBuffer);
        computeShader.SetBuffer(3, "ny", nyBuffer);
        computeShader.SetBuffer(3, "heightmapRaw", heightmapRawBuffer);
        computeShader.SetBuffer(2, "sampleSums", sampleSums);
        computeShader.SetBuffer(3, "sampleSums", sampleSums);
        computeShader.SetBuffer(2, "squaredSums", squaredSums);
        computeShader.SetBuffer(3, "squaredSums", squaredSums);
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
        heightmap_short = new ushort[originalWidth * originalHeight];
        heightmap = new float[xSize * zSize];
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
        squaredSums.Release();
    }
}
