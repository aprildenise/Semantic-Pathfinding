using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{



    public Vector3 worldPosition; //position in the world space
    public int level;
    public int gridX; //incedes of the node in the lowest level (if applicable)
    public int gridY;
    public string semantic;

    public Node parent;

    public Node(Vector3 worldPosition, int gridX, int gridY, int level, string semantic)
    {
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.level = level;
        this.semantic = semantic;
    }

    /* Old code
    //Constructor
    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    //may not be accruate for our case. Since I took this code from my previous 
    //implementation of A*
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    */



}
