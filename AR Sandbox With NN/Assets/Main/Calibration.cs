using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{
    GameObject ui;
    LineRenderer line1;
    LineRenderer line2;
    LineRenderer line3;
    LineRenderer line4;
    Transform Camera;
    GameObject terrainpos;
    Slider maxTerrainSlider;
    public InputField maxTerrainInput;
    Slider minTerrainSlider;
    public InputField minTerrainInput;
    Slider xCutSlider1;
    public InputField xCutInput1;
    Slider xCutSlider2;
    public InputField xCutInput2;
    Slider zCutSlider1;
    public InputField zCutInput1;
    Slider zCutSlider2;
    public InputField zCutInput2;
    public InputField currentMinHeight;
    public float maxTerrainHeight = 10;
    public float minTerrainHeight = 0f;
    public float rainOffset = 0f;
    public float rainHeight = 10;
    public float depthShiftx = 0;
    public float indexShiftx = 0;
    public float centerX = 0;
    public float depthShifty = 0;
    public float indexShifty = 0;
    public float centerY = 0;
    // public float[] shift00;
    // public float[] shift10;
    // public float[] shift01;
    // public float[] shift11;
    public float[][] depthShiftArray;
    public float[][] shiftArray;
    public float trueMinHeight;
    public Vector2Int xCut = new Vector2Int(0, 0);
    public Vector2Int zCut = new Vector2Int(0, 0);
    Vector2Int xCutNew = new Vector2Int(0, 0);
    Vector2Int zCutNew = new Vector2Int(0, 0);
    // Start is called before the first frame update

    Vector3 screenPosition;
    float minIndex;
    float[] oldShift;
    bool setMinHeightActive = false;
    void Start()
    {
        // shift00 = new float[2];
        // shift10 = new float[2];
        // shift01 = new float[2];
        // shift11 = new float[2];
        depthShiftArray = new float[][]{new float[2], new float[2], new float[2], new float[2]};
        shiftArray = new float[][]{new float[2], new float[2], new float[2], new float[2]};


        ui = GameObject.Find("UI");
        Camera = GameObject.Find("Main Camera").transform;
        line1 = GameObject.Find("Line1").GetComponent<LineRenderer>();
        line2 = GameObject.Find("Line2").GetComponent<LineRenderer>();
        line3 = GameObject.Find("Line3").GetComponent<LineRenderer>();
        line4 = GameObject.Find("Line4").GetComponent<LineRenderer>();
        terrainpos = GameObject.Find("TerrainTexture");
        maxTerrainSlider = GameObject.Find("maxTerrainSlider").GetComponent<Slider>();
        minTerrainSlider = GameObject.Find("minTerrainSlider").GetComponent<Slider>();
        xCutSlider1 = GameObject.Find("xCutSlider1").GetComponent<Slider>();
        xCutSlider2 = GameObject.Find("xCutSlider2").GetComponent<Slider>();
        zCutSlider1 = GameObject.Find("zCutSlider1").GetComponent<Slider>();
        zCutSlider2 = GameObject.Find("zCutSlider2").GetComponent<Slider>();
        LoadTransform(terrainpos.transform);
        LoadShifts(depthShiftArray, "depthShift");
        LoadShifts(shiftArray, "shift");
        if (PlayerPrefs.HasKey("maxTerrainHeight"))
        {
            maxTerrainHeight = PlayerPrefs.GetFloat("maxTerrainHeight");
            maxTerrainSlider.value = maxTerrainHeight;
            maxTerrainInput.text = maxTerrainHeight.ToString();
        }
        if (PlayerPrefs.HasKey("minTerrainHeight"))
        {
            minTerrainHeight = PlayerPrefs.GetFloat("minTerrainHeight");
            minTerrainSlider.value = minTerrainHeight;
            minTerrainInput.text = minTerrainHeight.ToString();
        }
        if (PlayerPrefs.HasKey("trueMinHeight"))
        {
            trueMinHeight = PlayerPrefs.GetFloat("trueMinHeight");
            currentMinHeight.text = trueMinHeight.ToString();
        }
        if (PlayerPrefs.HasKey("rainOffset"))
        {
            rainOffset = PlayerPrefs.GetFloat("rainOffset");
        }
        if (PlayerPrefs.HasKey("rainHeight"))
        {
            rainHeight = PlayerPrefs.GetFloat("rainHeight");
        }        
        if (PlayerPrefs.HasKey("xCut1"))
        {
            xCut.x = PlayerPrefs.GetInt("xCut1");
            xCutNew.x = xCut.x;
            xCutSlider1.value = xCut.x;
            xCutInput1.text = xCut.x.ToString();
        }
        if (PlayerPrefs.HasKey("xCut2"))
        {
            xCut.y = PlayerPrefs.GetInt("xCut2");
            xCutNew.y = xCut.y;
            xCutSlider2.value = xCut.y;
            xCutInput2.text = xCut.y.ToString();
        }
        if (PlayerPrefs.HasKey("zCut1"))
        {
            zCut.x = PlayerPrefs.GetInt("zCut1");
            zCutNew.x = zCut.x;
            zCutSlider1.value = zCut.x;
            zCutInput1.text = zCut.x.ToString();
        }
        if (PlayerPrefs.HasKey("zCut2"))
        {
            zCut.y = PlayerPrefs.GetInt("zCut2");
            zCutNew.y = zCut.y;
            zCutSlider2.value = zCut.y;
            zCutInput2.text = zCut.y.ToString();
        }
        if (PlayerPrefs.HasKey("rotation"))
        {
            Camera.eulerAngles = new Vector3(90, PlayerPrefs.GetFloat("rotation"), 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ui.SetActive(!ui.activeSelf);
        }
        if (ui.activeSelf)
        {
            DrawRectangle();
            minTerrainSlider.onValueChanged.AddListener((v) => { 
                minTerrainHeight = v;
                minTerrainInput.text = v.ToString();});
            maxTerrainSlider.onValueChanged.AddListener((v) => {
                maxTerrainHeight = v;
                maxTerrainInput.text = v.ToString();});
            xCutSlider1.onValueChanged.AddListener((v) => {
                xCutNew.x = (int)v;
                xCutInput1.text = v.ToString();});
            xCutSlider2.onValueChanged.AddListener((v) => {
                xCutNew.y = (int)v;
                xCutInput2.text = v.ToString();});
            zCutSlider1.onValueChanged.AddListener((v) => {
                zCutNew.x = (int)v;
                zCutInput1.text = v.ToString();});
            zCutSlider2.onValueChanged.AddListener((v) => {
                zCutNew.y = (int)v;
                zCutInput2.text = v.ToString();});

            maxTerrainInput.onEndEdit.AddListener((v) => {
                maxTerrainHeight = float.Parse(v);
                maxTerrainSlider.value = maxTerrainHeight;});
            minTerrainInput.onEndEdit.AddListener((v) => {
                minTerrainHeight = float.Parse(v);
                minTerrainSlider.value = minTerrainHeight;});
            xCutInput1.onEndEdit.AddListener((v) => {
                xCutNew.x = int.Parse(v);
                xCutSlider1.value = xCutNew.x;});
            xCutInput2.onEndEdit.AddListener((v) => {
                xCutNew.y = int.Parse(v);
                xCutSlider2.value = xCutNew.y;});
            zCutInput1.onEndEdit.AddListener((v) => {
                zCutNew.x = int.Parse(v);
                zCutSlider1.value = zCutNew.x;});
            zCutInput2.onEndEdit.AddListener((v) => {
                zCutNew.y = int.Parse(v);
                zCutSlider2.value = zCutNew.y;});

            // maxTerrainHeight = maxTerrainSlider.value;
            // minTerrainHeight = minTerrainSlider.value;
            // xCutNew.x = (int)xCutSlider1.value;
            // xCutNew.y = (int)xCutSlider2.value;
            // zCutNew.x = (int)zCutSlider1.value;
            // zCutNew.y = (int)zCutSlider2.value;
        }
        rainHeight = maxTerrainHeight + rainOffset;
        CalibrateTransform();

        if (Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)))
        {
            screenPosition = Input.mousePosition;
            Vector3 worldPosition = Camera.GetComponent<Camera>().ScreenToWorldPoint(screenPosition);
            Vector3 terrainPosition = terrainpos.transform.InverseTransformPoint(worldPosition);
            Vector3[] vertices = terrainpos.GetComponent<TerrainGen>().vertices;
            Vector3[] corners = new Vector3[4];
            corners[0] = vertices[0];
            corners[1] = vertices[terrainpos.GetComponent<TerrainGen>().xSize - 1];
            corners[2] = vertices[terrainpos.GetComponent<TerrainGen>().xSize * (terrainpos.GetComponent<TerrainGen>().zSize - 1)];
            corners[3] = vertices[terrainpos.GetComponent<TerrainGen>().xSize * terrainpos.GetComponent<TerrainGen>().zSize - 1];
            float minDist = float.MaxValue;
            for (int k = 0; k < 4; k++)
            {
                float distance = Vector3.Distance(terrainPosition, corners[k]);
                if (distance < minDist)
                {
                    minDist = distance;
                    minIndex = k;
                    oldShift = Input.GetKey(KeyCode.LeftShift) ? (float[])shiftArray[k].Clone() : (float[])depthShiftArray[k].Clone();
                }
            }
        }

        if (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)))
        {
            Vector3 screenPositionNew = Input.mousePosition;
            float diffX = screenPositionNew.x - screenPosition.x;
            float diffY = screenPositionNew.y - screenPosition.y;
            if (Input.GetKey(KeyCode.LeftShift)){
                shiftArray[(int)minIndex][0] = oldShift[0] + diffY/10;
                shiftArray[(int)minIndex][1] = oldShift[1] - diffX/10;
            }
            else{
                depthShiftArray[(int)minIndex][0] = oldShift[0] + diffY/100;
                depthShiftArray[(int)minIndex][1] = oldShift[1] - diffX/100;
            }
        }

        if (setMinHeightActive && Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            Vector3 terrainPosition = terrainpos.transform.InverseTransformPoint(worldPosition);
            int x = Mathf.Clamp((int)terrainPosition.x, 0, terrainpos.GetComponent<TerrainGen>().xSize - 1);
            int z = Mathf.Clamp((int)terrainPosition.z, 0, terrainpos.GetComponent<TerrainGen>().zSize - 1);
            trueMinHeight = terrainpos.GetComponent<TerrainGen>().heightmap[x + z * terrainpos.GetComponent<TerrainGen>().xSize];
            Debug.Log("Setting min height to " + trueMinHeight);
            ui.SetActive(true);
            currentMinHeight.text = trueMinHeight.ToString();
            setMinHeightActive = false;
        }

    }

    void DrawRectangle()
    {
        int xCutDiff1 = xCutNew.x - xCut.x;
        int xCutDiff2 = xCutNew.y - xCut.y;
        int zCutDiff1 = zCutNew.x - zCut.x;
        int zCutDiff2 = zCutNew.y - zCut.y;
        int xSize = 512 - (xCut.x + xCut.y);
        int zSize = 424 - (zCut.x + zCut.y);
        line1.widthMultiplier = 5f;
        line2.widthMultiplier = 5f;
        line3.widthMultiplier = 5f;
        line4.widthMultiplier = 5f;
        Vector3 point1 = terrainpos.transform.position + new Vector3(xCutDiff1 * terrainpos.transform.localScale.x, 500, zCutDiff1 * terrainpos.transform.localScale.z);
        Vector3 point2 = point1 + new Vector3((xSize - (xCutDiff1+xCutDiff2)) * terrainpos.transform.localScale.x, 0, 0);
        Vector3 point3 = point2 + new Vector3(0, 0, (zSize - (zCutDiff1+zCutDiff2)) * terrainpos.transform.localScale.z);
        Vector3 point4 = point3 + new Vector3((-xSize + (xCutDiff1+xCutDiff2)) * terrainpos.transform.localScale.x, 0, 0);
        line1.SetPosition(0, point1);
        line1.SetPosition(1, point2);
        line2.SetPosition(0, point2);
        line2.SetPosition(1, point3);
        line3.SetPosition(0, point3);
        line3.SetPosition(1, point4);
        line4.SetPosition(0, point4);
        line4.SetPosition(1, point1);
    }

    void CalibrateTransform()
    {
        float dt = Time.deltaTime;
        float speed = 20f * dt;
        float scaleSpeed = 0.1f * dt;
        float rotateSpeed = 5f * dt;
        float orientation = Camera.eulerAngles.y;
        orientation = orientation >= 315 ? orientation - 315 : orientation + 45;
        orientation = Mathf.Clamp(Mathf.Floor(orientation / 360 * 4), 0, 3);

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.UpArrow))
        {
            float[] x = new float[4]{0, scaleSpeed, 0, scaleSpeed};
            float[] z = new float[4]{scaleSpeed, 0, scaleSpeed, 0};
            terrainpos.transform.localScale += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            float[] x = new float[4]{0, speed, 0, -speed};
            float[] z = new float[4]{speed, 0, -speed, 0};
            terrainpos.transform.position += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.DownArrow))
        {
            float[] x = new float[4]{0, -scaleSpeed, 0, -scaleSpeed};
            float[] z = new float[4]{-scaleSpeed, 0, -scaleSpeed, 0};
            terrainpos.transform.localScale += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            float[] x = new float[4]{0, -speed, 0, speed};
            float[] z = new float[4]{-speed, 0, speed, 0};
            terrainpos.transform.position += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightArrow))
        {
            float[] x = new float[4]{scaleSpeed, 0, -scaleSpeed, 0};
            float[] z = new float[4]{0, -scaleSpeed, 0, scaleSpeed};
            terrainpos.transform.localScale += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
            //terrainpos.transform.localScale += new Vector3(scaleSpeed, 0, scaleSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightArrow))
        {
            Camera.eulerAngles -= new Vector3(0, rotateSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            float[] x = new float[4]{speed, 0, -speed, 0};
            float[] z = new float[4]{0, -speed, 0, speed};
            terrainpos.transform.position += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftArrow))
        {
            float[] x = new float[4]{-scaleSpeed, 0, scaleSpeed, 0};
            float[] z = new float[4]{0, scaleSpeed, 0, -scaleSpeed};
            terrainpos.transform.localScale += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
            //terrainpos.transform.localScale += new Vector3(-scaleSpeed, 0, -scaleSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftArrow))
        {
            Camera.eulerAngles += new Vector3(0, rotateSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            float[] x = new float[4]{-speed, 0, speed, 0};
            float[] z = new float[4]{0, speed, 0, -speed};
            terrainpos.transform.position += new Vector3(x[(int)orientation], 0, z[(int)orientation]);
        }

        if (Input.GetKey(KeyCode.O))
        {
            maxTerrainHeight += 100f * dt;
            rainHeight = maxTerrainHeight + rainOffset;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            maxTerrainHeight -= 100f * dt;
            rainHeight = maxTerrainHeight + rainOffset;
        }
        if (Input.GetKey(KeyCode.I))
        {
            minTerrainHeight += 100f * dt;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            minTerrainHeight -= 100f * dt;
        }
        if (Input.GetKey(KeyCode.U))
        {
            rainOffset += 100f * dt;
            rainHeight = maxTerrainHeight + rainOffset;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            rainOffset -= 100f * dt;
            rainHeight = maxTerrainHeight + rainOffset;
        }
    }

    void SaveTransform(Transform transform)
    {
        PlayerPrefs.SetString(transform.name, transform.name);
        PlayerPrefs.SetFloat(transform.name + "x", transform.position.x);
        PlayerPrefs.SetFloat(transform.name + "y", transform.position.y);
        PlayerPrefs.SetFloat(transform.name + "z", transform.position.z);
        PlayerPrefs.SetFloat(transform.name + "sx", transform.localScale.x);
        PlayerPrefs.SetFloat(transform.name + "sy", transform.localScale.y);
        PlayerPrefs.SetFloat(transform.name + "sz", transform.localScale.z);
        PlayerPrefs.SetFloat(transform.name + "rx", transform.rotation.x);
        PlayerPrefs.SetFloat(transform.name + "ry", transform.rotation.y);
        PlayerPrefs.SetFloat(transform.name + "rz", transform.rotation.z);
        PlayerPrefs.SetFloat(transform.name + "rw", transform.rotation.w);
    }

    void LoadTransform(Transform transform)
    {
        if (PlayerPrefs.HasKey(transform.name))
        {
            transform.position = new Vector3(PlayerPrefs.GetFloat(transform.name + "x"), PlayerPrefs.GetFloat(transform.name + "y"), PlayerPrefs.GetFloat(transform.name + "z"));
            transform.localScale = new Vector3(PlayerPrefs.GetFloat(transform.name + "sx"), PlayerPrefs.GetFloat(transform.name + "sy"), PlayerPrefs.GetFloat(transform.name + "sz"));
            //transform.rotation = new Quaternion(PlayerPrefs.GetFloat(transform.name + "rx"), PlayerPrefs.GetFloat(transform.name + "ry"), PlayerPrefs.GetFloat(transform.name + "rz"), PlayerPrefs.GetFloat(transform.name + "rw"));
        }
    }

    void SaveShifts(float[][] array, string name)
    {
        PlayerPrefs.SetString(name, name);
        PlayerPrefs.SetFloat(name + "00x", array[0][0]);
        PlayerPrefs.SetFloat(name + "00y", array[0][1]);
        PlayerPrefs.SetFloat(name + "10x", array[1][0]);
        PlayerPrefs.SetFloat(name + "10y", array[1][1]);
        PlayerPrefs.SetFloat(name + "01x", array[2][0]);
        PlayerPrefs.SetFloat(name + "01y", array[2][1]);
        PlayerPrefs.SetFloat(name + "11x", array[3][0]);
        PlayerPrefs.SetFloat(name + "11y", array[3][1]);
    }

    void LoadShifts(float[][] array, string name)
    {
        if (PlayerPrefs.HasKey(name)){
            array[0][0] = PlayerPrefs.GetFloat(name + "00x");
            array[0][1] = PlayerPrefs.GetFloat(name + "00y");
            array[1][0] = PlayerPrefs.GetFloat(name + "10x");
            array[1][1] = PlayerPrefs.GetFloat(name + "10y");
            array[2][0] = PlayerPrefs.GetFloat(name + "01x");
            array[2][1] = PlayerPrefs.GetFloat(name + "01y");
            array[3][0] = PlayerPrefs.GetFloat(name + "11x");
            array[3][1] = PlayerPrefs.GetFloat(name + "11y");
        }
    }

    void OnApplicationQuit()
    {
        Save();
    }

    public void ApplyCut()
    {
        Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetMinHeight()
    {
        ui.SetActive(false);
        setMinHeightActive = true;
    }

    public void ResetShifts()
    {
        for (int i = 0; i < 4; i++)
        {
            depthShiftArray[i] = new float[2];
            shiftArray[i] = new float[2];
        }
    }

    void Save()
    {
        Debug.Log("Saving calibration");
        PlayerPrefs.SetFloat("maxTerrainHeight", maxTerrainHeight);
        PlayerPrefs.SetFloat("minTerrainHeight", minTerrainHeight);
        PlayerPrefs.SetFloat("rainOffset", rainOffset);
        PlayerPrefs.SetFloat("rotation", Camera.eulerAngles.y);
        PlayerPrefs.SetInt("xCut1", xCutNew.x);
        PlayerPrefs.SetInt("xCut2", xCutNew.y);
        PlayerPrefs.SetInt("zCut1", zCutNew.x);
        PlayerPrefs.SetInt("zCut2", zCutNew.y);
        PlayerPrefs.SetFloat("trueMinHeight", trueMinHeight);
        SaveTransform(terrainpos.transform);
        SaveShifts(depthShiftArray, "depthShift");
        SaveShifts(shiftArray, "shift");
    }

}
