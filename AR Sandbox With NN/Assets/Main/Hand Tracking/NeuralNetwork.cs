using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using System;

public class NeuralNetwork : MonoBehaviour
{
    string[] labels = new string[] {"No Hand", "Open Hand", "Closed Hand"};
    ITensorAllocator allocator;
    Ops ops;
    public ModelAsset modelAsset1;
    public ModelAsset modelAsset2;
    Model runtimeModel1;
    Model runtimeModel2;
    IWorker worker1;
    IWorker worker2;
    TextureTransform transformLayout;
    TensorFloat inputTensor;
    public string predictedLabel;
    public float probability;
    public int x_cord;
    public int y_cord;

    public MultiSourceManager msm;
    public Texture2D inputTexture;
    public RenderTexture outputTexture;
    public TerrainGen terrain;
    Texture2D tensorTexture;
    Texture2D scaledTexture;
    public RawImage rawImage;
    ushort[] kinectDepth;
    Texture2D kinectColor;
    int number = 0;
    bool run = false;
    public ComputeShader computeShader;
    ComputeBuffer depthDataShort;
    ComputeBuffer depthData;
    ComputeBuffer input;
    ComputeBuffer output;
    float[] outputArray;

    TensorShape shape;

    //parameters
    public Calibration calibration;
    int originalWidth = 512;
    int originalHeight = 424;
    int xCutL = 124;
    int xCutB = 19;
    int modelRes = 256;
    int xSize = 300;
    int zSize = 400;

    //ComputeBuffer tensorBuffer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xSize = originalWidth - (calibration.xCut.x + calibration.xCut.y);
        zSize = originalHeight - (calibration.zCut.x + calibration.zCut.y);
        xCutL = calibration.xCut.x;
        xCutB = calibration.zCut.x;
        outputArray = new float[modelRes*modelRes*3];
        input = new ComputeBuffer(xSize*zSize*3, sizeof(float));
        depthDataShort = new ComputeBuffer(originalWidth * originalHeight / 2, sizeof(uint));
        depthData = new ComputeBuffer(originalWidth*originalHeight, sizeof(uint));
        output = new ComputeBuffer(modelRes*modelRes*3, sizeof(float));

        tensorTexture = new Texture2D(modelRes, modelRes);
        outputTexture = new RenderTexture(modelRes, modelRes, 24, RenderTextureFormat.ARGB32);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        
        shape = new TensorShape(1, modelRes, modelRes, 3);
        runtimeModel1 = ModelLoader.Load(modelAsset1);
        runtimeModel2 = ModelLoader.Load(modelAsset2);
        worker1 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel1);
        worker2 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel2);
        // allocator = new TensorCachingAllocator();
        // ops = WorkerFactory.CreateOps(BackendType.GPUCompute, allocator);

        computeShader.SetTexture(3, "outputTexture", outputTexture);
        computeShader.SetBuffer(3, "input", input);
        computeShader.SetBuffer(0, "depthDataShort", depthDataShort);
        computeShader.SetBuffer(0, "depthData", depthData);
        //computeShader.SetBuffer(1, "depthData", depthData);
        computeShader.SetBuffer(1, "depthData", terrain.nyBuffer);

        computeShader.SetBuffer(1, "input", input);
        computeShader.SetBuffer(2, "input", input);
        computeShader.SetBuffer(2, "output", output);
        computeShader.SetInt("inputDimX", xSize);
        computeShader.SetInt("inputDimY", zSize);
        computeShader.SetInt("xCutL", xCutL);
        computeShader.SetInt("zCutB", xCutB);
        computeShader.SetFloat("ratioX", (float)xSize/(float)modelRes);
        computeShader.SetFloat("ratioY", (float)zSize/(float)modelRes);
        computeShader.SetInt("modelRes", modelRes);

        inputTensor = TensorFloat.Zeros(shape);
        //tensorBuffer = ComputeTensorData.Pin(inputTensor).buffer;
        //computeShader.SetBuffer(2, "output", tensorBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        SingleModel();
    }

    void SingleModel()
    {
        //kinectDepth = msm.GetDepthData();
        //depthDataShort.SetData(kinectDepth);
        //computeShader.Dispatch(0, originalWidth*originalHeight/2/64, 1, 1);
        var gpuTensor = ComputeTensorData.Pin(inputTensor);
        computeShader.SetBuffer(2, "output", gpuTensor.buffer);

        computeShader.Dispatch(1, originalWidth, originalHeight, 1);
        computeShader.Dispatch(2, modelRes/8, modelRes/8, 1);
        // output.GetData(outputArray);

        // inputTensor = new TensorFloat(shape, outputArray);

        worker1.Execute(inputTensor);
        // inputTensor.Dispose();
        // tensorBuffer.Dispose();
        TensorFloat gesture_output = worker1.PeekOutput("gesture_output") as TensorFloat;
        gesture_output.MakeReadable();
        int maxIndex = 0;
        float max = 0;
        for (int i = 0; i < gesture_output.shape[1]; i++)
        {
            if (gesture_output[i] > max)
            {
                max = gesture_output[i];
                maxIndex = i;
            }
        }
        predictedLabel = labels[maxIndex];
        probability = max;
        TensorFloat position_output = worker1.PeekOutput("position_output") as TensorFloat;
        position_output.MakeReadable();
        x_cord = (int)(position_output[0] * xSize / modelRes);
        y_cord = zSize - (int)(position_output[1] * zSize / modelRes);
        computeShader.SetInt("handX", x_cord);
        computeShader.SetInt("handY", y_cord);

        // computeShader.SetBuffer(3, "output", testBuffer);
        // computeShader.Dispatch(3, modelRes/8, modelRes/8, 1);

        // testTexture.Apply();

        // rawImage.texture = outputTexture;
    }

    void OnDestroy()
    {
        worker1?.Dispose();
        inputTensor?.Dispose();
        depthDataShort?.Dispose();
        depthData?.Dispose();
        input?.Dispose();
        output?.Dispose();
        inputTensor?.Dispose();
    }
}

