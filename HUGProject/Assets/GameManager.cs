using ProceduralGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int gameSeed;

    // Start is called before the first frame update
    void Start()
    {
        //generate seed number
        Random rnd = new Random();
        this.gameSeed = Random.Range(0, 1000000);

        //initialize LevelManager
        LevelManager lvlman = this.gameObject.GetComponent<LevelManager>();
        lvlman.seed = gameSeed;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
