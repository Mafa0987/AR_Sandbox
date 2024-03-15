using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using System;

public class NeuralNetwork : MonoBehaviour
{
    string[] labels = new string[] {"Open Hand", "Closed Hand"};
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

    public MultiSourceManager msm;
    public Texture2D inputTexture;
    public RenderTexture outputTexture;
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
    int orginalWidth = 512;
    int orginalHeight = 424;
    int xCutL = 124;
    int xCutB = 19;
    int modelRes = 256;
    int xSize = 300;
    int zSize = 400;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputArray = new float[modelRes*modelRes*3];
        input = new ComputeBuffer(xSize*zSize*3, sizeof(float));
        depthDataShort = new ComputeBuffer(orginalWidth * orginalHeight / 2, sizeof(uint));
        depthData = new ComputeBuffer(orginalWidth*orginalHeight, sizeof(uint));
        output = new ComputeBuffer(modelRes*modelRes*3, sizeof(float));

        tensorTexture = new Texture2D(modelRes, modelRes);
        outputTexture = new RenderTexture(xSize, zSize, 24, RenderTextureFormat.ARGB32);
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
        computeShader.SetBuffer(1, "depthData", depthData);
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
    }

    // Update is called once per frame
    void Update()
    {
        SingleModel();
    }

    void SingleModel()
    {
        kinectDepth = msm.GetDepthData();
        depthDataShort.SetData(kinectDepth);
        computeShader.Dispatch(0, orginalWidth*orginalHeight/2/64, 1, 1);
        computeShader.Dispatch(1, orginalWidth, orginalHeight, 1);
        computeShader.Dispatch(2, modelRes/8, modelRes/8, 1);
        output.GetData(outputArray);

        inputTensor = new TensorFloat(shape, outputArray);
        worker1.Execute(inputTensor);
        inputTensor.Dispose();
        TensorFloat gesture_output = worker1.PeekOutput("gesture_output") as TensorFloat;
        gesture_output.MakeReadable();
        if (gesture_output[0] > gesture_output[1])
            Debug.Log($"Open hand with probability: {gesture_output[0]}");
        else
            Debug.Log($"Closed Hand with probability: {gesture_output[1]}");

        TensorFloat position_output = worker1.PeekOutput("position_output") as TensorFloat;
        position_output.MakeReadable();
        int x_cord = (int)position_output[0];
        int y_cord = (int)position_output[1];
        computeShader.SetInt("handX", x_cord);
        computeShader.SetInt("handY", y_cord);
        computeShader.Dispatch(3, xSize/8, zSize/8, 1);
        rawImage.texture = outputTexture;
        
    }

    void TwoModels()
    {
        kinectDepth = msm.GetDepthData();
        depthDataShort.SetData(kinectDepth);
        computeShader.Dispatch(0, orginalWidth*orginalHeight/2/64, 1, 1);
        computeShader.Dispatch(1, (int)Mathf.Ceil(xSize/8f), (int)Mathf.Ceil(zSize/8f), 1);
        computeShader.Dispatch(2, modelRes/8, modelRes/8, 1);
        output.GetData(outputArray);
        TensorShape shape = new TensorShape(1, modelRes, modelRes, 3);
        inputTensor = new TensorFloat(shape, outputArray);
        inputTensor = ops.Mul(inputTensor, 255.0f);
        worker1.Execute(inputTensor);
        TensorFloat outputTensor1 = worker1.PeekOutput() as TensorFloat;
        var coordinates = outputTensor1;
        coordinates.MakeReadable();
        var x_cord = coordinates[0];
        var y_cord = coordinates[1];
        Debug.Log($"x: {x_cord}, y: {y_cord}");
        inputTensor.MakeReadable();
        worker2.Execute(inputTensor);
        TensorFloat outputTensor2 = worker2.PeekOutput() as TensorFloat;
        var probabilities = outputTensor2;
        var indexOfMaxProba = ops.ArgMax(probabilities, -1, false);
        probabilities.MakeReadable();
        indexOfMaxProba.MakeReadable();
        var predictedNumber = indexOfMaxProba[0];
        var probability = probabilities[predictedNumber];
        Debug.Log($"Predicted label: {labels[predictedNumber]} with probability: {probability}");
        inputTensor.MakeReadable();
        computeShader.SetInt("handX", (int)x_cord);
        computeShader.SetInt("handY", (int)y_cord);
        computeShader.Dispatch(3, modelRes/8, modelRes/8, 1);
        rawImage.texture = outputTexture;
        outputTensor1?.Dispose();
        outputTensor2?.Dispose();
    }

    void TwoModelsCPU()
    {
        kinectDepth = msm.GetDepthData();
        kinectColor = processDepthData(kinectDepth, 124, 88, 19, 5);
        scaledTexture = Bilinear(kinectColor, modelRes, modelRes);

        for (int y = 0; y < modelRes; y++)
        {
            for (int x = 0; x < modelRes; x++)
            {
                outputArray[x*3 + y*modelRes*3] = scaledTexture.GetPixel(x, 239-y).r;
                outputArray[x*3+1 + y*modelRes*3] = scaledTexture.GetPixel(x, 239-y).g;
                outputArray[x*3+2 + y*modelRes*3] = scaledTexture.GetPixel(x, 239-y).b;
            }
        }
        TensorShape shape = new TensorShape(1, modelRes, modelRes, 3);
        inputTensor = new TensorFloat(shape, outputArray);
        inputTensor = ops.Mul(inputTensor, 255.0f);
        worker1.Execute(inputTensor);
        TensorFloat outputTensor1 = worker1.PeekOutput() as TensorFloat;
        var coordinates = outputTensor1;
        coordinates.MakeReadable();
        var x_cord = coordinates[0];
        var y_cord = coordinates[1];
        Debug.Log($"x: {x_cord}, y: {y_cord}");
        inputTensor.MakeReadable();
        worker2.Execute(inputTensor);
        TensorFloat outputTensor2 = worker2.PeekOutput() as TensorFloat;
        var probabilities = outputTensor2;
        var indexOfMaxProba = ops.ArgMax(probabilities, -1, false);
        probabilities.MakeReadable();
        indexOfMaxProba.MakeReadable();
        var predictedNumber = indexOfMaxProba[0];
        var probability = probabilities[predictedNumber];
        Debug.Log($"Predicted label: {labels[predictedNumber]} with probability: {probability}");
        inputTensor.MakeReadable();
        computeShader.SetInt("handX", (int)x_cord);
        computeShader.SetInt("handY", (int)y_cord);
        computeShader.Dispatch(3, modelRes/8, modelRes/8, 1);
        for (int y = 0; y < modelRes; y++)
        {
            for (int x = 0; x < modelRes; x++)
            {
                tensorTexture.SetPixel(x, 239-y, new Color(inputTensor[0, y, x, 0]/255f, inputTensor[0, y, x, 1]/255f, inputTensor[0, y, x, 2]/255f));
            }
        }
        tensorTexture.SetPixel((int)x_cord, modelRes-(int)y_cord, new Color(0, 1, 0));
        tensorTexture.SetPixel((int)x_cord+1, modelRes-(int)y_cord, new Color(0, 1, 0));
        tensorTexture.SetPixel((int)x_cord-1, modelRes-(int)y_cord, new Color(0, 1, 0));
        tensorTexture.SetPixel((int)x_cord, modelRes-(int)y_cord+1, new Color(0, 1, 0));
        tensorTexture.SetPixel((int)x_cord, modelRes-(int)y_cord-1, new Color(0, 1, 0));
        tensorTexture.Apply();
        rawImage.texture = tensorTexture;
        outputTensor1?.Dispose();
        outputTensor2?.Dispose();
    }

    Texture2D Bilinear(Texture2D origImage, int newWidth, int newHeight)
    {
        Texture2D newImage = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);

        for (int i = 0; i < newHeight; i++)
        {
            for (int j = 0; j < newWidth; j++)
            {
                float x = (float)j / newWidth * origImage.width;
                float y = (float)i / newHeight * origImage.height;
                int x1 = Mathf.FloorToInt(x);
                int y1 = Mathf.FloorToInt(y);
                int x2 = Mathf.Min(x1 + 1, origImage.width - 1);
                int y2 = Mathf.Min(y1 + 1, origImage.height - 1);
                float dx = x - x1;
                float dy = y - y1;

                Color color1 = origImage.GetPixel(x1, y1);
                Color color2 = origImage.GetPixel(x2, y1);
                Color color3 = origImage.GetPixel(x1, y2);
                Color color4 = origImage.GetPixel(x2, y2);

                Color finalColor = (1 - dx) * (1 - dy) * color1 +
                                   dx * (1 - dy) * color2 +
                                   (1 - dx) * dy * color3 +
                                   dx * dy * color4;

                newImage.SetPixel(j, i, finalColor);
            }
        }

        newImage.Apply();
        return newImage;
    }

    Texture2D processDepthData(ushort[] depthData, int xCut1, int xCut2, int zCut1, int zCut2)
    {
        int width = orginalWidth-xCut1-xCut2;
        int height = orginalHeight-zCut1-zCut2;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float depth = Mathf.Clamp(depthData[x + xCut1 + (y + zCut1)*orginalWidth] / 4500f, 0, 1);
                tex.SetPixel(x, y, GetColor(depth));
            }
            
        }
        tex.Apply();
        return tex;
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

    void OnDestroy()
    {
        allocator?.Dispose();
        ops?.Dispose();
        worker1?.Dispose();
        worker2?.Dispose();
        inputTensor?.Dispose();
        depthDataShort?.Dispose();
        depthData?.Dispose();
        input?.Dispose();
        output?.Dispose();
    }

}

