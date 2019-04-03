using ProceduralGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TileLevel currentLevel;
    public int seed;

    void Start()
    {
        //create level
        currentLevel = new TileLevel(seed);
        FileOperation.WriteResourceFile("newlevel", currentLevel.LevelData, true);

        //setup scene with level
        for (int i = 0; i < 4; i++)
        {
            //get gameobject of section
            GameObject currentSection = GameObject.Find("TileSection" + i.ToString());

            GameObject instanceOfSection = Instantiate(currentLevel.levelQuadrants[i]);
            instanceOfSection.transform.SetParent(currentSection.transform);
            instanceOfSection.transform.localPosition = (new Vector3(-8, 0, 8));
            
            if (currentLevel.tileSections[i].Orientation == TileSection.Rotation.Up)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 0f, 0);
            }
            else if (currentLevel.tileSections[i].Orientation == TileSection.Rotation.Right)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 90f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z - 2);
            }
            else if (currentLevel.tileSections[i].Orientation == TileSection.Rotation.Down)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 180f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x-2, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z - 2);
            }
            else if (currentLevel.tileSections[i].Orientation == TileSection.Rotation.Left)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, -90f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x - 2, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z);
            }
        }

    }

    void Update()
    {
        
    }
}
