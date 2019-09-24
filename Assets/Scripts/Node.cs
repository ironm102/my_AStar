using UnityEngine;

public class Node
{
    #region Variables
    //The node class is used to hold information about the objects in the grid
    //In this case, it holds info about the world position and also the position of the object in the grid
    //And if the position is walkable or not, i.e. is not an obstacle

    public Vector2 worldPosition;
    public bool walkable;
    public int gridX;
    public int gridY;

    //Each node also holds info about it's costs and also which is the parent(the last node we moved from) in itself
    public int gCost;
    public int hCost;
    public Node parent;


    //Determines the fCost, which is the total cost to move to the given position
    //Calculated by addingh the gCost and the hCost
    public int fCost
    {
        get { return gCost + hCost; }
    }
    #endregion Variables

    #region Constructor
    //Creates a constructor that assigns given info to the variables
    public Node(bool _walkable, Vector2 _worldPosition, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }
    #endregion Constructor
}
