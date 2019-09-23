using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public Vector2 gridWorldSize;
    public float nodeRadius;
    public LayerMask unwalkableMask;
    int gridSizeX, gridSizeY;

    float nodeDiameter;
    Node[,] grid;

    public List<Node> path;

    private void Start()
    {
        //nodeDiameter calculates the diameter of each Node
        nodeDiameter = nodeRadius * 2;
        //gridSizeX and Y are used to determine how many nodes fit in our grid
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        //Calls the method to create the grid
        CreateGrid();
    }

    void CreateGrid()
    {
        //Assigns the amount of nodes that fit into the grid
        grid = new Node[gridSizeX, gridSizeY];
        //Determines the bottom left corner of the grid, to be used for positions of the nodes
        Vector2 worldBottomLeft = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;
        //Creates a loop that goes through all the nodes in the grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //createes a Vector2 that determines the nodes positions, by addin the nodes center
                //to the world bottom left.
                Vector2 worldPosition = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                //Checks if the Node is not colliding with any objects(which is the obstacles), if so, assigns true to walkable
                bool walkable = !Physics2D.CircleCast(worldPosition, nodeRadius * 0.1f, Vector2.right, nodeRadius * 0.1f, unwalkableMask);
                //Assigns the information of walkable and the position to the corresponding node in the grid
                grid[x, y] = new Node(walkable, worldPosition, x, y);
            }
        }
    }

    //Method to get the info about the neighbouring nodes aronud the specified node
    public List<Node> GetNeighbours(Node node)
    {
        //creates a list to hold info abotu the neighbours
        List<Node> neighbours = new List<Node>();

        //loops through all grid positions around the node
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y < 1; y++)
            {
                //The node with position 0,0 is the node in the middle and therefore need to be skipped
                if (x == 0 && y == 0)
                {
                    continue;
                }

                //Variables that hold info aboutthe neighbour nodes by adding the values from the for loop
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //CHecks if the nieghbours have higher value than this node and also if they are inside the grids range
                //if so, adds them to the neighbours list
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        //Ends the loop by returing the list
        return neighbours;
    }

    //In the case of objects that does not have a static position, we use this method to
    //compare that objects position to the grid
    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        //Converts the objects position x and y to percent of the grid size
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        //Clamps the values to prevent a position outside of the grid
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        //Converts the information to int and then multiplies by the percent to determine the
        //position on the grid
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        //Returns the position as a Node
        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        //Draws the grid size in the UI
        Gizmos.DrawWireCube(transform.position, gridWorldSize);

        //Checks to see if the grid is not empty
        if (grid != null)
        {
            //Goes through all the nodes in the grid
            foreach (Node n in grid)
            {
                //If the object is walkable, draw it as white, if not, as red.
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                //if the path list is not empt and the node position in the retrace path list exists
                //inside the grids list
                //then paints that node black to show the path
                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                //Draws the cube at the corresponding position with a slightly smaller size
                Gizmos.DrawCube(n.worldPosition, Vector2.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
