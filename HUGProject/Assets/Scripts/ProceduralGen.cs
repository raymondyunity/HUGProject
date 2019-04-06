using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using HUGUtility;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;
using Random = UnityEngine.Random;

namespace ProceduralGen
{
    public class Tile
    {
        public Tile(char c, int x, int y)
        {
            data = c;
            Coordinates = new Coords(x, y);

            if ((x < 4) && (y < 4))
            {
                Quadrant = 1;
            }
            else if ((x > 4) && (y < 4))
            {
                Quadrant = 2;
            }
            else if ((x < 4) && (y > 4))
            {
                Quadrant = 3;
            }
            else if ((x > 4) && (y > 4))
            {
                Quadrant = 4;
            }
        }
        public int Quadrant { get; set; }
        public Coords Coordinates { get; set; }
        private char data { get; set; }
        //wall, door, doorway, hole
        static char[] _solidTypes = {'w', 'W', 'D', 'd', 'o', 'O'};
        //floor
        static char[] _floorTypes = {'x', 'X'};
        //keys, chests
        static char[] _itemTypes = {'K', 'C'};
        
        public bool IsSameQuadrantAs(Tile t)
        {
            return (Quadrant == t.Quadrant);
        }

        public static bool IsTileTypeSolid(char t)
        {
            for (int i = 0; i < _solidTypes.Length; i++)
            {
                if (_solidTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool IsTileTypeFloor(char t)
        {
            for (int i = 0; i < _floorTypes.Length; i++)
            {
                if (_floorTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool IsTileTypeItem(char t)
        {
            for (int i = 0; i < _itemTypes.Length; i++)
            {
                if (_itemTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    public class TileSection
    {
        private char[,] sectiondata = new char[8, 8];
        public enum Rotation
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }
        GameObject prefabAsset;
        Rotation orientation;
        public TileSection(char[,] sectiondata)
        {
            this.Sectiondata = sectiondata;
            this.Orientation = TileSection.Rotation.Up;
        }

        public char[,] Sectiondata { get => sectiondata; set => sectiondata = value; }
        public GameObject PrefabAsset { get => prefabAsset; set => prefabAsset = value; }
        public Rotation Orientation { get => orientation; set => orientation = value; }
    }

    public class TileLevel
    {
        public GameObject[] levelQuadrants;
        public TileSection[] tileSections;
        private char[,] levelData;
        public TextAsset levelDataFile;
        public string levelName;

        public TileLevel(int seed)
        {
            //naive level generation for now
            //TODO: proper level generation
            levelQuadrants = new GameObject[4];
            tileSections = new TileSection[4];
            string SECTIONNAME = "TileSection1";
            levelName = "newlevel";
            char[,] tilesectiondata = FileOperation.ReadTileSectionFile(SECTIONNAME);

            for (int i = 0; i < 4; i++)
            {
                tileSections[i] = new TileSection(tilesectiondata);
                levelQuadrants[i] = (GameObject) Resources.Load(SECTIONNAME + "_prefab", typeof(GameObject));
                tileSections[i].Orientation = (TileSection.Rotation)GenerationOperation.GenerateRandomResult(4, seed, i);
            }

            LevelData = GenerationOperation.GenerateLevel(tileSections);

            FileOperation.WriteResourceFile(levelName, levelData);
            levelDataFile = Resources.Load(levelName) as TextAsset;
        }

        public TextAsset LevelDataFile { get => levelDataFile; set => levelDataFile = value; }
        public char[,] LevelData { get => levelData; set => levelData = value; }
        public string LevelName { get => levelName; set => levelName = value; }
    }

    public class GenerationOperation
    {
        public static int GenerateRandomResult(int enumRange, int seed, int salt=0)
        {
            Random rnd = new Random();
            Random.InitState(seed + salt);
            return Random.Range(0, enumRange);
        }

        public static char[,] GenerateLevel(TileSection[] sections)
        {
            char[,] leveldata = new char[16, 16];
            int quadrantindexmathX = 0;
            int quadrantindexmathY = 0;

            for (int quadrant = 0; quadrant < 4; quadrant++)
            {
                //quadrant math to find starting spot
                quadrantindexmathX = (quadrant >= 2) ? 8 : 0;
                quadrantindexmathY = ((quadrant == 1) || (quadrant == 3)) ? 8 : 0;

                //determine rotation
                if (sections[quadrant].Orientation == TileSection.Rotation.Up)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            leveldata[i + quadrantindexmathX, j + quadrantindexmathY] = sections[quadrant].Sectiondata[i, j];
                        }
                    }
                }
                else if (sections[quadrant].Orientation == TileSection.Rotation.Right)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            leveldata[i + quadrantindexmathX, j + quadrantindexmathY] = sections[quadrant].Sectiondata[7 - j, i];
                        }
                    }
                }
                else if (sections[quadrant].Orientation == TileSection.Rotation.Down)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            leveldata[i + quadrantindexmathX, j + quadrantindexmathY] = sections[quadrant].Sectiondata[7 - i, 7 - j];
                        }
                    }
                }
                else if (sections[quadrant].Orientation == TileSection.Rotation.Left)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            leveldata[i + quadrantindexmathX, j + quadrantindexmathY] = sections[quadrant].Sectiondata[j, 7 - i];
                        }
                    }
                }
            }

            return leveldata;
        }
    }
    
    public class FileOperation
    {
        public static char[,] ReadTileSectionFile(string filename)
        {
            char[,] ret = new char[8, 8];
            //has to be in a resources folder
            TextAsset testdata = Resources.Load(filename) as TextAsset;
            string text = testdata.text;
            string[] lines = text.Split("\n"[0]);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ret[i, j] = lines[i][j];
                }
            }
            return ret;
        }

        public static void WriteResourceFile(string filename, char[,] chararray, bool debugOutput = false)
        {
            using (StreamWriter streamWriter = File.CreateText(Application.dataPath + "/resources/" + filename + ".txt"))
            {
                for (int i = 0; i < chararray.GetLength(0); i++)
                {
                    for (int j = 0; j < chararray.GetLength(1); j++)
                    {
                        streamWriter.Write(chararray[i, j]);
                    }
                    streamWriter.Write("\n");
                }
            }
            if (debugOutput)
            {
                StringBuilder sb = new StringBuilder("", 256);
                for (int i = 0; i < chararray.GetLength(0); i++)
                {
                    for (int j = 0; j < chararray.GetLength(1); j++)
                    {
                        sb.Append(chararray[i, j]);
                    }
                    sb.Append("\n");
                }
                Debug.Log(sb.ToString());
            }
        }

       
    }



}
