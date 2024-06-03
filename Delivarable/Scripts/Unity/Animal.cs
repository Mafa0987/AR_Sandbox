using UnityEngine;

public class Animal : MonoBehaviour
{
    public string species;
    public float rotateVal;
    public float survivalTimer;
    public bool dead;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotateVal = 0;
        survivalTimer = 0;
        dead = false;
    }
}
