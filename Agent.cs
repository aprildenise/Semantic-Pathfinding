using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent: MonoBehaviour{

    //references
    public Map map;

    //for pathfinding
    public Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
    public Dictionary<Cell, int> costSoFar = new Dictionary<Cell, int>();
    public Dictionary<Threshold, Threshold> tcameFrom = new Dictionary<Threshold, Threshold>();
    public Dictionary<Threshold, int> tcostSoFar = new Dictionary<Threshold, int>();
    public List<Cell> pathToTake; 
    public List<Threshold> tpathToTake;

    //temporary
    public Transform goal;
    public Transform agent; 
    

    //signals to update or pause pathfinding
    public bool isPaused;
    public bool isStopped;
    public bool isRefreshed;
    public float rate;


    void Start(){
        map.InitMap();
        agent = gameObject.GetComponent<Transform>();
        HPAAlt(agent.position, goal.position);
    }

    void LateUpdate(){
        
    }

    //copy and pasted algorithms
    /* An alternate implementation for hpa
 */
    public List<Cell> HPAAlt(Vector3 start, Vector3 goal)
    {
        List<Cell> finalPath = new List<Cell>();

        //add the start position and the goal position to the threshold graph
        //by finding which threshold is close to the two positions
        Threshold thresholdStart = FindNeartestThreshold(start, goal);
        Threshold thresholdGoal = FindNeartestThreshold(goal, start);

        //find a path of thresholds that exist starting from thresholdStart and to thresholdGoal
        List<Threshold> thresholdPath = FindPathT(thresholdStart, thresholdGoal);

        //using all these thresholds, find a path to the goal from the start with the help of astar
        //first node
        finalPath.Add(map.CellFromWorldPos(start));
        //start to first threshold
        List<Cell> temp = FindPath(start, thresholdStart.worldPosition);
        finalPath.AddRange(temp);
        //between thresholds 
        for (int i = 1; i < thresholdPath.Count; i++)
        {
            //get the last position from the final path, this will be the new "start"
            Vector3 newStart = thresholdPath[i - 1].worldPosition;
            //the threshold will be the new goal
            Vector3 newGoal = thresholdPath[i].worldPosition;
            temp = FindPath(newStart, newGoal);

            //add this path to the finalpath
            finalPath.AddRange(temp);

        }
        //final threshold to the end
        temp = FindPath(thresholdPath[thresholdPath.Count - 1].worldPosition, goal);
        finalPath.AddRange(temp);

        //done
        pathToTake = finalPath; //temp
        return finalPath;


    }

        /* Given a position, find the threshold that is near to both the beginning
     * position and the goal position
     * Input: position in question, goal position
     * Output: threshold that is nearest to those positions
     */

    public Threshold FindNeartestThreshold(Vector3 position, Vector3 goal)
    {
        Threshold threshold = null;

        //find the zone that the starting position belongs to
        Cell cell = map.CellFromWorldPos(position);

        //traverse through the threshold graph to find the appropriate threshold
        //the appropriate threshold is:
        //in the same zone as or around the cell, and close to the goal (if possible)
        float distanceFromPosition = Mathf.Infinity;
        foreach (Threshold t in map.thresholdGraph)
        {


            //find if this threshold is close to the starting position
            if (t.tzoneID == cell.zoneId || t.zoneId == cell.zoneId)
            {
                //this threshold is in or connected with the same zone as the current position
                //we know it is close to the starting position

                //see if this threshold is close to the goal
                float distance = Vector3.Distance(t.worldPosition, goal);
                if (distance <= distanceFromPosition)
                {
                    //we'll consider this to be the appropriate threshold
                    distanceFromPosition = distance;
                    threshold = t;
                }
            }
        }

        return threshold;
    }


       /* Find the path using the A* algo
     * Input: starting position of the agent and its target position
     */
    public List<Cell> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell startCell = map.CellFromWorldPos(startPos);
        Cell targetCell = map.CellFromWorldPos(targetPos);

        List<Cell> foundPath = null;

        //keep track of which cells have have been visited
        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();
        Dictionary<Cell, int> hcostSoFar = new Dictionary<Cell, int>();
        cameFrom[startCell] = startCell;
        costSoFar[startCell] = 0;

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            Cell currCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                //choose which cell to look at
                Cell openCell = openSet[i];
                int openfCost = costSoFar[openCell] + hcostSoFar[openCell];
                int currfCost = costSoFar[currCell] + hcostSoFar[currCell];
                if (openfCost < currfCost || (openfCost == currfCost && hcostSoFar[openCell] < hcostSoFar[currCell]))
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
                foundPath = RetracePath(startCell, targetCell);
                Debug.Log("path found in astar");
                break;
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
                int newCostToNeighbor = (int)costSoFar[currCell] + (int)Vector3.Distance(currCell.worldPosition, neighbor.worldPosition);
                
                if (!costSoFar.ContainsKey(neighbor)){
                                        //update this cell with the new costs
                    //neighbor.gCost = newCostToNeighbor;
                    costSoFar[neighbor] = newCostToNeighbor;
                    int hCost = (int)newCostToNeighbor + (int)Vector3.Distance(neighbor.worldPosition, targetCell.worldPosition);
                    hcostSoFar[neighbor] = hCost;
                    //change the parent of the neighbor
                    cameFrom[neighbor] = currCell;

                    if (!openSet.Contains(neighbor))
                    {
                        //add this neighboring cell to the open set
                        openSet.Add(neighbor);
                    }
                }

                // if (newCostToNeighbor < costSoFar[neighbor] || !openSet.Contains(neighbor))
                // {

                //     //update this cell with the new costs
                //     //neighbor.gCost = newCostToNeighbor;
                //     costSoFar[neighbor] = newCostToNeighbor;
                //     int hCost = (int)newCostToNeighbor + (int)Vector3.Distance(neighbor.worldPosition, targetCell.worldPosition);
                //     hcostSoFar[neighbor] = hCost;
                //     //change the parent of the neighbor
                //     cameFrom[neighbor] = currCell;

                //     if (!openSet.Contains(neighbor))
                //     {
                //         //add this neighboring cell to the open set
                //         openSet.Add(neighbor);
                //     }
                // }
            }
        }

        //done, return the path found
        return foundPath;
    }


   /* Like the function above, but on the threshold graph rather than the entire 2d grid
     */
    public List<Threshold> FindPathT(Threshold startThreshold, Threshold targetThreshold)
    {


        List<Threshold> foundPath = null;

        //keep track of which threhsholds have have been visited
        List<Threshold> openSet = new List<Threshold>();
        HashSet<Threshold> closedSet = new HashSet<Threshold>();
        Dictionary<Cell, int> hcostSoFar = new Dictionary<Cell, int>();
        tcameFrom[startThreshold] = startThreshold;
        tcostSoFar[startThreshold] = 0;

        //List<Cell> openSet = new List<Cell>();
        //HashSet<Cell> closedSet = new HashSet<Cell>();

        openSet.Add(startThreshold);

        while (openSet.Count > 0)
        {
            Threshold currCell= openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                //choose which cell to look at
                Threshold openCell = openSet[i];
                int openfCost = tcostSoFar[openCell] + hcostSoFar[openCell];
                int currfCost = tcostSoFar[currCell] + hcostSoFar[currCell];
                if (openfCost < currfCost || (openfCost == currfCost && hcostSoFar[openCell] < hcostSoFar[currCell]))
                {

                    currCell = openSet[i];
                }
            }


            //update both sets
            openSet.Remove(currCell);
            closedSet.Add(currCell);


            //check if we have already reached the goal
            if (currCell == targetThreshold)
            {
                foundPath = RetracePathT(startThreshold, targetThreshold);
                Debug.Log("path found");
                break;
            }


            //if we have yet to reach the goal, look at the cell's neighbors
            foreach (ThresholdEdge e in currCell.tedgesToNeighbors)
            {

                Threshold neighbor = e.incident;
                //if this neighbor is unwalkable (should not be) or if we already visited this cell
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                 //determine path based on gCost
                 
                int newCostToNeighbor = (int)tcostSoFar[currCell] + (int)Vector3.Distance(currCell.worldPosition, neighbor.worldPosition);
                
                if (!tcostSoFar.ContainsKey(neighbor)){
                                        //update this cell with the new costs
                    //neighbor.gCost = newCostToNeighbor;
                    tcostSoFar[neighbor] = newCostToNeighbor;
                    int hCost = (int)newCostToNeighbor + (int)Vector3.Distance(neighbor.worldPosition, targetThreshold.worldPosition);
                    hcostSoFar[neighbor] = hCost;
                    //change the parent of the neighbor
                    tcameFrom[neighbor] = currCell;

                    if (!openSet.Contains(neighbor))
                    {
                        //add this neighboring cell to the open set
                        openSet.Add(neighbor);
                    }
                }

                // if (newCostToNeighbor < tcostSoFar[neighbor] || !openSet.Contains(neighbor))
                // {

                //     //update this cell with the new costs
                //     //neighbor.gCost = newCostToNeighbor;
                //     tcostSoFar[neighbor] = newCostToNeighbor;
                //     int hCost = (int)newCostToNeighbor + (int)Vector3.Distance(neighbor.worldPosition, targetThreshold.worldPosition);
                //     hcostSoFar[neighbor] = hCost;
                //     //change the parent of the neighbor
                //     tcameFrom[neighbor] = currCell;

                //     if (!openSet.Contains(neighbor))
                //     {
                //         //add this neighboring cell to the open set
                //         openSet.Add(neighbor);
                //     }
                // }
            }
        }

        //done, return the path found
        return foundPath;
    }
    
    public List<Cell> RetracePath(Cell start, Cell end)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = end;
        while (currentCell != start)
        {
            path.Add(currentCell);
            currentCell = cameFrom[currentCell];
            //currentCell = currentCell.parent;
        }

        path.Add(start);
        path.Reverse();
        this.pathToTake = path;
        return path;
    }

    
    public List<Threshold> RetracePathT(Threshold start, Threshold end)
    {
        List<Threshold> path = new List<Threshold>();
        Threshold currentCell = end;
        while (currentCell != start)
        {
            path.Add(currentCell);
            //currentCell = currentCell.tparent;
            currentCell = tcameFrom[currentCell];
        }
        path.Add(start);
        path.Reverse();
        tpathToTake = path;
        return path;
    }


    private void OnDrawGizmos()
    {
        if (pathToTake != null)
        {
            foreach (Cell c in pathToTake)
            {
                Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
                Vector3 center = c.worldPosition + increment;
                Gizmos.color = Color.green;
                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
            }
        }
    }


}