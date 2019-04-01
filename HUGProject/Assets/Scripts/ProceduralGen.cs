using System.IO;
using System.Text;
using UnityEngine;

namespace ProceduralGen
{
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

        public TileLevel(int seed)
        {
            //naive level generation for now
            //TODO: proper level generation
            levelQuadrants = new GameObject[4];
            tileSections = new TileSection[4];
            string SECTIONNAME = "TileSection1";
            char[,] tilesectiondata = FileOperation.ReadTileSectionFile(SECTIONNAME);

            for (int i = 0; i < 4; i++)
            {
                tileSections[i] = new TileSection(tilesectiondata);
                levelQuadrants[i] = (GameObject) Resources.Load(SECTIONNAME + "_prefab", typeof(GameObject));
                tileSections[i].Orientation = (TileSection.Rotation)GenerationOperation.GenerateRandomResult(4, seed, i);
            }

            LevelData = GenerationOperation.GenerateLevel(tileSections);

            //TODO:write to file and assign to leveldatafile
        }

        public TextAsset LevelDataFile { get => levelDataFile; set => levelDataFile = value; }
        public char[,] LevelData { get => levelData; set => levelData = value; }
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
