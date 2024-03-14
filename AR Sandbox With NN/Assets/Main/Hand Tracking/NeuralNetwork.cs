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
    public Vector2Int handPosition;
    public string predictedLabel;
    public float probability;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputArray = new float[240*240*3];
        input = new ComputeBuffer(300*400*3, sizeof(float));
        depthDataShort = new ComputeBuffer(512 * 424 / 2, sizeof(uint));
        depthData = new ComputeBuffer(512*424, sizeof(uint));
        output = new ComputeBuffer(240*240*3, sizeof(float));

        tensorTexture = new Texture2D(240, 240);
        outputTexture = new RenderTexture(240, 240, 24, RenderTextureFormat.ARGB32);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        
        allocator = new TensorCachingAllocator();
        ops = WorkerFactory.CreateOps(BackendType.GPUCompute, allocator);
        runtimeModel1 = ModelLoader.Load(modelAsset1);
        runtimeModel2 = ModelLoader.Load(modelAsset2);
        worker1 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel1);
        worker2 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel2);

        computeShader.SetTexture(3, "outputTexture", outputTexture);
        computeShader.SetBuffer(3, "input", input);
        computeShader.SetBuffer(0, "depthDataShort", depthDataShort);
        computeShader.SetBuffer(0, "depthData", depthData);
        computeShader.SetBuffer(1, "depthData", depthData);
        computeShader.SetBuffer(1, "input", input);
        computeShader.SetBuffer(2, "input", input);
        computeShader.SetBuffer(2, "output", output);
        computeShader.SetInt("inputDimX", 300);
        computeShader.SetInt("inputDimY", 400);
        computeShader.SetInt("xCutL", 124);
        computeShader.SetInt("zCutB", 19);
        computeShader.SetFloat("ratioX", 300f/240f);
        computeShader.SetFloat("ratioY", 400f/240f);
    }

    // Update is called once per frame
    void Update()
    {
        kinectDepth = msm.GetDepthData();
        depthDataShort.SetData(kinectDepth);
        //kinectColor = processDepthData(kinectDepth, 124, 88, 19, 5);
        //scaledTexture = Bilinear(kinectColor, 240, 240);
        computeShader.Dispatch(0, 512*424/2/64, 1, 1);
        computeShader.Dispatch(1, (int)Mathf.Ceil(300/8f), (int)Mathf.Ceil(400/8f), 1);
        computeShader.Dispatch(2, 240/8, 240/8, 1);
        output.GetData(outputArray);
        // for (int y = 0; y < 240; y++)
        // {
        //     for (int x = 0; x < 240; x++)
        //     {
        //         bilde[x*3 + y*240*3] = scaledTexture.GetPixel(x, 239-y).r;
        //         bilde[x*3+1 + y*240*3] = scaledTexture.GetPixel(x, 239-y).g;
        //         bilde[x*3+2 + y*240*3] = scaledTexture.GetPixel(x, 239-y).b;
        //     }
        // }
        TensorShape shape = new TensorShape(1, 240, 240, 3);
        inputTensor = new TensorFloat(shape, outputArray);
        inputTensor = ops.Mul(inputTensor, 255.0f);
        worker1.Execute(inputTensor);
        TensorFloat outputTensor1 = worker1.PeekOutput() as TensorFloat;
        var coordinates = outputTensor1;
        coordinates.MakeReadable();
        var x_cord = coordinates[0];
        var y_cord = coordinates[1];
        //handPosition = new Vector2Int((int)(x_cord*300/240), (int)((399-y_cord)*400/240));
        handPosition = new Vector2Int((int)(x_cord*300/240), (int)((240-y_cord)*400/240));
        Debug.Log($"x: {x_cord}, y: {y_cord}");
        inputTensor.MakeReadable();
        worker2.Execute(inputTensor);
        TensorFloat outputTensor2 = worker2.PeekOutput() as TensorFloat;
        var probabilities = outputTensor2;
        var indexOfMaxProba = ops.ArgMax(probabilities, -1, false);
        probabilities.MakeReadable();
        indexOfMaxProba.MakeReadable();
        var predictedNumber = indexOfMaxProba[0];
        probability = probabilities[predictedNumber];
        predictedLabel = labels[predictedNumber];

        Debug.Log($"Predicted label: {labels[predictedNumber]} with probability: {probability}");
        inputTensor.MakeReadable();
        computeShader.SetInt("handX", (int)x_cord);
        computeShader.SetInt("handY", (int)y_cord);
        computeShader.Dispatch(3, 240/8, 240/8, 1);
        // for (int y = 0; y < 240; y++)
        // {
        //     for (int x = 0; x < 240; x++)
        //     {
        //         tensorTexture.SetPixel(x, 239-y, new Color(inputTensor[0, y, x, 0]/255f, inputTensor[0, y, x, 1]/255f, inputTensor[0, y, x, 2]/255f));
        //     }
        // }
        // tensorTexture.SetPixel((int)x_cord, 240-(int)y_cord, new Color(0, 1, 0));
        // tensorTexture.SetPixel((int)x_cord+1, 240-(int)y_cord, new Color(0, 1, 0));
        // tensorTexture.SetPixel((int)x_cord-1, 240-(int)y_cord, new Color(0, 1, 0));
        // tensorTexture.SetPixel((int)x_cord, 240-(int)y_cord+1, new Color(0, 1, 0));
        // tensorTexture.SetPixel((int)x_cord, 240-(int)y_cord-1, new Color(0, 1, 0));
        // tensorTexture.Apply();
        rawImage.texture = outputTexture;
        outputTensor1?.Dispose();
        outputTensor2?.Dispose();
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
        int width = 512-xCut1-xCut2;
        int height = 424-zCut1-zCut2;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float depth = Mathf.Clamp(depthData[x + xCut1 + (y + zCut1)*512] / 4500f, 0, 1);
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

}

