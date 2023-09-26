using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class imageviewer : MonoBehaviour
{
    public MeasureDepth measureDepth;
    public MultiSourceManager msm;

    public RawImage rawImage;
    public RawImage rawDepth;


    // Update is called once per frame
    void Update()
    {
        rawImage.texture = msm.GetColorTexture();
        rawDepth.texture = measureDepth.mDepthTexture;
    }
}
