using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NerualNetwork : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var model = ModelLoader.Load(filename);
        var engine = WorkerFactory.CreateWorker(model, WorkerFactory.Device.GPU);

        var input = new Tensor(1, 1, 1, 10);
        var output = engine.Execute(input).PeekOutput();
    }
}
