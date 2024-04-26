using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    public Transform[] fish;
    public Transform[] deer;
    public TerrainGen terrain;
    public Transform terrainTransform;
    public Calibration calibration;
    public NeuralNetwork nn;
    float height;
    int[] fishRotateVal;
    int[] deerRotateVal;
    float[] survivalTimerFish;
    float[] survivalTimerDeer;
    bool[] deadFish;
    bool[] deadDeer;
    // Start is called before the first frame update
    void Start()
    {
        fishRotateVal = new int[fish.Length];
        deerRotateVal = new int[deer.Length];
        survivalTimerFish = new float[fish.Length];
        survivalTimerDeer = new float[deer.Length];
        deadFish = new bool[fish.Length];
        deadDeer = new bool[deer.Length];
        for (int i = 0; i < fish.Length; i++)
        {
            int x = Random.Range(0, terrain.xSize);
            int z = Random.Range(0, terrain.zSize);
            fish[i].localPosition = new Vector3(x, 1000, z);
        }
        for (int i = 0; i < deer.Length; i++)
        {
            int x = Random.Range(0, terrain.xSize);
            int z = Random.Range(0, terrain.zSize);
            deer[i].localPosition = new Vector3(x, 1000, z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < fish.Length; i++)
        {
            MoveAnimal(fish[i], 0.45f, 0f, ref fishRotateVal[i], ref survivalTimerFish[i], ref deadFish[i]);
        }   
        for (int i = 0; i < deer.Length; i++)
        {
            MoveAnimal(deer[i], 0.7f, 0.5f, ref deerRotateVal[i], ref survivalTimerDeer[i], ref deadDeer[i]);
        }
    }

    void MoveAnimal(Transform animal, float upperLim, float lowerLim, ref int rotateVal, ref float survivalTimer, ref bool dead)
    {
        float currentHeight = terrain.heightmap[Mathf.RoundToInt(animal.localPosition.x) + terrain.xSize * Mathf.RoundToInt(animal.localPosition.z)];
        float currentHeightNorm = (currentHeight - calibration.minTerrainHeight) / (calibration.maxTerrainHeight - calibration.minTerrainHeight);
        if (currentHeightNorm < lowerLim || currentHeightNorm > upperLim)
        {
            survivalTimer += Time.deltaTime;
            dead = survivalTimer >= 5;
        }
        else
        {
            if (dead)
                animal.localScale = new Vector3(25, 25, 25);
            dead = false;
            survivalTimer = 0;
        }
        if (dead)
        {
            animal.localScale = new Vector3(0, 0, 0);
            AdjustPosition(animal);
            return;
        }
        animal.localScale = new Vector3(25, 25, 25);       
        Vector3 rotation = animal.forward;
        Vector3 nextStep = animal.localPosition + rotation * Time.deltaTime * 20f;
        Vector3Int nextLook = Vector3Int.RoundToInt(animal.localPosition + rotation * 20);
        bool outOfBounds = nextLook.x >= terrain.xSize || nextLook.z >= terrain.zSize || nextLook.x <= 0 || nextLook.z <= 0;
        if (outOfBounds)
        {
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + 180, animal.eulerAngles.z);
            AdjustPosition(animal);
            return;
        }
        height = terrain.heightmap[nextLook.x + terrain.xSize * nextLook.z];
        height = (height - calibration.minTerrainHeight) / (calibration.maxTerrainHeight-calibration.minTerrainHeight);
        if (height < upperLim && height > lowerLim)
        {
            if (nn.predictedLabel != "No Hand")
            {
                Vector3 targetDirection = new Vector3(animal.localPosition.x - nn.x_cord, animal.forward.y, animal.localPosition.z - nn.y_cord);
                if (targetDirection.magnitude < 100.0)
                {
                    Vector3 newDirection = Vector3.RotateTowards(animal.forward, targetDirection, 100 * Time.deltaTime, 0.0f);
                    animal.localRotation = Quaternion.LookRotation(newDirection);
                    rotateVal = 0;
                    animal.localPosition = nextStep;
                    animal.position = new Vector3(animal.position.x, currentHeight, animal.position.z);
                    animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + Random.Range(-100 * Time.deltaTime, 100 * Time.deltaTime), animal.eulerAngles.z);
                    AdjustPosition(animal);
                    return;
                }
            }
            rotateVal = 0;
            animal.localPosition = nextStep;
            animal.position = new Vector3(animal.position.x, currentHeight, animal.position.z);
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + Random.Range(-100 * Time.deltaTime, 100 * Time.deltaTime), animal.eulerAngles.z);
            AdjustPosition(animal);
            return;
        }
        else
        {
            Vector3Int nextLookLeft = Vector3Int.RoundToInt(nextLook - animal.right * 10);
            Vector3Int nextLookRight = Vector3Int.RoundToInt(nextLook + animal.right * 10);
            bool outOfBoundsLeft = nextLookLeft.x >= terrain.xSize || nextLookLeft.z >= terrain.zSize || nextLookLeft.x <= 0 || nextLookLeft.z <= 0;
            bool outOfBoundsRight = nextLookRight.x >= terrain.xSize || nextLookRight.z >= terrain.zSize || nextLookRight.x <= 0 || nextLookRight.z <= 0;
            bool leftWithinEnv;
            bool rightWithinEnv;
            if (!outOfBoundsLeft){
                float heightLeft = terrain.heightmap[Mathf.RoundToInt(nextLookLeft.x) + terrain.xSize * Mathf.RoundToInt(nextLookLeft.z)];
                heightLeft = (heightLeft - calibration.minTerrainHeight) / (calibration.maxTerrainHeight - calibration.minTerrainHeight);
                leftWithinEnv = heightLeft < upperLim && heightLeft > lowerLim;
            }
            else{
                leftWithinEnv = false;
            }
            if (!outOfBoundsRight){
                float heightRight = terrain.heightmap[Mathf.RoundToInt(nextLookRight.x) + terrain.xSize * Mathf.RoundToInt(nextLookRight.z)];
                heightRight = (heightRight - calibration.minTerrainHeight) / (calibration.maxTerrainHeight - calibration.minTerrainHeight);
                rightWithinEnv = heightRight < upperLim && heightRight > lowerLim;
            }
            else{
                rightWithinEnv = false;
            }
            if (rotateVal == 0)
                rotateVal = leftWithinEnv && !rightWithinEnv ? -100 : 100;
            Vector3 nextStep2 = animal.localPosition + rotation * Time.deltaTime * 10f;
            animal.localPosition = nextStep2;
            animal.eulerAngles = new Vector3(animal.eulerAngles.x, animal.eulerAngles.y + rotateVal * Time.deltaTime, animal.eulerAngles.z);
        }
        AdjustPosition(animal);
    }

    void AdjustPosition(Transform animal)
    {
        int x1 = (int)Mathf.Floor(animal.localPosition.x);
        int z1 = (int)Mathf.Floor(animal.localPosition.z);
        int x2 = Mathf.Min(x1 + 1, terrain.xSize - 1);
        int z2 = Mathf.Min(z1 + 1, terrain.zSize - 1);
        Vector3 pos00 = terrain.vertices[x1 + z1 * terrain.xSize];
        Vector3 pos10 = terrain.vertices[x2 + z1 * terrain.xSize];
        Vector3 pos01 = terrain.vertices[x1 + z2 * terrain.xSize];
        Vector3 pos11 = terrain.vertices[x2 + z2 * terrain.xSize];
        Vector3 pos0 = Vector3.Lerp(pos00, pos10, animal.localPosition.x - x1);
        Vector3 pos1 = Vector3.Lerp(pos01, pos11, animal.localPosition.x - x1);
        Vector3 pos = Vector3.Lerp(pos0, pos1, animal.localPosition.z - z1);
        Vector3 worldPos = terrainTransform.TransformPoint(pos);
        Transform rig = animal.GetChild(0);
        rig.position = worldPos;
    }
}
