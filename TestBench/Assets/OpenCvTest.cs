using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class OpenCvTest : MonoBehaviour
{
    public RawImage rawImage;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Texture2D tex = LoadPNG("Assets/hand.png");
        rawImage.texture = tex;
        Mat src = OpenCvSharp.Unity.TextureToMat(tex);
        Mat dst = new Mat();
        Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
        //Find Contours in image
        Point[][] contours; // list of contour points
        HierarchyIndex[] hierarchy;
        // find contours
        Cv2.FindContours(dst, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

        MatOfPoint contour = MatOfPoint.FromArray(contours[0]);
        // create hull array for convex hull points
        MatOfInt hull = new MatOfInt();
        Cv2.ConvexHull(contour, hull, clockwise: false, returnPoints: false);
        Vec4i[] defects = Cv2.ConvexityDefects(contours[0], hull);

        for (int i = 0; i < defects.Length; i++)
        {
            if (defects[i].Item3 > 10000)
                Cv2.Circle(src, contours[0][defects[i].Item2], 5, new Scalar(255, 0, 0), 3);
        }


        




        //Cv2.Polylines(src, new Point[][] { hull }, true, new Scalar(0, 0, 255), 3);
        // //Draw Contours
        Cv2.DrawContours(src, contours, 0, new Scalar(0, 255, 0), 3);
        rawImage.texture = OpenCvSharp.Unity.MatToTexture(src);
    }

    public static Texture2D LoadPNG(string filePath) 
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)) 	{
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }   
}
