using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour
{
    public MultiSourceManager msm; 
    public Texture2D mDepthTexture;
    private ushort[] depthData = null;
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;
    private ushort[] mDepthData = null;
    private KinectSensor sensor = null;

    private readonly Vector2Int mDepthResolution = new Vector2Int(512, 424);
    private CoordinateMapper mMapper = null;

    // Start is called before the first frame update
    void Awake()
    {
        sensor = KinectSensor.GetDefault();
        mMapper = sensor.CoordinateMapper;
        int arraySize = mDepthResolution.x * mDepthResolution.y;
        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints = new ColorSpacePoint[arraySize];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DepthToColor();
            mDepthTexture = CreateTexture();
        }
        
    }

    private void DepthToColor()
    {
        mDepthData = msm.GetDepthData();
        mMapper.MapDepthFrameToCameraSpace(mDepthData, mCameraSpacePoints);
        mMapper.MapDepthFrameToColorSpace(mDepthData, mColorSpacePoints);
    }
    private Texture2D CreateTexture()
    {
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false);
        for(int x = 0; x < 1920; x++)
        {
            for(int y = 0; y < 1080; y++)
            {
                newTexture.SetPixel(x, y, Color.clear);
            }
        }
        foreach(ColorSpacePoint point in mColorSpacePoints)
        {
            newTexture.SetPixel((int)point.X, (int)point.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }
}
