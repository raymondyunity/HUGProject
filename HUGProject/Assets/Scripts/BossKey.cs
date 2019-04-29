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
        public int depth;

        public BossKeyNode(BossKeyType type)
        {
            nodeType = type;
            nodes = new List<BossKeyNode>();
            parent = null;
            specialType = BossKeySpecialType.None;
            depth = 0;
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
            node.depth = this.depth + 1;
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
        bool noRepeatedSpecialDoorsConstraint;
        bool noRepeatedSpecialKeysConstraint;

        int depth;
        int pathPerNode;
        int backtrack;
        int totalPaths;

        int genericKeysMax;
        int genericDoorsMax;
        int specialKeysMax;
        int specialDoorsMax;
        int totalPathsMax;
        int backtrackDistanceMin;

        int genericKeys;
        int genericDoors;
        int specialKeys;
        int specialDoors;

        int spawnedGenericKeys;
        int spawnedGenericDoors;
        int spawnedSpecialKeys;
        int spawnedSpecialDoors;
        int doorsInStack;

        int[] specialKeyArray;
        int[] backtrackKeyArray;
        int[] specialDoorsArray;

        int seed;

        BossKeyNode root;

        Stack<BossKeyNode> spawnStack;

        public BossKeyGraph(int seed)
        {
            this.seed = seed;
            root = new BossKeyNode(BossKeyType.Start);
            specialKeyArray = new int[4];
            backtrackKeyArray = new int[4];
            specialDoorsArray = new int[4];
            Setup();       
            Generate();
        }

        public void Setup()
        {
            spawnStack = new Stack<BossKeyNode>();
            spawnStack.Push(root);
            doorsInStack = 0;

            genericKeysMax = 3;
            genericDoorsMax = 3;
            specialKeysMax = 2;
            specialDoorsMax = 2;
            //TODO: this needs to be checked for validity, FIX THIS
            backtrackDistanceMin = 2;

            genericKeys = GenerationOperation.GenerateRandomResult(genericKeysMax, seed, 1);
            genericDoors = GenerationOperation.GenerateRandomResult(genericKeys, seed, 2);
            specialKeys = GenerationOperation.GenerateRandomResult(specialKeysMax, seed, 1);
            specialDoors = GenerationOperation.GenerateRandomResult(specialKeys, seed, 2);

            totalPathsMax = 25;
            pathPerNode = 6;
            backtrack = 2;
            depth = 4;

            finishAtFinalDepthConstraint = true;
            noExtraGenericKeysConstraint = false;
            noConsecutiveSpecialDoorsConstraint = true;
            noRepeatedSpecialKeysConstraint = true;
            noRepeatedSpecialDoorsConstraint = true;
        }

        public BossKeyGraph Generate()
        {
            int currDepth = root.depth;
            totalPaths = 0;
            int saltCounter = 0;
            while ((currDepth < depth))
            {
                if (spawnStack.Count == 0)
                {
                    break;
                }
                if (totalPaths == totalPathsMax)
                {
                    break;
                }
                saltCounter++;
                BossKeyNode poppedNode = spawnStack.Pop();


                //PRIMARY CONSTRAINT
                //ignore key nodes past 1st level so we only continue from doors
                if (currDepth > 1)
                {
                    //should never get stuck** Primary constraint assures this
                    while ((poppedNode.nodeType == BossKeyType.SpecialKey) || (poppedNode.nodeType == BossKeyType.GenericKey))
                    {
                        poppedNode = spawnStack.Pop();
                    }
                }

                if ((poppedNode.nodeType == BossKeyType.GenericDoor) || (poppedNode.nodeType == BossKeyType.SpecialDoor))
                {
                    doorsInStack--;
                }

                if (backtrack > 0)
                {
                    //spawn a door but do not downgrade to key
                    //also make the current node a previous door on the stack and add the key there
                    if ((spawnStack.Count > 0) && (doorsInStack > 1))
                    {
                        BossKeyNode doorToSwitch = spawnStack.Pop();

                        //keep popping until door is found
                        while ((doorToSwitch.nodeType == BossKeyType.SpecialKey) || (doorToSwitch.nodeType == BossKeyType.GenericKey))
                        {
                            doorToSwitch = spawnStack.Pop();
                        }

                        if (doorToSwitch == null)
                        {
                            Debug.Log("This is impossible");
                        }
                        doorsInStack--;
                        BossKeyNode copyNode = new BossKeyNode(poppedNode.nodeType);
                        copyNode = poppedNode;
                        if ((copyNode.nodeType == BossKeyType.GenericDoor) || (copyNode.nodeType == BossKeyType.SpecialDoor))
                        {
                            doorsInStack++;
                        }
                        spawnStack.Push(copyNode);
                        //set current node
                        poppedNode = doorToSwitch;

                        //kill backtrack counter
                        backtrack--;
                    }
                }

                currDepth = poppedNode.depth;

                //pick num of paths
                int numPaths = GenerationOperation.GenerateRandomResult(pathPerNode, seed, saltCounter);
                bool floorDoor = false;

                //PRIMARY CONSTRAINT
                //hardcode start path must have at least 2 path
                if ((numPaths < 2) || (spawnStack.Count == 0))
                {
                    numPaths = 2;
                }
                //update total paths
                totalPaths += numPaths;
                for (int i = 0; i < numPaths; i++)
                {
                    //pick a node type
                    BossKeyType newType = (BossKeyType)GenerationOperation.GenerateRandomResultMinMax(0, 3, seed, i);

                    //create new node
                    BossKeyNode newNode = new BossKeyNode(newType);

                    //PRIMARY CONSTRAINT
                    //hardcode if no generic key yet, spawn a key then door
                    if ((i == (numPaths-1))) 
                    {
                        if ((spawnedGenericKeys > 0) && (spawnedGenericDoors == 0))
                        {
                            newNode.nodeType = BossKeyType.GenericDoor;
                        }
                        else if ((spawnedSpecialKeys > 0) && (spawnedSpecialDoors == 0))
                        {
                            newNode.nodeType = BossKeyType.SpecialDoor;
                        }
                        else if ((spawnedSpecialKeys == 0) && (spawnedSpecialDoors > 0) && (backtrack == 0) && (doorsInStack == 0))
                        {
                            newNode.nodeType = BossKeyType.SpecialKey;
                        }
                        else if ((spawnedGenericKeys == 0) && (spawnedGenericDoors > 0) && (backtrack==0) && (doorsInStack == 0))
                        {
                            newNode.nodeType = BossKeyType.GenericKey;
                        }

                        if (!floorDoor && i > 0)
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
                    //handle special doors
                    if (newNode.nodeType == BossKeyType.SpecialDoor)
                    {
                        //check if special door is valid otherwise change type
                        if ((spawnedSpecialKeys == 0) && (backtrack == 0))
                        {
                            newNode.nodeType = BossKeyType.SpecialKey;
                        }
                        //PRIMARY CONSTRAINT
                        //must spawn in more than 2 path and greater than the first level
                        else if ((numPaths < 3) && (currDepth < 1))
                        {
                            newNode.nodeType = BossKeyType.SpecialKey;
                        }
                        else
                        {
                            //spawn a door
                            List<BossKeySpecialType> potentialSpecialKeys = new List<BossKeySpecialType>();
                            //find possible special keys
                            for (int j = 0; j < 4; j++)
                            {
                                if (specialKeyArray[j] > 0)
                                {
                                    potentialSpecialKeys.Add((BossKeySpecialType)j);
                                }
                            }

                            //if list is empty then we have backtrack enabled and no such key exists for the door yet
                            //pick a random colour and add this colour to the list
                            if (potentialSpecialKeys.Count == 0)
                            {
                                BossKeySpecialType randomSpecialType = (BossKeySpecialType)GenerationOperation.GenerateRandomResult(3, seed, i);
                                newNode.specialType = randomSpecialType;
                            }
                            else
                            {
                                newNode.specialType = potentialSpecialKeys[GenerationOperation.GenerateRandomResult(potentialSpecialKeys.Count - 1, seed, i)];
                            }
    
                            if (backtrack == 0)//no backtrack
                            {
                                //if no special key was generated then we just downgrade
                                if (potentialSpecialKeys.Count == 0)
                                {
                                    //create special key
                                    newNode.nodeType = BossKeyType.SpecialKey;
                                }
                                else
                                {
                                    newNode.specialType = potentialSpecialKeys[GenerationOperation.GenerateRandomResult(potentialSpecialKeys.Count - 1, seed, i)];
                                }

                            }

                        }

                        if (newNode.nodeType == BossKeyType.SpecialDoor)
                        {
                            //if door colour is used again
                            if (noRepeatedSpecialDoorsConstraint)
                            {
                                if (specialDoorsArray[(int)newNode.specialType] > 0)
                                {
                                    //down convert to a door
                                    newNode.nodeType = BossKeyType.GenericDoor;
                                }
                                else
                                {
                                    spawnedSpecialDoors++;
                                    doorsInStack++;
                                    specialDoorsArray[(int)newNode.specialType]++;
                                    floorDoor = true;
                                }
                            }

                        }
                    }
                    //handle special keys
                    if (newNode.nodeType == BossKeyType.SpecialKey)
                    {
                        //PRIMARY CONSTRAINT
                        //must spawn in more than 2 path and greater than the first level
                        if ((numPaths < 3) && (currDepth < 1))
                        {
                            newNode.nodeType = BossKeyType.GenericKey;
                        }
                        else
                        {
                            List<BossKeySpecialType> potentialSpecialKeys = new List<BossKeySpecialType>();
                            //find possible special keys
                            for (int j = 0; j < 4; j++)
                            {
                                if (specialDoorsArray[j] > 0)
                                {
                                    potentialSpecialKeys.Add((BossKeySpecialType)j);
                                }
                            }

                            if (potentialSpecialKeys.Count > 0)
                            {
                                newNode.specialType = potentialSpecialKeys[GenerationOperation.GenerateRandomResult(potentialSpecialKeys.Count - 1, seed, i)];
                            }
                            else
                            {
                                //random pick a key colour
                                BossKeySpecialType randomSpecialType = (BossKeySpecialType)GenerationOperation.GenerateRandomResult(3, seed, i);
                                //set key colour
                                newNode.specialType = randomSpecialType;
                            }

                            //if key colour is used again
                            if (specialKeyArray[(int)newNode.specialType] > 0)
                            {
                                //down convert to a door
                                newNode.nodeType = BossKeyType.GenericDoor;
                            }
                        }
                        

                        if (newNode.nodeType == BossKeyType.SpecialKey)
                        {
                            specialKeyArray[(int)newNode.specialType]++;
                            backtrackKeyArray[(int)newNode.specialType]++;
                            spawnedSpecialKeys++;
                        }

                    }
                    //handle generic doors
                    if (newNode.nodeType == BossKeyType.GenericDoor)
                    {         
                        if (spawnedGenericKeys == 0)
                        {
                            //downgrade to key
                            newNode.nodeType = BossKeyType.GenericKey;
                        }

                        if (newNode.nodeType == BossKeyType.GenericDoor)
                        {
                            newNode.specialType = BossKeySpecialType.None;
                            spawnedGenericDoors++;
                            spawnedGenericKeys--;
                            doorsInStack++;
                            floorDoor = true;
                        }
                    }
                    //handle generic keys
                    if (newNode.nodeType == BossKeyType.GenericKey)
                    {
                        //upgrade for backtrack
                        if (spawnedGenericKeys > 0 && spawnedSpecialDoors > 0)
                        {
                            int mykey = -1;
                            for (int k = 0; k < 4; k++)
                            {
                                if ((specialDoorsArray[k] > 0) && (specialKeyArray[k] == 0))
                                {
                                    //bump backtrack array key
                                    backtrackKeyArray[k] += specialDoorsArray[k];

                                    if (backtrackDistanceMin <= backtrackKeyArray[k])
                                    {
                                        mykey = k;
                                    }
                                        
                                }
                            }
                            if (mykey != -1)
                            {
                                newNode.nodeType = BossKeyType.SpecialKey;
                                newNode.specialType = (BossKeySpecialType) mykey;
                                specialKeyArray[(int)newNode.specialType]++;
                                spawnedSpecialKeys++;
                            }
                        }

                        if (newNode.nodeType == BossKeyType.GenericKey)
                        {
                            newNode.specialType = BossKeySpecialType.None;
                            spawnedGenericKeys++;
                        }
                            
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
                    BossKeyNode greatestDepthNode = spawnStack.Peek();
                    int greatestDepth = 0;
                    bool isValidGreatestDepthFound = false;
                    //find deepest node
                    for (int i = 0; i < nodeArray.Length; i++)
                    {
                        int parentCount = 0;
                        BossKeyNode mynode = nodeArray[i];
                        while (mynode.parent != null)
                        {
                            parentCount++;
                            mynode = mynode.parent;
                        }
                        //Greatest depth node that is a door and not past max depth
                        if ((parentCount <= depth - 1) && (greatestDepth <= parentCount) && ((mynode.nodeType == BossKeyType.SpecialDoor) || (mynode.nodeType == BossKeyType.GenericDoor)))
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
