using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    float[] array = new float[512*424];
    float sum = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 512*424; i++)
        {
            array[i] = i/1000.0f;
        }
        int test = 1;
        Cv2.ImRead("test");
    }

    // Update is called once per frame
    void Update()
    {
        // sum = 0;
        // for (int i = 0; i < 512*424; i++)
        // {
        //     sum += array[i];
        // }
        // int test = 1;
    }
}
