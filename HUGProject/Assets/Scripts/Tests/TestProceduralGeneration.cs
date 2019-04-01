using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using ProceduralGen;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [TestFixture]
    public class TestProceduralGeneration
    {
        public static string RESOURCEPATH = "/Resources/";
        public static string GENERATETESTFILENAME = "writeresourcefiletest";
        public static string LEVELASSERTFILENAME = "leveltest_assert";

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(Application.dataPath + RESOURCEPATH + GENERATETESTFILENAME))
            {
                File.Delete(Application.dataPath + RESOURCEPATH + GENERATETESTFILENAME);
            }
        }

        [Test]
        public void TestReadResourceFile()
        {
            char[,] tilesection2data;
            string FILENAME = "tilesectiontest";
 
            tilesection2data = FileOperation.ReadTileSectionFile(FILENAME);
            char [,] readArray = UtilityRead(FILENAME, 8, 8);
            bool isDifferent = false;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (tilesection2data[i,j] != readArray[i, j])
                    {
                        isDifferent = true;
                        Assert.Fail("Different values :" + tilesection2data[i, j] + " " + readArray[i, j]);
                    }
                }
            }

            Assert.IsFalse(isDifferent);        
        }

        [Test]
        public void TestWriteResourceFile()
        {
            string FILENAME = "tilesectiontest";
            bool isDifferent = false;

            char[,] readArray = UtilityRead(FILENAME, 8, 8);
            FileOperation.WriteResourceFile(GENERATETESTFILENAME, readArray);
            char[,] readTestArray = UtilityRead(GENERATETESTFILENAME, 8, 8);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (readTestArray[i, j] != readArray[i, j])
                    {
                        isDifferent = true;
                        Assert.Fail("Different values :" + readTestArray[i, j] + " " + readArray[i, j]);
                    }
                }
            }

            Assert.IsFalse(isDifferent);
        }

        [Test]
        public void TestGenerateLevel()
        {
            string FILENAME = "tilesectiontest";
            bool isDifferent = false;

            char [,] tilesectiondata = FileOperation.ReadTileSectionFile(FILENAME);

            TileSection tile1 = new TileSection(tilesectiondata);
            TileSection tile2 = new TileSection(tilesectiondata);
            TileSection tile3 = new TileSection(tilesectiondata);
            TileSection tile4 = new TileSection(tilesectiondata);

            tile2.Orientation = TileSection.Rotation.Right;
            tile3.Orientation = TileSection.Rotation.Down;
            tile4.Orientation = TileSection.Rotation.Left;

            TileSection[] levelsections = new TileSection[4];
            levelsections[0] = tile1;
            levelsections[1] = tile2;
            levelsections[2] = tile3;
            levelsections[3] = tile4;

            char[,] leveldataarray = GenerationOperation.GenerateLevel(levelsections);
            char[,] readAssertArray = UtilityRead(LEVELASSERTFILENAME, 16, 16);

            //assert level
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (leveldataarray[i, j] != readAssertArray[i, j])
                    {
                        isDifferent = true;
                        Assert.Fail("Different values :" + leveldataarray[i, j] + " " + readAssertArray[i, j]);
                    }
                }
            }
            Assert.IsFalse(isDifferent);

        }

        [Test]
        public void TestGenerateRandomResultRepeated()
        {
            int first = GenerationOperation.GenerateRandomResult(10, 123456789);
            int second = GenerationOperation.GenerateRandomResult(10, 123456789);
            int third = GenerationOperation.GenerateRandomResult(10, 123456789);
            Debug.Log(first);
            Debug.Log(second);
            Debug.Log(third);
            Assert.AreEqual(first, second, "AreEqual: " + first + " equals " + second);
            Assert.AreEqual(second, third, "AreEqual: " + second + " equals " + third);
        }

        public char[,] UtilityRead(string filename, int x, int y)
        {
            string[] lines = File.ReadAllLines(Application.dataPath + RESOURCEPATH + filename + ".txt", Encoding.UTF8);
            char[,] assertarray = new char[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    assertarray[i, j] = lines[i][j];
                }
            }
            return assertarray;
        }

    }
}
