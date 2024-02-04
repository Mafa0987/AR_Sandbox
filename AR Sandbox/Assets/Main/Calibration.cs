using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEditor.SearchService;
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
    GameObject waterpos;
    Slider maxTerrainSlider;
    Slider minTerrainSlider;
    Slider xCutSlider1;
    Slider xCutSlider2;
    Slider zCutSlider1;
    Slider zCutSlider2;
    public float maxTerrainHeight = 10;
    public float minTerrainHeight = 0f;
    public float rainOffset = 0f;
    public float rainHeight = 10;
    public Vector2Int xCut = new Vector2Int(0, 0);
    public Vector2Int zCut = new Vector2Int(0, 0);
    Vector2Int xCutNew = new Vector2Int(0, 0);
    Vector2Int zCutNew = new Vector2Int(0, 0);
    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("UI");
        Camera = GameObject.Find("Main Camera").transform;
        line1 = GameObject.Find("Line1").GetComponent<LineRenderer>();
        line2 = GameObject.Find("Line2").GetComponent<LineRenderer>();
        line3 = GameObject.Find("Line3").GetComponent<LineRenderer>();
        line4 = GameObject.Find("Line4").GetComponent<LineRenderer>();
        terrainpos = GameObject.Find("TerrainTexture");
        waterpos = GameObject.Find("WaterPlane");
        maxTerrainSlider = GameObject.Find("maxTerrainSlider").GetComponent<Slider>();
        minTerrainSlider = GameObject.Find("minTerrainSlider").GetComponent<Slider>();
        xCutSlider1 = GameObject.Find("xCutSlider1").GetComponent<Slider>();
        xCutSlider2 = GameObject.Find("xCutSlider2").GetComponent<Slider>();
        zCutSlider1 = GameObject.Find("zCutSlider1").GetComponent<Slider>();
        zCutSlider2 = GameObject.Find("zCutSlider2").GetComponent<Slider>();
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
        if (PlayerPrefs.HasKey("xCut1"))
        {
            xCut.x = PlayerPrefs.GetInt("xCut1");
            xCutNew.x = xCut.x;
            xCutSlider1.value = xCut.x;
        }
        if (PlayerPrefs.HasKey("xCut2"))
        {
            xCut.y = PlayerPrefs.GetInt("xCut2");
            xCutNew.y = xCut.y;
            xCutSlider2.value = xCut.y;
        }
        if (PlayerPrefs.HasKey("zCut1"))
        {
            zCut.x = PlayerPrefs.GetInt("zCut1");
            zCutNew.x = zCut.x;
            zCutSlider1.value = zCut.x;
        }
        if (PlayerPrefs.HasKey("zCut2"))
        {
            zCut.y = PlayerPrefs.GetInt("zCut2");
            zCutNew.y = zCut.y;
            zCutSlider2.value = zCut.y;
        }
        if (PlayerPrefs.HasKey("rotation"))
        {
            Camera.eulerAngles = new Vector3(Camera.rotation.x, PlayerPrefs.GetFloat("rotation"), Camera.rotation.z);
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
            minTerrainHeight = minTerrainSlider.value;
            maxTerrainHeight = maxTerrainSlider.value;
            xCutNew.x = (int)xCutSlider1.value;
            xCutNew.y = (int)xCutSlider2.value;
            zCutNew.x = (int)zCutSlider1.value;
            zCutNew.y = (int)zCutSlider2.value;
        }
        rainHeight = maxTerrainHeight + rainOffset;
        CalibrateTransform();
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
        Save();
    }

    public void ApplyCut()
    {
        Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Save()
    {
        Debug.Log("Saving calibration");
        PlayerPrefs.SetFloat("maxTerrainHeight", maxTerrainHeight);
        PlayerPrefs.SetFloat("minTerrainHeight", minTerrainHeight);
        PlayerPrefs.SetFloat("rainOffset", rainOffset);
        PlayerPrefs.SetInt("xCut1", xCutNew.x);
        PlayerPrefs.SetInt("xCut2", xCutNew.y);
        PlayerPrefs.SetInt("zCut1", zCutNew.x);
        PlayerPrefs.SetInt("zCut2", zCutNew.y);
        SaveTransform(terrainpos.transform);
        SaveTransform(waterpos.transform);
    }

}
