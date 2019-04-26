using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralGen;

namespace BossKey
{
    public enum BossKeyType{
        GenericKey,
        GenericDoor,
        SpecialKey,
        SpecialDoor,
        Start,
        Finish
    }

    public enum BossKeySpecialType
    {
        Red, Blue, Yellow, Green, None
    }

    public class BossKeyNode
    {
        public List<BossKeyNode> nodes;
        public BossKeyType nodeType;
        public BossKeyNode parent;
        public BossKeySpecialType specialType;

        public BossKeyNode(BossKeyType type)
        {
            nodeType = type;
            nodes = new List<BossKeyNode>();
            parent = null;
            specialType = BossKeySpecialType.None;
        }

        public bool isSpecial()
        {
            if (specialType == BossKeySpecialType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void AddChild(BossKeyNode node)
        {
            nodes.Add(node);
            node.parent = this;
        }

        public new BossKeyType GetType()
        {
            return this.nodeType;
        }

        public void RemoveLeaf()
        {
            if (nodes.Count == 0)
            {
                //update parent
                if (this.parent != null)
                {
                    this.parent.nodes.Remove(this);
                }
                else
                {
                    throw new System.Exception("Node has no parent.");
                }

                this.parent = null;
            }
            else
            {
                throw new System.Exception("Node is not a leaf.");
            }
        }
    }

    public class BossKeyGraph
    {
        bool finishAtFinalDepthConstraint;
        bool noExtraGenericKeysConstraint;
        bool noConsecutiveSpecialDoorsConstraint;

        int depth;
        int pathPerNode;
        int backtrack;
        int totalPaths;

        int genericKeysMax;
        int genericDoorsMax;
        int specialKeysMax;
        int specialDoorsMax;
        int totalPathsMax;

        int genericKeys;
        int genericDoors;
        int specialKeys;
        int specialDoors;

        int spawnedGenericKeys;
        int spawnedGenericDoors;
        int spawnedSpecialKeys;
        int spawnedSpecialDoors;

        int[] specialKeyArray;
        int[] backtrackKeyArray;

        int seed = 123;

        BossKeyNode root;

        Stack<BossKeyNode> spawnStack;

        public BossKeyGraph()
        {
            root = new BossKeyNode(BossKeyType.Start);
            specialKeyArray = new int[4];
            backtrackKeyArray = new int[4];     
            Setup();       
            Generate();
        }

        public void Setup()
        {
            spawnStack = new Stack<BossKeyNode>();
            spawnStack.Push(root);

            genericKeysMax = 3;
            genericDoorsMax = 3;
            specialKeysMax = 2;
            specialDoorsMax = 2;

            genericKeys = GenerationOperation.GenerateRandomResult(genericKeysMax, seed, 1);
            genericDoors = GenerationOperation.GenerateRandomResult(genericKeys, seed, 2);
            specialKeys = GenerationOperation.GenerateRandomResult(specialKeysMax, seed, 1);
            specialDoors = GenerationOperation.GenerateRandomResult(specialKeys, seed, 2);

            totalPathsMax = 10;
            pathPerNode = 3;
            backtrack = 0;
            depth = 5;

            finishAtFinalDepthConstraint = true;
            noExtraGenericKeysConstraint = true;
            noConsecutiveSpecialDoorsConstraint = true;
        }

        public BossKeyGraph Generate()
        {
            int currDepth = 0;
            totalPaths = 0;
            int saltCounter = 0;
            while ((currDepth < depth))
            {
                if (spawnStack.Count == 0)
                {
                    break;
                }
                saltCounter++;
                BossKeyNode poppedNode = spawnStack.Pop();
                currDepth++;

                //pick num of paths
                int numPaths = GenerationOperation.GenerateRandomResult(pathPerNode, seed, saltCounter);
                bool floorDoor = false;
                       
                //hardcode start path must have at least one path
                if ((numPaths == 0) && (spawnStack.Count == 0))
                {
                    numPaths = 1;
                }


                for (int i = 0; i < numPaths; i++)
                {
                    //pick a node type
                    BossKeyType newType = (BossKeyType) GenerationOperation.GenerateRandomResultMinMax(0, 3, seed, i);

                    //create new node
                    BossKeyNode newNode = new BossKeyNode(newType);

                    //hardcode if no generic key yet, spawn a key then door
                    if (i == (numPaths - 1))
                    {
                        if ((spawnedGenericKeys == 0) && (spawnedSpecialKeys == 0))
                        {
                            newNode.nodeType = BossKeyType.GenericKey;
                        }
                        else if ((spawnedGenericDoors == 0) && (spawnedSpecialDoors == 0))
                        {
                            newNode.nodeType = BossKeyType.GenericDoor;
                        }
                        else if ((spawnedSpecialKeys > 0) && (spawnedGenericKeys == 0))
                        {
                            newNode.nodeType = BossKeyType.SpecialDoor;
                        }

                        if (!floorDoor && i>0)
                        {
                            if (spawnedSpecialKeys > 0)
                            {
                                newNode.nodeType = BossKeyType.SpecialDoor;
                            }
                            else if (spawnedGenericKeys > 0)
                            {
                                newNode.nodeType = BossKeyType.GenericDoor;
                            }
                            
                        }

                    }

                    if (noConsecutiveSpecialDoorsConstraint)
                    {
                        if (poppedNode.nodeType == BossKeyType.SpecialDoor)
                        {
                            //downgrade to generic door if possible
                            if (spawnedSpecialKeys > 0)
                            {
                                newNode.nodeType = BossKeyType.GenericDoor;
                            }
                            else
                            {
                                newNode.nodeType = BossKeyType.GenericKey;
                            }
                        }
                    }

                    //if final depth and door, downgrade to genericKey
                    if ((currDepth == depth) && ((newNode.nodeType == BossKeyType.GenericDoor) || (newNode.nodeType == BossKeyType.SpecialDoor)))
                    {
                        newNode.nodeType = BossKeyType.GenericKey;
                    }

                    if (newNode.nodeType == BossKeyType.SpecialDoor)
                    {                        
                        //check if special door is valid otherwise change type
                        if (spawnedSpecialKeys == 0)
                        {
                            //spawn a specialkey
                            newNode.nodeType = BossKeyType.SpecialKey;
                            newNode.specialType = (BossKeySpecialType)GenerationOperation.GenerateRandomResult(3, seed, i);
                            specialKeyArray[(int)newNode.specialType]++;
                            backtrackKeyArray[(int)newNode.specialType]++;
                            spawnedSpecialKeys++;
                        }
                        else
                        {
                            //spawn a door
                            if (backtrack == 0)
                            {

                                List<BossKeySpecialType> potentialSpecialKeys = new List<BossKeySpecialType>();
                                //find possible special keys
                                for (int j = 0; j < 4; j++)
                                {
                                    if (specialKeyArray[j] > 0)
                                    {
                                        potentialSpecialKeys.Add((BossKeySpecialType)j);
                                    }
                                }
                                //if no special key was generated then we just downgrade
                                if (potentialSpecialKeys.Count == 0)
                                {
                                    //create special key
                                    newNode.nodeType = BossKeyType.SpecialKey;
                                    newNode.specialType = (BossKeySpecialType)GenerationOperation.GenerateRandomResult(3, seed, i);
                                    specialKeyArray[(int)newNode.specialType]++;
                                    backtrackKeyArray[(int)newNode.specialType]++;
                                    spawnedSpecialKeys++;
                                    break;
                                }

                                //random pick a key colour
                                BossKeySpecialType randomSpecialKey = potentialSpecialKeys[GenerationOperation.GenerateRandomResult(potentialSpecialKeys.Count-1, seed, i)];
                                //set door colour
                                newNode.specialType = randomSpecialKey;
                                spawnedSpecialDoors++;
                                floorDoor = true;
                            }

                        }
                        
                    }
                    else if (newNode.nodeType == BossKeyType.GenericDoor)
                    {
                        newNode.specialType = BossKeySpecialType.None;

                        if (spawnedGenericKeys == 0)
                        {
                            //downgrade to key
                            newNode.nodeType = BossKeyType.GenericKey;
                            spawnedGenericKeys++;
                            break;
                        }

                        spawnedGenericDoors++;
                        spawnedGenericKeys--;
                        floorDoor = true;
                    }
                    else if (newNode.nodeType == BossKeyType.SpecialKey)
                    {
                        //random pick a key colour
                        BossKeySpecialType randomSpecialType = (BossKeySpecialType)GenerationOperation.GenerateRandomResult(3, seed, i);
                        //set key colour
                        newNode.specialType = randomSpecialType;

                        //if second key then we convert to door
                        if (specialKeyArray[(int)newNode.specialType] > 0)
                        {
                            newNode.nodeType = BossKeyType.SpecialDoor;
                            spawnedSpecialDoors++;
                        }
                        else
                        {
                            spawnedSpecialKeys++;
                        }  
                    }
                    else if (newNode.nodeType == BossKeyType.GenericKey)
                    {
                        newNode.specialType = BossKeySpecialType.None;
                        spawnedGenericKeys++;
                    }

                    poppedNode.AddChild(newNode);
                    spawnStack.Push(newNode);
                }

            }

            //push fin, add to deepest node on stack
            if (spawnStack.Count > 0)
            {
                BossKeyNode finishNode = new BossKeyNode(BossKeyType.Finish);

                if (finishAtFinalDepthConstraint)
                {
                    BossKeyNode[] nodeArray = spawnStack.ToArray();
                    BossKeyNode greatestDepthNode = nodeArray[0];
                    int greatestDepth = 0;
                    bool isValidGreatestDepthFound = false;
                    //find deepest node
                    for (int i =0;i<nodeArray.Length; i++)
                    {
                        int parentCount = 0;
                        BossKeyNode mynode = nodeArray[i];
                        while (mynode.parent != null)
                        {
                            parentCount++;
                            mynode = mynode.parent;
                        }
                        //Greatest depth node that is a door and not past max depth
                        if ((parentCount <= depth-1) && (greatestDepth <= parentCount) && ((mynode.nodeType == BossKeyType.SpecialDoor) || (mynode.nodeType == BossKeyType.GenericDoor)))
                        {
                            greatestDepthNode = nodeArray[i];
                            greatestDepth = parentCount;
                            isValidGreatestDepthFound = true;
                        }
                    }
                    if (!isValidGreatestDepthFound)
                    {
                        greatestDepthNode = greatestDepthNode.parent;
                    }
                    greatestDepthNode.AddChild(finishNode);
                }
                else
                {
                    Stack<BossKeyNode> selectStack = new Stack<BossKeyNode>();
                    foreach (BossKeyNode node in spawnStack)
                    {
                        BossKeyNode tmpnode;
                        tmpnode = node;
                        int parentCount = 0;
                        while (tmpnode.parent != null)
                        {
                            parentCount++;
                            tmpnode = tmpnode.parent;
                        }

                        if (parentCount >= depth)
                        {
                            selectStack.Push(node);
                        }
                    }
                    BossKeyNode randomNodePick = selectStack.ToArray()[GenerationOperation.GenerateRandomResult(spawnStack.Count, seed, saltCounter)];
                    randomNodePick.AddChild(finishNode);
                }

                if (noExtraGenericKeysConstraint)
                {
                    while ((spawnStack.Count > 0) && (spawnedGenericKeys > 0))
                    {
                        BossKeyNode node = spawnStack.Pop();
                        if (node.nodeType == BossKeyType.GenericKey)
                        {
                            node.RemoveLeaf();
                        }
                        spawnedGenericKeys--;
                    }
                }
            }
            else
            {
                Debug.Log("Shouldn't be possible");
            }


            return this;
        }

        public BossKeyNode GetRoot()
        {
            return root;
        }
    }


}
