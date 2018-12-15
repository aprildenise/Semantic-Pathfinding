using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cell{


    public int zoneId; //the zone that the cell belongs to
    public bool isWalkable; //is the cell walkable
    public Vector3 worldPosition; //position of the cell in world coordinates
    public float cellSize; //size of the cell in world coordinates

    public List<Edge> edgesToNeighbors;

    public int gridPositionX; //position of the cell in the 2d grid
    public int gridPositionZ;
    public bool threshold; //temp

    ////for A* gCost and hCost
    ////!!WILL HAVE TO CHANGE LATER!!
    //public int gCost;
    //public int hCost;
    //public Cell parent; //only specifically for astar, should change later so that we 
    ////don't have so many references in this class


    //constructor
    public Cell(Vector3 worldpos, int gridx, int gridz, float size)
    {
        zoneId = -1;
        worldPosition = worldpos;
        gridPositionX = gridx;
        gridPositionZ = gridz;
        threshold = false;
        cellSize = size;
        edgesToNeighbors = new List<Edge>();
        CheckIfWalkable();
    }


    /* Check if this cell is walkable by checking if the navmesh
     * is present at its worldPosition
     */
    private void CheckIfWalkable()
    {
        NavMeshHit hit;
        Vector3 increment = new Vector3(cellSize / 2f, 0, -1f * cellSize / 2f);
        Vector3 center = worldPosition + increment;
        if (NavMesh.SamplePosition(center, out hit, cellSize, NavMesh.AllAreas))
        {
            //this cell is walkable
            isWalkable = true;
        }
        else
        {
            isWalkable = false;
        }
    }


    /* Assign the given list of edges and neighbors to this cell.
     */
    public void AssignNeighbors(List<Edge> n)
    {
        edgesToNeighbors = n;
    }


    ////get fCost
    //public int fCost
    //{
    //    get
    //    {
    //        return gCost + hCost;
    //    }
    //}
}
