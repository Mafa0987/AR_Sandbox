using UnityEngine;

public class gadery : MonoBehaviour
{
    public ComputeShader shader;
    public ComputeBuffer buffer;
    float[] data = new float[512*424];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buffer = new ComputeBuffer(512*424, sizeof(float));
        shader.SetBuffer(0, "Data", buffer);
    }

    // Update is called once per frame
    void Update()
    {
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
        shader.Dispatch(0, 512/8, 424/8, 1);
        buffer.GetData(data);
    }
}
