using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    #region variables, start and Update
    public Transform seeker, target;

    public float maxTime = 0.2f;
    float timer;

    Grid grid;
    Utilities utilities = new Utilities();

    List<Node> path;
    List<Node> tempPath;

    int pathIndex;
    bool foundPath = true;

    private void Awake()
    {
        utilities.Assert(seeker);
        utilities.Assert(target);
        grid = GetComponent<Grid>();
        RandomPos(target);
    }

    private void Update()
    {
        //does a check to see if the target is on an obstacle, if so, randomizes a new position
        while (Physics2D.Raycast(target.position, Vector2.zero, 1, grid.unwalkableMask).collider)
        {
            RandomPos(target);
        }
        //Calls the FindPath method, using the player and target positions as the sources
        FindPath(seeker.position, target.position);
        MovePlayer();
    }
    #endregion variables, start and Update

    #region FindPath
    void FindPath(Vector2 startPos, Vector2 targetPos)
    {
        //Makes variables out of the players and targets positions on the grid
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        //creates lists out that are for the nodes that  is to be looked at(openSet) and the ones
        //that we dont need to look at/already looked at(closed)
        //Also adds the the first node, which is the players position, as the object in the openSet
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        //Starts a loop that checks the objects in the open List, but only if there are objects in the list
        while (openSet.Count > 0)
        {
            //Sets the first object in the List as a temp variable that we compare to in the loop
            Node currentNode = openSet[0];
            //for loop that checks all nodes in the open list
            for (int i = 1; i < openSet.Count; i++)
            {
                //Then starts a for loop to determine if there is any objects with lower cost in the List
                //If there is, sets that object as the currentNode to be used from now on
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            //Removes the current Node from the open list and adds it to the closed list,
            //since we don't need to compare to it anymore
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            //If the current node has the same position as the target node, then that means we are done,
            //And we trace back our steps to the original position and then exit the loop
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            //starts a loop to check the neighbours of our current node
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                //if the neighbour isnt walkable(an obstacle) or it's in the closed list, move on to the next node
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                //Determines the new cost of the neighbour node, by adding the distance to the target
                //to our current node and puts the value in a new variable
                int newMovementCostToNeighbour = currentNode.gCost + getDistance(currentNode, neighbour);
                //if the new cost is less than the neighbours or if the open set does not contain our neighbour
                //give new values to the neighbour
                //This is to update the value of a path in the case of it being close to the start node,
                //but we took a long way to get there,therefore has a high cost
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    //Sets the neighbours gCost(distance from startPosition) to the new cost, which is our current cost
                    neighbour.gCost = newMovementCostToNeighbour;
                    //And updates the hCost(distance to targetPosition)
                    neighbour.hCost = getDistance(neighbour, targetNode);
                    //Also sets the current node as the nieghbours parent, to be used later to retrace our path back
                    neighbour.parent = currentNode;

                    //and lastly, if the neighbour is not in the open list, adds it
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }
    #endregion FindPath

    #region TracePath
    //Creates a method to retrace our path back to the starting node, using the players start position
    //and the targets position
    void RetracePath(Node startNode, Node endNode)
    {
        //Creates a list to hold the values of the nodes in the path
        path = new List<Node>();
        //Variable that holds the targets position
        Node currentNode = endNode;

        //A loop that adds all the nodes in the path to the path list
        //by constantly adding the current node and then changing it to the parent node
        //until there are no parents left

        while (currentNode != startNode)
        {

            path.Add(currentNode);
            currentNode = currentNode.parent;

        }
        //then reverse the path, since we want to trace back from the end node to get the shortest path
        path.Reverse();

        //changes path list in the grid class, to the values of this path list
        grid.path = path;
    }
    #endregion TracePath

    #region GetDistance
    //Method to get the distance from one node to the other and set the cost values for the nodes
    //and returns the cost as an integer
    int getDistance(Node nodeA, Node nodeB)
    {
        //Creates integers that hold the x and y grid position of the node
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        //returns the cost using the distance values, multiplied by 10 for easier reading
        //each horizontal/diagonal direction adds 10 to the cost and each diagonal adds 14
        //14 comes from the pythagoras theorem(x^2+x^2=y^2, so in this case it's 1^2+1^2=y^2, which is 1.4)
        //
        //If the y distance is shorter than the x distance, multiplies the vertical value by the Y distance
        //To get how many diagonal moves we need.Then adds the missing horizontal moves by substracting
        //the vertical moves count from the total horizontal moves count.
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        //If that is not the case, swaps the X and Y values and returns that value
        return 14 * dstX + 10 * (dstY - dstX);
    }
    #endregion GetDistancec

    #region Player Movement
    void MovePlayer()
    {
        //starts a timer to use for incremental movement
        timer += Time.deltaTime;

        //Since we only want to change the pathing to a new position once the seeker is at the target,
        //We do a bool that deactivates itself after setting a new temp list to the current path list
        if (foundPath)
        {
            tempPath = path;
            foundPath = false;
        }

        //Movement that only happens every 0.1sec, by reseting itself everytime it gets to the maxValue(0.1f)
        //also increments the pathIndex, which is used later to set the seekers position
        if (timer > maxTime)
        {
            seeker.transform.position = tempPath[pathIndex].worldPosition;
            pathIndex++;
            timer = 0f;
        }

        //checks if the seeker is at the same grid position as the target,
        //if so, randomize a new position for the target and reset the list index
        //and then sets the foundPath bool to true, to set the new list positions to the new targets list positions
        if (grid.NodeFromWorldPoint(seeker.position) == grid.NodeFromWorldPoint(target.position))
        {
            RandomPos(target);

            pathIndex = 0;
            foundPath = true;
        }
    }

    //randomizes the position of the specified object, to the size of the grid
    private void RandomPos(Transform transform)
    {
        Vector2 newTargetPos = new Vector2(Random.Range(Mathf.RoundToInt((-grid.gridWorldSize.x + 1) / 2), Mathf.RoundToInt(grid.gridWorldSize.x / 2)), Random.Range(Mathf.RoundToInt((-grid.gridWorldSize.y + 1) / 2), Mathf.RoundToInt(grid.gridWorldSize.y / 2)));
        transform.position = newTargetPos;
    }
    #endregion Player Movement
}
