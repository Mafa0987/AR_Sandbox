using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UserTest : MonoBehaviour
{
    public RectTransform textTransform;
    public TMP_Text text;
    public TerrainGen terrain;
    public Water water;
    bool drag = false;
    int level = 0;
    float rainPercentage = 0;
    float landWaterRatio = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(level)
        {
            case 0:
                text.text = @"Once upon a time, in a augmented sandbox world,
an intricate ecosystem thrive where 
every grain of sand represented life.

The sand formed the ground, where tiny animals roamed. 
Below this threshold, the sand transformed
into shimmering ponds,
teeming with colorful fishes. 

The delicate existence of life in the AR Sandbox 
depended on the balance between the land and water.";
                if (Input.GetKey(KeyCode.Return))
                {
                    level++;
                }
                break;
            case 1:
                text.text = @"Water Imbalance Warning (General): 
Ecosystem imbalance detected! 
Restore harmony before irreversible 
consequences occur.";
                if (landWaterRatio > 0.25)
                {
                    level++;
                }
                break;
            case 2:
                text.text = "The percentage of water that is deeper than 5 units is " + rainPercentage + ".\n This is the percentage of water that is deeper than 5 units.";
                break;
            case 3:
                text.text = "The current level is " + level + ".\n This is the current level of the user test.";
                break;
            default:
                text.text = "The user test is complete.\n Thank you for participating!";
                break;
        }
        //temp
        float sum = 0;
        for (int i = 0; i < terrain.heightmap.Length; i++)
        {
            float height_norm = (terrain.heightmap[i] - terrain.minTerrainHeight) / (terrain.maxTerrainHeight - terrain.minTerrainHeight);
            if (height_norm < 0.477){
                sum += 1;
            }
        }
        landWaterRatio = sum / terrain.heightmap.Length;
        Debug.Log(landWaterRatio);
        //
        // temp
        float depthSum = 0;
        for (int i = 0; i < water.vertices.Length; i++)
        {
            float depth = water.depthMap[i];
            if (depth > 5f)
            {
                depthSum += 1;
            }
        }
        rainPercentage = depthSum / water.vertices.Length;
        Debug.Log("rain " + rainPercentage);
        //


        if (drag)
        {
            textTransform.position = Input.mousePosition;
            if (Input.mouseScrollDelta.y > 0)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    text.fontSize += 1;
                }
                else
                {
                    textTransform.eulerAngles += new Vector3(0, 0, 10);

                }
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    text.fontSize -= 1;
                }
                else
                {
                    textTransform.eulerAngles -= new Vector3(0, 0, 10);
                }
            }
        }
    }

    public void TextClick()
    {
        drag = true;
    }

    public void TextRelease()
    {
        drag = false;
    }
}
