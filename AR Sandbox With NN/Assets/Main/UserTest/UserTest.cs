using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserTest : MonoBehaviour
{
    public RectTransform textTransform;
    public TMP_Text text;
    public Image image;
    public TerrainGen terrain;
    public Water water;
    bool drag = false;
    int level = 0;
    float rainPercentage = 0;
    float landWaterRatio = 0;
    float timer = 0;
    public AnimalController animalController;
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

                water.rainEnabled = true;
                text.text = @"Once upon a time, in a augmented sandbox world,
an intricate ecosystem thrive where 
every grain of sand represented life.

The sand formed the ground, where tiny animals roamed. 
Below this threshold, the sand transformed
into shimmering ponds,
teeming with colorful fishes. 

The delicate existence of life in the AR Sandbox 
depended on the balance between the land and water.";
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                }
                break;

            case 1:

                water.rainEnabled = false;
                text.text = @"Ecosystem imbalance detected! 
                
Restore harmony before irreversible consequences occur.

   - In this AR Sandbox Symbiosis game, 
     you have to create a habitat balance between fishes and animals.
   - Hint: More fishes will thrive on medium blue sea level 
     and more animals thrive on a greener ground.
   - Move the sand level to the right balance between ground 
     and water level.";
                int fishAlive = 0;
                for (int i = 0; i < animalController.fish.Length; i++)
                {
                    Animal fish = animalController.fish[i].GetComponent<Animal>();
                    if (!fish.dead)
                    {
                        fishAlive++;
                    }
                }

                if (fishAlive > 2)
                {
                    level++;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                }
                break;

            case 2:
                water.rainEnabled = false;
                text.text = @"Congratulations! 
The balance of life is achieved. 
You are now in the next level";
                timer += Time.deltaTime;
                if (timer > 5)
                {
                    timer = 0;
                    level++;
                }
                break;
            case 3:

                water.rainEnabled = true;
                text.text = @"The fishes and animals need rain 
to grow more in number.
   - Add rain to achieve that.";

                if (rainPercentage > 0.4)
                {
                    level++;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                }
                break;

            case 4:
                water.rainEnabled = false;
                text.text = @"Congratulations! 
Ecosystem restored, 
the balance of life is achieved.";
                timer += Time.deltaTime;
                if (timer > 5)
                {
                    timer = 0;
                    level++;
                }
                break;

            case 5:
                image.color = new Color(1, 0, 0, 1);
                text.text = @"Storm is currently in the habitat!";
                timer += Time.deltaTime;
                if (timer > 5)
                {
                    timer = 0;
                    level++;
                }
                break;

            case 6:
                text.text = @"Ground depletion detected! 
Animal populations are dwindling due to excessive rain. 
Restore land levels to sustain their habitat.

   - The habitat has more than enough fishes in the sea.
   - You have 30 seconds to revive the animals.";
                timer += Time.deltaTime;
                if (timer > 5)
                {
                    timer = 0;
                    level++;
                }
                break;

            case 7:
                water.addRain = true;
                timer += Time.deltaTime;
                int time = (int)timer;
                text.text = @"You need to raise the ground to save the animals.
                
   - Time remaining: " + (30 - time) + " seconds.";
                if (timer > 20)
                {
                    text.text = @"Emergency: Ecosystem critical! 
Imbalance between water and land 
is causing widespread devastation. 
Immediate intervention required 
to prevent total collapse of the ecosystem
   - Time remaining: " + (30 - time) + " seconds.";
                }
                if (timer > 30)
                {
                    timer = 0;
                    int deerAlive = 0;
                    for (int i = 0; i < animalController.deer.Length; i++)
                    {
                        Animal deer = animalController.deer[i].GetComponent<Animal>();
                        if (!deer.dead)
                        {
                            deerAlive++;
                        }
                    }
                    if (deerAlive > 2)
                    {                       
                        level = 9;
                    }
                    else
                    {
                        level = 8;
                    }
                }
                break;

            case 8:
                water.addRain = false;
                text.text = @"Game Over. 
The ecosystem has collapsed due to imbalance. 
The once vibrant habitats lie submerged, 
the animals have vanished.";
                break;

            case 9:
                water.addRain = false;
                text.text = @"Congratulations! 
Through your careful management, 
you've restored balance to the ecosystem. 
The animals thrive once more, 
ensuring the continued harmony of this digital world.";
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
