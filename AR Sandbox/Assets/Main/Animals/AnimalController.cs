using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    public Transform[] fish;
    public Transform[] deer;
    public TerrainGen terrain;
    public Calibration calibration;
    float height;
    //public Transform deer;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < fish.Length; i++)
        {
            //fish[i].position = new Vector3(Random.Range(0, terrain.xSize), 1000, Random.Range(0, terrain.zSize));
            fish[i].position = new Vector3(100, 1000, 200);
        }
        for (int i = 0; i < deer.Length; i++)
        {
            deer[i].position = new Vector3(170, 1000, 350);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < fish.Length; i++)
        {
            MoveAnimal(fish[i], 0.45f, 0f);
        }   
        for (int i = 0; i < deer.Length; i++)
        {
            MoveAnimal(deer[i], 1f, 0.5f);
        }
    }

    void MoveAnimal(Transform animal, float upperLim, float lowerLim)
    {
        float currentHeight = terrain.heightmap[Mathf.RoundToInt(animal.localPosition.x) + terrain.xSize * Mathf.RoundToInt(animal.localPosition.z)];
        Vector3 rotation = animal.forward;
        Vector3 nextStep = animal.localPosition + rotation * Time.deltaTime * 20f;
        Vector3Int nextLook = Vector3Int.RoundToInt(animal.localPosition + rotation * 20);
        bool outOfBounds = nextLook.x >= terrain.xSize || nextLook.z >= terrain.zSize || nextLook.x <= 0 || nextLook.z <= 0;
        Debug.Log(outOfBounds);
        if (outOfBounds)
        {
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + 180, animal.eulerAngles.z);
            return;
        }
        height = terrain.heightmap[nextLook.x + terrain.xSize * nextLook.z];
        height = (height - calibration.minTerrainHeight) / (calibration.maxTerrainHeight-calibration.minTerrainHeight);
        if (height < upperLim && height > lowerLim)
        {
            animal.localPosition = nextStep;
            animal.position = new Vector3(animal.position.x, currentHeight, animal.position.z);
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + Random.Range(-100 * Time.deltaTime, 100 * Time.deltaTime), animal.eulerAngles.z);
            return;
        }
        else
        {
            Vector3Int nextLookLeft = Vector3Int.RoundToInt(nextLook - animal.right * 10);
            Vector3Int nextLookRight = Vector3Int.RoundToInt(nextLook + animal.right * 10);
            bool outOfBoundsLeft = nextLookLeft.x >= terrain.xSize || nextLookLeft.z >= terrain.zSize || nextLookLeft.x <= 0 || nextLookLeft.z <= 0;
            bool outOfBoundsRight = nextLookRight.x >= terrain.xSize || nextLookRight.z >= terrain.zSize || nextLookRight.x <= 0 || nextLookRight.z <= 0;
            if (outOfBoundsLeft && outOfBoundsRight)
            {
                animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + 180, animal.eulerAngles.z);
                return;
            }
            else if (outOfBoundsLeft)
            {
                animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + 100 * Time.deltaTime, animal.eulerAngles.z);
                return;
            }
            else if (outOfBoundsRight)
            {
                animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y - 100 * Time.deltaTime, animal.eulerAngles.z);
                return;
            }
            float heightLeft = terrain.heightmap[Mathf.RoundToInt(nextLookLeft.x) + terrain.xSize * Mathf.RoundToInt(nextLookLeft.z)];
            float heightRight = terrain.heightmap[Mathf.RoundToInt(nextLookRight.x) + terrain.xSize * Mathf.RoundToInt(nextLookRight.z)];
            heightLeft = (heightLeft - calibration.minTerrainHeight) / (calibration.maxTerrainHeight - calibration.minTerrainHeight);
            heightRight = (heightRight - calibration.minTerrainHeight) / (calibration.maxTerrainHeight - calibration.minTerrainHeight);
            bool leftWater = heightLeft < upperLim && heightLeft > lowerLim;
            bool rightWater = heightRight < upperLim && heightRight > lowerLim;
            int rotateVal = leftWater && !rightWater ? -100 : 100;
            Vector3 nextStep2 = animal.localPosition + rotation * Time.deltaTime * 1f;
            animal.localPosition = nextStep2;
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + rotateVal * Time.deltaTime, animal.eulerAngles.z);
        }
    }
}
