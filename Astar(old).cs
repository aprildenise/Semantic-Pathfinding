using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour {

    //variables
    public Map map;
    public Transform agent;
    public Transform target;


    void Awake()
    {
        //map.GetComponent<Map>();
    }
    //A*
    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell startCell = map.CellFromWorldPos(startPos);
        Cell targetCell = map.CellFromWorldPos(targetPos);

        //keep track of which cells have have been visited
        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            Cell currCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currCell.fCost || openSet[i].fCost == currCell.fCost)
                {
                    if (openSet[i].hCost < currCell.hCost)
                        currCell = openSet[i];
                }
            }

            //update both sets
            openSet.Remove(currCell);
            closedSet.Add(currCell);

            if (currCell == targetCell)
            {
                return;
            }

            //check neighbors
            foreach (Cell neighbor in map.getNeighbor(currCell))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                //determine path based on gCost
                int newCostToNeighbor = currCell.gCost + GetDistance(currCell, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetCell);
                    List<Edge> e = neighbor.edgesToNeighbors;
                    int cost = newCostToNeighbor;
                    for (int i = 0; i < e.Count; i++)
                    {
                        if (e[i].isActive)
                        {
                            if (e[i].origin.fCost < cost)
                            {
                                cost = e[i].origin.fCost;
                            }
                            else
                            {
                                e[i].origin = currCell;
                            }
                        }
                    }
                     
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
    }

    //get distance between two cells 
    int GetDistance(Cell cellA, Cell cellB)
    {
        int dstX = Mathf.Abs(cellA.gridPositionX - cellB.gridPositionX);
        int dstZ = Mathf.Abs(cellA.gridPositionZ - cellB.gridPositionZ);

        if (dstX > dstZ)
            return 14 * dstZ + 10 * (dstX - dstZ);
        return 14 * dstX + 10 * (dstZ - dstX);
    }


    void Update()
    {
        FindPath(agent.position, target.position);
    }

}
