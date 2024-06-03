using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserTest : MonoBehaviour
{
    public RectTransform textTransform;
    public AudioSource audioSource;
    public AudioClip[] ttsClips;
    public AudioSource waterAmbiance;
    public TMP_Text text;
    public Image image;
    public TerrainGen terrain;
    public Water water;
    bool drag = false;
    int level = -1;
    float rainPercentage = 0;
    float landWaterRatio = 0;
    float timer = 0;
    public AnimalController animalController;
    bool audioPlayed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(level)
        {
            case -1:
                water.rainEnabled = true;
                text.text = @"Welcome to AR Sandbox Symbiosis game!";
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    water.clearWater = true;
                    water.WaterCS.SetBool("clearWater", water.clearWater);
                    level++;
                    audioSource.clip = ttsClips[0];
                    audioSource.Play();
                }
                break;
            case 0:

                water.rainEnabled = false;
                text.text = @"Once upon a time, in a augmented sandbox world,
an intricate ecosystem thrive where 
every grain of sand represented life.

The sand formed the ground, where tiny animals roamed. 
Below this threshold, the sand transformed
into shimmering ponds,
teeming with colorful fish. 

In this AR Sandbox Symbiosis game, you have 
to create a habitat balance between fish and animals.

The delicate existence of life in the AR Sandbox 
depended on the balance between the land and water.";
                timer += Time.deltaTime;
                if (timer > 25)
                {
                    timer = 0;
                    level++;
                    audioSource.clip = ttsClips[1];
                    audioSource.Play();
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                    audioSource.clip = ttsClips[1];
                    audioSource.Play();
                }
                break;

            case 1:

                water.rainEnabled = false;
                text.text = @"Ecosystem imbalance detected! 
                
Restore harmony before irreversible consequences occur.

   - Hint: More fish will thrive on medium blue sea level 
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
                    audioSource.clip = ttsClips[2];
                    audioSource.Play();
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                    audioSource.clip = ttsClips[2];
                    audioSource.Play();
                }
                break;

            case 2:
                water.rainEnabled = false;
                text.text = @"Congratulations! 
The balance of life is achieved. 
You are now at the next level";
                timer += Time.deltaTime;
                if (timer > 5)
                {
                    timer = 0;
                    level++;
                    audioSource.clip = ttsClips[3];
                    audioSource.Play();
                }
                break;
            case 3:

                water.rainEnabled = true;
                text.text = @"The fish and animals need rain 
to grow more in number.
   - Add rain to using your hand to achieve that.";

                if (rainPercentage > 0.4)
                {
                    level++;
                    audioSource.clip = ttsClips[4];
                    audioSource.Play();
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    level++;
                    audioSource.clip = ttsClips[4];
                    audioSource.Play();
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
                    audioSource.clip = ttsClips[5];
                    audioSource.Play();
                }
                break;

            case 5:
                image.color = new Color(1, 0, 0, 1);
                text.text = @"A storm is currently in the habitat!";
                timer += Time.deltaTime;
                if (timer > 3)
                {
                    timer = 0;
                    level++;
                    audioSource.clip = ttsClips[6];
                    audioSource.Play();
                }
                break;

            case 6:
                text.text = @"Ground depletion detected! 
Animal populations are dwindling due to excessive rain. 

   - The habitat has more than enough fish in the sea.
   - You have 30 seconds to keep three ground animals.";
                timer += Time.deltaTime;
                if (timer > 13)
                {
                    timer = 0;
                    level++;
                    audioSource.clip = ttsClips[7];
                    audioSource.Play();
                }
                break;

            case 7:
                animalController.deerUpperLimit = 1.0f;
                water.addRain = true;
                timer += Time.deltaTime;
                int time = (int)timer;
                text.text = @"You need to raise the ground to save the animals.
                
   - Time remaining: " + (30 - time) + " seconds.";
                if (timer > 20)
                {
                    if (!audioPlayed)
                    {
                        audioSource.clip = ttsClips[8];
                        audioSource.Play();
                        audioPlayed = true;
                    }
                    text.text = @"Emergency: Ecosystem critical! 
Imbalance between water and land 
is causing widespread devastation. 
Immediate intervention required 
to prevent total collapse of the ecosystem
   - Time remaining: " + (30 - time) + " seconds.";
                }
                if (timer > 30)
                {
                    audioPlayed = false;
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
                        audioSource.clip = ttsClips[9];
                        audioSource.Play();
                    }
                    else
                    {
                        level = 8;
                        audioSource.clip = ttsClips[10];
                        audioSource.Play();
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
                image.color = new Color(0, 0.8f, 0, 1);
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
            if (depth > 1f)
            {
                depthSum += 1;
            }
        }
        rainPercentage = depthSum / water.vertices.Length;
        //

        waterAmbiance.volume = rainPercentage;

        if (!waterAmbiance.isPlaying)
        {
            waterAmbiance.Play();
        }



        // if (drag)
        // {
        //     textTransform.position = Input.mousePosition;
        //     if (Input.mouseScrollDelta.y > 0)
        //     {
        //         if (Input.GetKey(KeyCode.LeftAlt))
        //         {
        //             text.fontSize += 1;
        //         }
        //         else
        //         {
        //             textTransform.eulerAngles += new Vector3(0, 0, 10);

        //         }
        //     }
        //     else if (Input.mouseScrollDelta.y < 0)
        //     {
        //         if (Input.GetKey(KeyCode.LeftAlt))
        //         {
        //             text.fontSize -= 1;
        //         }
        //         else
        //         {
        //             textTransform.eulerAngles -= new Vector3(0, 0, 10);
        //         }
        //     }
        // }
    }

    // public void TextClick()
    // {
    //     drag = true;
    // }

    // public void TextRelease()
    // {
    //     drag = false;
    // }
}
