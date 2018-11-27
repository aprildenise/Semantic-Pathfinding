using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgo : MonoBehaviour
{

    //variables
    public Map map;
    public Transform agent;
    public Transform target;

    public List<Cell> aStarPath;

    //temp
    List<Cell> things;

    //for movement
    public Vector3 currentPos;
    public int currentCellToMoveTo = 0;
    float Timer = 0;


    void Awake()
    {
        map.GetComponent<Map>();
    }



    void LateUpdate()
    {
        //FindPath(agent.position, target.position);
    }


    ////move the agent to the target position
    //private void FixedUpdate()
    //{
    //    if (aStarPath != null && currentCellToMoveTo < aStarPath.Count)
    //    {
    //        Timer += Time.deltaTime * 1f;
    //        currentPos = aStarPath[currentCellToMoveTo].worldPosition;
    //        if (agent.transform.position != currentPos)
    //        {
    //            agent.transform.position = Vector3.Lerp(agent.transform.position, currentPos, Timer);
    //        }
    //        else
    //        {
    //            if (currentCellToMoveTo < aStarPath.Count - 1)
    //            {
    //                currentCellToMoveTo++;
    //                CheckCell();
    //            }
    //        }
    //    }
    //}

    ////for movement
    //void CheckCell()
    //{
    //    Timer = 0;
    //    currentPos = aStarPath[currentCellToMoveTo].worldPosition;
    //}

    //A*
    public void FindPath(Vector3 startPos, Vector3 targetPos)
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
                if (openSet[i].fCost < currCell.fCost || (openSet[i].fCost == currCell.fCost && openSet[i].hCost < currCell.hCost)) //new code
                //if (openSet[i].fCost < currCell.fCost || (openSet[i].fCost == currCell.fCost)
                {

                    currCell = openSet[i];
                    /*
                    if (openSet[i].hCost < currCell.hCost)
                    {
                        
                    }   
                    */
                }
            }


            //update both sets
            openSet.Remove(currCell);
            closedSet.Add(currCell);


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
                    {
                        openSet.Add(neighbor);
                    }
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
        things = path;
    }



    private void OnDrawGizmos()
    {
        if (things != null)
        {
            foreach (Cell c in things)
            {
                Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
                Vector3 center = c.worldPosition + increment;
                Gizmos.color = Color.green;
                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
            }
        }
    }

}
