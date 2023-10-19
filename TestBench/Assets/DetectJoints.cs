using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class DetectJoints : MonoBehaviour
{

    public GameObject BodySrcManager;
    public JointType TrackedJoint;
    private BodySourceManager bodyManager;
    private Body[] bodies;
    // Start is called before the first frame update
    void Start()
    {
        if (BodySrcManager == null)
        {
            Debug.Log("Assign Game Ojbect with Body Source Manager");
        }
        else
        {
            bodyManager = BodySrcManager.GetComponent<BodySourceManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (BodySrcManager == null)
        {
            return;
        }
        else
        {
            bodies = bodyManager.GetData();
            if (bodies == null)
            {
                return;
            }
            foreach (var body in bodies)
            {
                if (body == null)
                {
                    continue;
                }
                if (body.IsTracked)
                {
                    var pos = body.Joints[TrackedJoint].Position;
                    gameObject.transform.position = new Vector3(pos.X * 10, pos.Y * 10);
                }
            }
        }
    }
}
