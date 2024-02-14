using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using System;

public class NeuralNetwork : MonoBehaviour
{
    public Texture2D inputTexture;
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
    Texture2D BITCH;
    Texture2D scaledTexture;
    public RawImage rawImage;
    float[] bilde;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scaledTexture = Bilinear(inputTexture, 240, 240);
        bilde = new float[240*240*3];
        BITCH = new Texture2D(240, 240);
        for (int y = 0; y < 240; y++)
        {
            for (int x = 0; x < 240; x++)
            {
                bilde[x*3 + y*240*3] = scaledTexture.GetPixel(x, 239-y).r;
                bilde[x*3+1 + y*240*3] = scaledTexture.GetPixel(x, 239-y).g;
                bilde[x*3+2 + y*240*3] = scaledTexture.GetPixel(x, 239-y).b;
            }
        }
        //transformLayout.SetTensorLayout(TensorLayout.NHWC);
        //TensorFloat temp = TextureConverter.ToTensor(inputTexture, channels: 3);
        allocator = new TensorCachingAllocator();
        ops = WorkerFactory.CreateOps(BackendType.GPUCompute, allocator);
        TensorShape shape = new TensorShape(1, 240, 240, 3);
        inputTensor = new TensorFloat(shape, bilde);
        inputTensor = ops.Mul(inputTensor, 255.0f);
        runtimeModel1 = ModelLoader.Load(modelAsset1);
        runtimeModel2 = ModelLoader.Load(modelAsset2);
        worker1 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel1);
        worker2 = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel2);
        inputTensor.MakeReadable();
        for (int y = 0; y < 240; y++)
        {
            for (int x = 0; x < 240; x++)
            {
                //print("r:" + inputTensor[0, 0, i, j] + " g:" + inputTensor[0, 1, i, j] + " b:" + inputTensor[0, 2, i, j]);
                BITCH.SetPixel(x, 239-y, new Color(inputTensor[0, y, x, 0]/255f, inputTensor[0, y, x, 1]/255f, inputTensor[0, y, x, 2]/255f));
            }
        }
        BITCH.Apply();
        rawImage.texture = BITCH;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            worker1.Execute(inputTensor);
            TensorFloat outputTensor1 = worker1.PeekOutput() as TensorFloat;
            var coordinates = outputTensor1;
            coordinates.MakeReadable();

            var x = coordinates[0];
            var y = coordinates[1];
            Debug.Log($"x: {x}, y: {y}");
            inputTensor.MakeReadable();
            BITCH.SetPixel((int)x, (int)(240-y), new Color(0, 1, 0));
            BITCH.Apply();
            rawImage.texture = BITCH;

            worker2.Execute(inputTensor);
            TensorFloat outputTensor2 = worker2.PeekOutput() as TensorFloat;
            var probabilities = outputTensor2;
            var indexOfMaxProba = ops.ArgMax(probabilities, -1, false);
            probabilities.MakeReadable();
            indexOfMaxProba.MakeReadable();
            var predictedNumber = indexOfMaxProba[0];
            var probability = probabilities[predictedNumber];
            Debug.Log($"Predicted number: {predictedNumber} with probability: {probability}");
            outputTensor1?.Dispose();
            outputTensor2?.Dispose();
        }
    }

    void OnDestroy()
    {
        allocator?.Dispose();
        ops?.Dispose();
        worker1?.Dispose();
        worker2?.Dispose();
        inputTensor?.Dispose();
    }

    Texture2D Bilinear(Texture2D origImage, int newWidth, int newHeight)
    {
        Texture2D newImage = new Texture2D(newWidth, newHeight);

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


}

