using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{
    LineRenderer line1;
    LineRenderer line2;
    LineRenderer line3;
    LineRenderer line4;
    Transform Camera;
    Transform sphere;
    GameObject terrainpos;
    GameObject waterpos;
    Slider maxTerrainSlider;
    Slider minTerrainSlider;
    Slider xCutSlider;
    Slider zCutSlider;
    public float maxTerrainHeight = 10;
    public float minTerrainHeight = 0f;
    public float rainOffset = 0f;
    public float rainHeight = 10;
    public int xCut = 100;
    public int zCut = 0;
    int xCutNew = 0;
    int zCutNew = 0;
    // Start is called before the first frame update
    void Start()
    {
        Camera = GameObject.Find("Main Camera").transform;
        sphere = GameObject.Find("Sphere").transform;
        line1 = GameObject.Find("Line1").GetComponent<LineRenderer>();
        line2 = GameObject.Find("Line2").GetComponent<LineRenderer>();
        line3 = GameObject.Find("Line3").GetComponent<LineRenderer>();
        line4 = GameObject.Find("Line4").GetComponent<LineRenderer>();
        terrainpos = GameObject.Find("TerrainTexture");
        waterpos = GameObject.Find("WaterPlane");
        maxTerrainSlider = GameObject.Find("maxTerrainSlider").GetComponent<Slider>();
        minTerrainSlider = GameObject.Find("minTerrainSlider").GetComponent<Slider>();
        xCutSlider = GameObject.Find("xCutSlider").GetComponent<Slider>();
        zCutSlider = GameObject.Find("zCutSlider").GetComponent<Slider>();
        LoadTransform(terrainpos.transform);
        LoadTransform(waterpos.transform);
        if (PlayerPrefs.HasKey("maxTerrainHeight"))
        {
            maxTerrainHeight = PlayerPrefs.GetFloat("maxTerrainHeight");
            maxTerrainSlider.value = maxTerrainHeight;
        }
        if (PlayerPrefs.HasKey("minTerrainHeight"))
        {
            minTerrainHeight = PlayerPrefs.GetFloat("minTerrainHeight");
            minTerrainSlider.value = minTerrainHeight;
        }
        if (PlayerPrefs.HasKey("rainOffset"))
        {
            rainOffset = PlayerPrefs.GetFloat("rainOffset");
        }
        if (PlayerPrefs.HasKey("rainHeight"))
        {
            rainHeight = PlayerPrefs.GetFloat("rainHeight");
        }        
        if (PlayerPrefs.HasKey("xCut"))
        {
            xCut = PlayerPrefs.GetInt("xCut");
            xCutNew = xCut;
        }
        if (PlayerPrefs.HasKey("zCut"))
        {
            zCut = PlayerPrefs.GetInt("zCut");
            zCutNew = zCut;
        }
        if (PlayerPrefs.HasKey("rotation"))
        {
            Camera.eulerAngles = new Vector3(Camera.rotation.x, PlayerPrefs.GetFloat("rotation"), Camera.rotation.z);
        }
        if (PlayerPrefs.HasKey("xCut"))
        {
            xCut = PlayerPrefs.GetInt("xCut");
            xCutNew = xCut;
            xCutSlider.value = xCut;
        }
        if (PlayerPrefs.HasKey("zCut"))
        {
            zCut = PlayerPrefs.GetInt("zCut");
            zCutNew = zCut;
            zCutSlider.value = zCut;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawRectangle();
        sphere.position = terrainpos.transform.position;
        minTerrainHeight = minTerrainSlider.value;
        maxTerrainHeight = maxTerrainSlider.value;
        xCutNew = (int)xCutSlider.value;
        zCutNew = (int)zCutSlider.value;
        rainHeight = maxTerrainHeight + rainOffset;
        CalibrateTransform();
    }

    void DrawRectangle()
    {
        int xCutDiff = xCutNew - xCut;
        int zCutDiff = zCutNew - zCut;
        int xSize = 512 - xCut * 2;
        int zSize = 424 - zCut * 2;
        line1.widthMultiplier = 5f;
        line2.widthMultiplier = 5f;
        line3.widthMultiplier = 5f;
        line4.widthMultiplier = 5f;
        Vector3 point1 = terrainpos.transform.position + new Vector3(xCutDiff, 500, zCutDiff);
        Vector3 point2 = point1 + new Vector3((xSize - xCutDiff*2) * terrainpos.transform.localScale.x, 0, 0);
        Vector3 point3 = point2 + new Vector3(0, 0, (zSize - zCutDiff*2) * terrainpos.transform.localScale.z);
        Vector3 point4 = point3 + new Vector3((-xSize + xCutDiff*2) * terrainpos.transform.localScale.x, 0, 0);
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

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.UpArrow))
        {
            terrainpos.transform.localScale += new Vector3(0, 0, scaleSpeed);
            waterpos.transform.localScale += new Vector3(0, 0, scaleSpeed);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            terrainpos.transform.position += new Vector3(0, 0, speed);
            waterpos.transform.position += new Vector3(0, 0, speed);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.DownArrow))
        {
            terrainpos.transform.localScale += new Vector3(0, 0, -scaleSpeed);
            waterpos.transform.localScale += new Vector3(0, 0, -scaleSpeed);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            terrainpos.transform.position += new Vector3(0, 0, -speed);
            waterpos.transform.position += new Vector3(0, 0, -speed);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightArrow))
        {
            terrainpos.transform.localScale += new Vector3(scaleSpeed, 0, 0);
            waterpos.transform.localScale += new Vector3(scaleSpeed, 0, 0);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightArrow))
        {
            // terrainpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
            // waterpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
            Camera.rotation *= Quaternion.Euler(0, 0, rotateSpeed);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            terrainpos.transform.position += new Vector3(speed, 0, 0);
            waterpos.transform.position += new Vector3(speed, 0, 0);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftArrow))
        {
            terrainpos.transform.localScale += new Vector3(-scaleSpeed, 0, 0);
            waterpos.transform.localScale += new Vector3(-scaleSpeed, 0, 0);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftArrow))
        {
            // terrainpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
            // waterpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
            Camera.rotation *= Quaternion.Euler(0, 0, -rotateSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            terrainpos.transform.position += new Vector3(-speed, 0, 0);
            waterpos.transform.position += new Vector3(-speed, 0, 0);
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

    void OnApplicationQuit()
    {
        Debug.Log("Saving calibration");
        PlayerPrefs.SetFloat("maxTerrainHeight", maxTerrainHeight);
        float test = PlayerPrefs.GetFloat("maxTerrainHeight");
        PlayerPrefs.SetFloat("minTerrainHeight", minTerrainHeight);
        PlayerPrefs.SetFloat("rainOffset", rainOffset);
        PlayerPrefs.SetInt("xCut", xCut);
        PlayerPrefs.SetInt("zCut", zCut);
        SaveTransform(terrainpos.transform);
        SaveTransform(waterpos.transform);
    }

    public void ApplyCut()
    {
        PlayerPrefs.SetInt("xCut", xCutNew);
        PlayerPrefs.SetInt("zCut", zCutNew);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
