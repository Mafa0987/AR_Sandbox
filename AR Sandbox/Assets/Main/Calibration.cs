using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    GameObject terrainpos;
    GameObject waterpos;
    Slider maxTerrainSlider;
    Slider minTerrainSlider;
    public float maxTerrainHeight = 10;
    public float minTerrainHeight = 0f;
    public float rainOffset = 0f;
    public float rainHeight = 10;
    // Start is called before the first frame update
    void Start()
    {
        terrainpos = GameObject.Find("TerrainTexture");
        waterpos = GameObject.Find("WaterPlane");
        maxTerrainSlider = GameObject.Find("maxTerrainSlider").GetComponent<Slider>();
        minTerrainSlider = GameObject.Find("minTerrainSlider").GetComponent<Slider>();
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
    }

    // Update is called once per frame
    void Update()
    {
        minTerrainHeight = minTerrainSlider.value;
        maxTerrainHeight = maxTerrainSlider.value;
        rainHeight = maxTerrainHeight + rainOffset;
        CalibrateTransform();
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
            terrainpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
            waterpos.transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
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
            terrainpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
            waterpos.transform.rotation *= Quaternion.Euler(0, -rotateSpeed, 0);
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
            transform.rotation = new Quaternion(PlayerPrefs.GetFloat(transform.name + "rx"), PlayerPrefs.GetFloat(transform.name + "ry"), PlayerPrefs.GetFloat(transform.name + "rz"), PlayerPrefs.GetFloat(transform.name + "rw"));
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Saving calibration");
        PlayerPrefs.SetFloat("maxTerrainHeight", maxTerrainHeight);
        float test = PlayerPrefs.GetFloat("maxTerrainHeight");
        PlayerPrefs.SetFloat("minTerrainHeight", minTerrainHeight);
        PlayerPrefs.SetFloat("rainOffset", rainOffset);
        SaveTransform(terrainpos.transform);
        SaveTransform(waterpos.transform);
    }

}
