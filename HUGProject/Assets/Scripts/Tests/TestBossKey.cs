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
        public string outputString = "";
        public int outputCounter = 0;

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void TestNewGraph()
        {
            BossKeyGraph newGraph = new BossKeyGraph(7463423);
            Debug.Log("end test");
            //start it
            outputString = "switch(start) {";
            GenOutput(newGraph.GetRoot().nodes);
            Debug.Log(outputString);
            Assert.IsFalse(false);
        }

        public void GenOutput(List<BossKeyNode> outputnodelist)
        {            
            //if leaf
            if ((outputnodelist.Count == 0) || (outputnodelist == null))
            {
                outputString = outputString + "}";
                return;
            }
            foreach (BossKeyNode node in outputnodelist)
            {
                outputCounter++;
                string colour = "";
                if (node.specialType != BossKeySpecialType.None)
                {
                    colour = node.specialType.ToString();
                }
                string mystring = outputCounter.ToString() + " => switch(" + colour + node.nodeType.ToString() + outputCounter.ToString() + "){";
                outputString = outputString + mystring;
                GenOutput(node.nodes);
            }
            outputString = outputString + "}";
        }

    }
}
