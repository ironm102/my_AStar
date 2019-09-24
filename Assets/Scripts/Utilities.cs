using UnityEngine;

public class Utilities
{
    //This class is mostly just for learning purposes, since i could just do an assertion in the actual scripts

    //Checks if an object is linked, if not, tells you to do that
    public void Assert(Transform obj)
    {
        Debug.Assert(obj != null, $"{obj} not found! Please set it in the editor!");
    }
}
