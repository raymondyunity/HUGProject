using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using ProceduralGen;

namespace HUGUtility
{
    public class Coords
    {
        private int x;
        private int y;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        
        public Coords (int xin=0, int yin=0)
        {
            x = xin;
            y = yin;
        }

    }

    public class Solver
    {
        private char[,] map;
        private bool[,] visitedMap;
        private bool[,] solution;
        private int stepsAnswer;
        private int steps;
        private char startTile;
        private char goalTile;
        private const int XMAX = 16;
        private const int YMAX = 16;

        public bool[,] Solution => solution;

        public bool[,] VisitedMap => visitedMap;

        public int StepsAnswer => stepsAnswer;

        public Solver(char [,] mapToSolve, char startTile, char goalTile)
        {
            stepsAnswer = 0;
            map = mapToSolve;
            this.startTile = startTile;
            this.goalTile = goalTile;
            //reset visited bool array
            visitedMap = new bool[16,16];
        }
        
        public bool SolvePath(int x, int y, bool[,] visited)
        { 
            if ((x < 0) || (y < 0))
                return false;
            
            if ((x >= XMAX) || (y >= YMAX))
                return false;

            if (visited[x, y] == true)
                return false;
            
            //if solid then return
            if (Tile.IsTileTypeSolid(map[x, y]))
                return false;

            
            if (map[x, y] == goalTile)
            {
                stepsAnswer = steps;
                //we found it
                return true;
            }

            visited[x, y] = true;
            steps++;
            if (SolvePath(x + 1, y, visited))
                return true;
            if (SolvePath(x - 1, y, visited))
                return true;
            if (SolvePath(x, y + 1, visited))
                return true;
            if (SolvePath(x, y - 1, visited))
                return true;

            return false;

        }
    }

}


