using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgo : MonoBehaviour
{

    //references
    public Map map;

    //globals
    private Transform agent;
    private Transform target;

    public List<Cell> path;

    //temp
    //List<Cell> things;

    //for movement
    /*
    public Vector3 currentPos;
    public int currentCellToMoveTo = 0;
    */
    //float Timer = 0; 


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



    /* Find the path using the A* algo
     * Input: starting position of the agent and its target position
     */
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
                //choose which cell to look at
                if (openSet[i].fCost < currCell.fCost || (openSet[i].fCost == currCell.fCost && openSet[i].hCost < currCell.hCost))
                {

                    currCell = openSet[i];
                }
            }


            //update both sets
            openSet.Remove(currCell);
            closedSet.Add(currCell);


            //check if we have already reached the goal
            if (currCell == targetCell)
            {
                List<Cell> p = RetracePath(startCell, targetCell);
                return;
            }


            //if we have yet to reach the goal, look at the cell's neighbors
            foreach (Edge e in currCell.edgesToNeighbors)
            {

                Cell neighbor = e.incident; 
                //if this neighbor is unwalkable (should not be) or if we already visited this cell
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                //determine path based on gCost
                int newCostToNeighbor = currCell.gCost + GetDistance(currCell, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {

                    //update this cell with the new costs
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetCell);
                    //change the parent of the neighbor
                    neighbor.parent = currCell;

                    if (!openSet.Contains(neighbor))
                    {
                        //add this neighboring cell to the open set
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



    /* Return the path given by the A* algorithm
     * Input: the start cell of the agent, the end cell of the agent
     * Output: list of nodes that is the path to the goal
     */
    public List<Cell> RetracePath(Cell start, Cell end)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = end;
        while (currentCell != start)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();
        this.path = path;
        return path;
    }

    //returns the found path
    /*
    public List<Cell> RetrievesPath(Cell start, Cell end)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = end;
        while (currentCell != start)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();
        this.path = path;
        return path;
    }
    */


    
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            foreach (Cell c in path)
            {
                Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
                Vector3 center = c.worldPosition + increment;
                Gizmos.color = Color.green;
                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
            }
        }
    }
    

}
