using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    //variables
    Map map;
    public Transform agent;
    public Transform target;

    public List<Cell> aStarPath;


    void Awake()
    {
        map.GetComponent<Map>();
    }


    //move the agent to the target position
    private IEnumerable MoveAgent()
    {
        if (aStarPath == null)
        {
            yield return null;
        }
        foreach (Cell cell in aStarPath)
        {
            Vector3 newPos = cell.worldPosition;
            agent.transform.Translate(newPos * Time.deltaTime);

            //wait until the agent has arrived at the new position before giving it a newPos
            do
            {
                yield return null;
            } while (agent.position != newPos);
        }

        yield return null;
    }


    //A*
    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell startCell = map.CellFromWorldPos(startPos);
        Cell targetCell = map.CellFromWorldPos(targetPos);

        Debug.Log("start: " + startCell);
        Debug.Log("target: " + targetCell);

        //keep track of which cells have have been visited
        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();
        List<Cell> path = new List<Cell>(); //list that will hold the path to the targetPos

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            Cell currCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currCell.fCost || (openSet[i].fCost == currCell.fCost && openSet[i].hCost < currCell.hCost)) //new code
                //if (openSet[i].fCost < currCell.fCost || (openSet[i].fCost == currCell.fCost)
                {
                    if (openSet[i].hCost < currCell.hCost)
                        currCell = openSet[i];
                }
            }

            //update both sets
            openSet.Remove(currCell);
            closedSet.Add(currCell);
            path.Add(currCell); //add this cell to our path

            if (currCell == targetCell)
            {
                RetracePath(startCell, targetCell);
                return;
            }

            //check neighbors
            //foreach (Cell neighbor in map.getNeighbor(currCell))
            foreach (Edge e in currCell.edgesToNeighbors) //new code
            {
                Cell neighbor = e.incident; //new code
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
                    //new code
                    //change the parent of the neighbor
                    neighbor.parent = currCell;

                    /*
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
                    */

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
        MoveAgent();
    }



    //new code
    /* Return the path given by the A* algorithm
     * Input: the start cell of the agent, the end cell of the agent
     */
    public void RetracePath(Cell start, Cell end)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = end;
        while (currentCell != start)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();
        aStarPath = path;
    }

}
