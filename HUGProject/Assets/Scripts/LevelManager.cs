using ProceduralGen;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HUGUtility;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TileLevel currentLevel;
    public Tile playerTile;
    public Tile exitTile;
    public int seed;
    public List<GameObject> TileQuadrants;

    void Start()
    {
        //create level
        currentLevel = new TileLevel(seed);
        FileOperation.WriteResourceFile("newlevel", currentLevel.LevelData);

        //Build level
        PlacePlayerSpawnPoint();
        PlaceExitPoint();
        
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
            
            TileQuadrants.Add(instanceOfSection);  

        }        
        
        
        SaveLevel(currentLevel, true);
            
        //try solving
        IsValidPath(playerTile, exitTile, true);

    }

    void Update()
    {
        
    }
    
    
    //function to add player spawn point
    public void PlacePlayerSpawnPoint()
    {
        List<Tile> listOfSpawnPoints = new List<Tile>();
        //find valid floor
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                //put coords into list
                if (Tile.IsTileTypeFloor(currentLevel.LevelData[i,j]))
                {
                    listOfSpawnPoints.Add(new Tile(currentLevel.LevelData[i,j], i, j));
                }
                
            }
        }
        
        //pick with seed from the list
        int listChoice = GenerationOperation.GenerateRandomResult(listOfSpawnPoints.Count, seed, 123);
        Tile tileChoice = listOfSpawnPoints[listChoice];
        
        //set player tile
        tileChoice.Data = 'P';
        playerTile = tileChoice;
        
        //replace char data and save
        currentLevel.LevelData[tileChoice.Coordinates.X, tileChoice.Coordinates.Y] = 'P';
        SaveLevel(currentLevel);
    }
    
    //function to add exit point
    public void PlaceExitPoint()
    {
        List<Tile> listOfSpawnPoints = new List<Tile>();
        //find valid floor
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                //put coords into list
                if (Tile.IsTileTypeFloor(currentLevel.LevelData[i,j]))
                {
                    Tile selectTile = new Tile(currentLevel.LevelData[i, j], i, j);
                    if (!selectTile.IsSameQuadrantAs((playerTile)))
                    {
                        listOfSpawnPoints.Add(selectTile);
                    }
                    
                }
                
            }
        }
        
        //pick with seed from the list
        int listChoice = GenerationOperation.GenerateRandomResult(listOfSpawnPoints.Count, seed, 567);
        Tile tileChoice = listOfSpawnPoints[listChoice];
        //set exit tile
        tileChoice.Data = 'E';
        exitTile = tileChoice;
        //replace char data and save
        currentLevel.LevelData[tileChoice.Coordinates.X, tileChoice.Coordinates.Y] = 'E';
        SaveLevel(currentLevel);
    }
    
    public Solver IsValidPath(Tile start, Tile finish, bool debugOutput = false)
    {
        Solver solve = new Solver(currentLevel.LevelData, start.Data, finish.Data);
        bool isValid = solve.SolvePath(start.Coordinates.X, start.Coordinates.Y, solve.VisitedMap);
        
        if (debugOutput)
        {
            Debug.Log("Steps: " + solve.StepsAnswer);
        }
        return solve;
    }
    
    //save level
    public TileLevel SaveLevel(TileLevel levelToSave, bool debugOutput=false)
    {
        currentLevel = levelToSave;
        FileOperation.WriteResourceFile(levelToSave.levelDataFile.name, levelToSave.LevelData, debugOutput);
        return currentLevel;
    }
    
}
