using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using ProceduralGen;
using BossKey;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [TestFixture]
    public class TestBossKey
    {
        public static string RESOURCEPATH = "/Resources/";
        public static string GENERATETESTFILENAME = "writeresourcefiletest";
        public static string LEVELASSERTFILENAME = "leveltest_assert";

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void TestNewGraph()
        {
            BossKeyGraph newGraph = new BossKeyGraph(999);
            Debug.Log("end test");
            Assert.IsFalse(false);
        }

    }
}
