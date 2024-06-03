using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using System;

public class NeuralNetwork : MonoBehaviour
{
    string[] labels = new string[] {"No Hand", "Open Hand", "Closed Hand"};
    public ModelAsset modelAsset;
    Model runtimeModel;
    IWorker worker;
    TensorFloat inputTensor;
    public string predictedLabel;
    public float probability;
    public int x_cord;
    public int y_cord;

    public RenderTexture outputTexture;
    public TerrainGen terrain;
    public RawImage rawImage;
    public ComputeShader computeShader;
    ComputeBuffer depthColor;
    ComputeBuffer modelInput;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xSize = originalWidth - (calibration.xCut.x + calibration.xCut.y);
        zSize = originalHeight - (calibration.zCut.x + calibration.zCut.y);
        xCutL = calibration.xCut.x;
        xCutB = calibration.zCut.x;
        depthColor = new ComputeBuffer(xSize*zSize*3, sizeof(float));
        modelInput = new ComputeBuffer(modelRes*modelRes*3, sizeof(float));

        outputTexture = new RenderTexture(modelRes, modelRes, 24, RenderTextureFormat.ARGB32);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        
        shape = new TensorShape(1, modelRes, modelRes, 3);
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);

        computeShader.SetTexture(2, "outputTexture", outputTexture);
        computeShader.SetBuffer(2, "depthColor", depthColor);
        computeShader.SetBuffer(0, "depthData", terrain.depthDataInt);

        computeShader.SetBuffer(0, "depthColor", depthColor);
        computeShader.SetBuffer(1, "depthColor", depthColor);
        computeShader.SetBuffer(1, "modelInput", modelInput);
        computeShader.SetInt("inputDimX", xSize);
        computeShader.SetInt("inputDimY", zSize);
        computeShader.SetInt("xCutL", xCutL);
        computeShader.SetInt("zCutB", xCutB);
        computeShader.SetFloat("ratioX", (float)(xSize-1)/(float)(modelRes-1));
        computeShader.SetFloat("ratioY", (float)(zSize-1)/(float)(modelRes-1));
        computeShader.SetInt("modelRes", modelRes);

        inputTensor = TensorFloat.Zeros(shape);
    }

    // Update is called once per frame
    void Update()
    {
        SingleModel();
    }

    void SingleModel()
    {
        var gpuTensor = ComputeTensorData.Pin(inputTensor);
        computeShader.SetBuffer(1, "modelInput", gpuTensor.buffer);

        computeShader.Dispatch(0, originalWidth, originalHeight, 1);
        computeShader.Dispatch(1, modelRes/8, modelRes/8, 1);

        worker.Execute(inputTensor);
        TensorFloat gesture_output = worker.PeekOutput("gesture_output") as TensorFloat;
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
        TensorFloat position_output = worker.PeekOutput("position_output") as TensorFloat;
        position_output.MakeReadable();
        x_cord = (int)(position_output[0] * xSize / modelRes);
        y_cord = zSize - (int)(position_output[1] * zSize / modelRes);
        computeShader.SetInt("handX", x_cord);
        computeShader.SetInt("handY", y_cord);

        // computeShader.Dispatch(2, modelRes/8, modelRes/8, 1);
        // testTexture.Apply();
        // rawImage.texture = outputTexture;
    }

    void OnDestroy()
    {
        worker?.Dispose();
        inputTensor?.Dispose();
        depthColor?.Dispose();
        modelInput?.Dispose();
    }
}

