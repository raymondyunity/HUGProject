using System;
using ProceduralGen;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HUGUtility;
using UnityEditor.Experimental.UIElements;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TileLevel currentLevel;
    public Tile playerTile;
    public Tile exitTile;
    public int seed;
    public List<GameObject> TileQuadrants;
    public GameObject[,] tileObjects = new GameObject[16,16];
    public List<GameObject> testacross;
    public List<GameObject> testdown;
    
    void Start()
    {
        //create level
        currentLevel = new TileLevel(seed);
        FileOperation.WriteResourceFile("newlevel", currentLevel.LevelData);
        
        //setup scene with level
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            //get gameobject of section
            //TODO: find by tag, name sucks
            GameObject currentSection = GameObject.Find("TileQuarter" + quadrant.ToString());

            GameObject instanceOfSection = Instantiate(currentLevel.levelQuadrants[quadrant]);
            instanceOfSection.transform.SetParent(currentSection.transform);
            instanceOfSection.transform.localPosition = (new Vector3(-8, 0, 8));
            
            if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Up)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 0f, 0); 
            }
            else if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Right)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 90f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z - 2);
            }
            else if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Down)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, 180f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x-2, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z - 2);
            }
            else if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Left)
            {
                currentSection.transform.localEulerAngles = new Vector3(0, -90f, 0);
                currentSection.transform.localPosition = new Vector3(currentSection.transform.localPosition.x - 2, currentSection.transform.localPosition.y, currentSection.transform.localPosition.z);
            }
            //get list of quads
            Transform[] childrenQuads = currentSection.transform.GetComponentsInChildren<Transform>();
            
            foreach (Transform child in childrenQuads)
            {
                if (child.name.Contains("Quad"))
                {
                    //fragile code here that parses gameobject name to reference in array
                    string[] splitName = child.name.Split('_'); 
                    int oldx = Int32.Parse(splitName[0][splitName[0].Length-1].ToString());
                    int oldy = Int32.Parse(splitName[1][0].ToString());

                    int x=oldx;
                    int y=oldy;

                    //find transform depending on quadrant and direction 
                    if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Right)
                    {
                        x = oldy;
                        y = Math.Abs(oldx - 7);
                    }
                    else if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Down)
                    {
                        x = Math.Abs(oldx - 7);
                        y = Math.Abs(oldy - 7);
                    }
                    else if (currentLevel.tileSections[quadrant].Orientation == TileSection.Rotation.Left)
                    {
                        x = Math.Abs(oldy - 7);
                        y = oldx;
                    }
                    
                    if (quadrant == 1)
                    {
                        y += 8;
                    }
                    else if (quadrant == 2)
                    {
                        x += 8;
                    }
                    else if (quadrant == 3)
                    {
                        x += 8;
                        y += 8;
                    }
                    //TODO: tiles cannot be disabled or array will be populated with nulls
                    tileObjects[x, y] = child.gameObject;                    
                }
                    
            }
            
            TileQuadrants.Add(instanceOfSection);  

        }        
        
        //Build level
        PlacePlayerSpawnPoint();
        PlaceExitPoint();
        
        //save
        SaveLevel(currentLevel, true);
            
        ////DEBUG CODE////
        //try solving
        IsValidPath(playerTile, exitTile, true);
        
        //mark player spawn
        GameObject playerTileObject = tileObjects[playerTile.Coordinates.X, playerTile.Coordinates.Y];
        
        for (int i = 0; i < 16; i++)
        {
            testacross.Add(tileObjects[0, i]);
        }
        
        for (int i = 0; i < 16; i++)
        {
            testdown.Add(tileObjects[i, 0]);
        }
       
        GameObject playerSpawnMarkerObject = Instantiate((GameObject) Resources.Load("PlayerSpawnMarkerPrefab", typeof(GameObject)));
        //set pos
        playerSpawnMarkerObject.transform.position = playerTileObject.transform.position;
        
        //mark exit
        GameObject exitTileObject = tileObjects[exitTile.Coordinates.X, exitTile.Coordinates.Y];
        GameObject exitMarkerObject = Instantiate((GameObject) Resources.Load("ExitMarkerPrefab", typeof(GameObject)));
        //set pos
        exitMarkerObject.transform.position = exitTileObject.transform.position;
        ////END DEBUG CODE////
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
