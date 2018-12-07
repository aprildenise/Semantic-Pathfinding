using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent: MonoBehaviour{

    //references
    public Map map;

    //for pathfinding
    private Dictionary<Cell, Cell> cameFrom;
    private Dictionary<Cell, float> costSoFar;
    private Dictionary<Threshold, Threshold> tcameFrom;
    private Dictionary<Threshold, float> tcostSoFar;
    private List<Cell> pathToTake; 
    private List<Threshold> tpathToTake; //temp
    private bool pathFound;

    //temporary
    public Transform goal;
    //public Transform agent;


    //signals to update or pause pathfinding
    private bool isHalted = false;
    private bool isRefreshed = false;
    private int updateRate = 0;

    //for movement
    public float moveSpeed;
    private int stepsTaken = 0; //aka cell in the pathToTake list
    private float Timer;
    private Vector3 nextCell;


    void Start(){
        pathToTake = FindHPAPath(transform.position, goal.position);
    }

    void RestartPath()
    {
        pathToTake = null;
        tpathToTake = null;
        pathFound = false;
    }

    //for movement
    void LateUpdate(){
        if (pathFound && !isHalted)
        {
            Timer += Time.deltaTime * moveSpeed;
            nextCell = pathToTake[stepsTaken].worldPosition;
            if (transform.position != nextCell)
            {
                transform.position = Vector3.Lerp(transform.position, nextCell, Timer);
            }
            else
            {
                if (stepsTaken < pathToTake.Count - 1)
                {
                    stepsTaken++;
                    CheckCell();
                }
            }
        }
    }

    private void CheckCell()
    {
        Timer = 0;
        nextCell = pathToTake[stepsTaken].worldPosition;
    }


    public void HaltAgent(bool status)
    {
        isHalted = status;
    }

    /* An implementation for hpa
    */
    public List<Cell> FindHPAPath(Vector3 start, Vector3 goal)
    {
        List<Cell> finalPath = new List<Cell>();

        //add the start position and the goal position to the threshold graph
        //by finding which threshold is close to the two positions
        //Debug.Log("For start:");
        Threshold thresholdStart = FindNeartestThreshold(start, goal);
        //Debug.Log("For goal:");
        Threshold thresholdGoal = FindNeartestThreshold(goal, start);

        //find a path of thresholds that exist starting from thresholdStart and to thresholdGoal
        List<Threshold> thresholdPath = FindThresholdPath(thresholdStart, thresholdGoal);
        tpathToTake = thresholdPath;

        //using all these thresholds, find a path to the goal from the start with the help of astar
        //start to first threshold
        List<Cell> temp = FindCellPath(start, thresholdStart.worldPosition);
        finalPath.AddRange(temp);
        //between thresholds 
        for (int i = 1; i < thresholdPath.Count; i++)
        {
            Debug.Log("Iteration:" + i);
            //get the last position from the final path, this will be the new "start"
            Vector3 newStart = thresholdPath[i - 1].worldPosition;
            //the threshold will be the new goal
            Vector3 newGoal = thresholdPath[i].worldPosition;
            temp = FindCellPath(newStart, newGoal);

            //add this path to the finalpath
            finalPath.AddRange(temp);

        }
        //final threshold to the end
        temp = FindCellPath(thresholdPath[thresholdPath.Count - 1].worldPosition, goal);
        finalPath.AddRange(temp);

        //done
        pathToTake = finalPath; //temp
        pathFound = true;
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
        Cell currentCell = map.CellFromWorldPos(position);
        Cell goalCell = map.CellFromWorldPos(goal);

        //get the thresholds list from the zone that has this cell
        int id = currentCell.zoneId;
        Zone zone = map.GetZone(id);

        //find the threshold that is closest to the goal
        float cost = Mathf.Infinity;
        int i = 0;
        foreach (Threshold t in zone.thresholds){
            float temp = GetCellCost(goalCell, t) + GetCellCost(currentCell, t);
            if (temp <= cost){
                
                cost = temp;
                threshold = t;
                //Debug.Log("A potential threshold! It is :" + threshold.worldPosition + " at the distance of:" + temp + " at: " + i);
                i++;
            }
        }

        return threshold;


        /*
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
        */
    }


    //new code for the astar algorithm
    public List<Cell> FindCellPath (Vector3 startPos, Vector3 goalPos){
        
        Cell startCell = map.CellFromWorldPos(startPos);
        Cell goalCell = map.CellFromWorldPos(goalPos);
        PriorityQueue<Cell> frontier = new PriorityQueue<Cell>();
        cameFrom = new Dictionary<Cell, Cell>();
        costSoFar = new Dictionary<Cell, float>();
        List<Cell> path = null;

        frontier.Enqueue(startCell, 0);
        cameFrom[startCell] = startCell;
        costSoFar[startCell] = 0;

        while (frontier.Count() > 0){
            Cell current = frontier.Deqeueue();

            if (current.Equals(goalCell)){
                Debug.Log("A path has been found!");
                path = RetraceCellPath(startCell, goalCell);
                return path;
            }

            foreach(Edge e in current.edgesToNeighbors){
                Cell neighbor = e.incident;

                float newCost = costSoFar[current] + GetCellCost(current, neighbor);
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]){
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + GetCellHeuristic(neighbor, goalCell);
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        Debug.Log("A path was not found...");
        return path;
    }


    private List<Cell> RetraceCellPath(Cell startCell, Cell goalCell){
        
        //loop through the cameFrom dictionary
        List<Cell> path = new List<Cell>();
        Cell currentCell = goalCell;
        while (currentCell != startCell)
        {
            Debug.Log("Adding....");
            path.Add(currentCell);
            currentCell = cameFrom[currentCell];
        }

        path.Add(startCell);
        path.Reverse();
        return path;
    }


    public List<Threshold> FindThresholdPath(Threshold startThreshold, Threshold goalThreshold)
    {

        PriorityQueue<Threshold> frontier = new PriorityQueue<Threshold>();
        List<Threshold> path = null;
        tcameFrom = new Dictionary<Threshold, Threshold>();
        tcostSoFar = new Dictionary<Threshold, float>();

        frontier.Enqueue(startThreshold, 0);
        tcameFrom[startThreshold] = startThreshold;
        tcostSoFar[startThreshold] = 0;

        while (frontier.Count() > 0)
        {
            Threshold current = frontier.Deqeueue();

            if (current.Equals(goalThreshold))
            {
                path = RetraceThresholdPath(startThreshold, goalThreshold);
                break;
            }

            foreach (ThresholdEdge e in current.tedgesToNeighbors)
            {
                Threshold neighbor = e.incident;

                float newCost = tcostSoFar[current] + GetThresholdCost(current, neighbor);
                if (!tcostSoFar.ContainsKey(neighbor) || newCost < tcostSoFar[neighbor])
                {
                    tcostSoFar[neighbor] = newCost;
                    float priority = newCost + GetThresholdHeuristic(neighbor, goalThreshold);
                    frontier.Enqueue(neighbor, priority);
                    tcameFrom[neighbor] = current;
                }
            }
        }

        return path;
    }


    
    private List<Threshold> RetraceThresholdPath(Threshold startThreshold, Threshold goalThreshold){
        
        //loop through the cameFrom dictionary
        List<Threshold> path = new List<Threshold>();
        Threshold current = goalThreshold;
        while (current != startThreshold)
        {
            path.Add(current);
            current = tcameFrom[current];
        }

        path.Add(startThreshold);
        path.Reverse();
        return path;
    }


    private float GetCellCost(Cell source, Cell sink){
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return cost;
    }


    private float GetCellHeuristic(Cell source, Cell sink){
        float h = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return h;
    }


    
    private float GetThresholdCost(Threshold source, Threshold sink){
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return cost;
    }


    private float GetThresholdHeuristic(Threshold source, Threshold sink){
        float h = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return h;
    }


    //private void OnDrawGizmos()
    //{
    //    if (pathToTake != null)
    //    {
    //        foreach (Cell c in pathToTake)
    //        {
    //            Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
    //            Vector3 center = c.worldPosition + increment;
    //            Gizmos.color = Color.green;
    //            Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
    //        }
    //    }
    //    if (tpathToTake != null)
    //    {
    //        foreach (Threshold c in tpathToTake)
    //        {
    //            Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
    //            Vector3 center = c.worldPosition + increment;
    //            Gizmos.color = Color.blue;
    //            Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
    //        }
    //    }
    //}


}