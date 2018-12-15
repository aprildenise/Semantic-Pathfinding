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

    //the goal the agent wants to move to
    public Transform goal;

    //current signals to update or pause pathfinding
    [HideInInspector]
    public bool isCalculatingPath; //agent is calculating a path
    [HideInInspector]
    public bool hasFoundPath; //agent has finished calculating a path
    private bool hasHaltedMovement; //agent has halted following the path it has found
    private bool hasHaltedCalculating; //agent has halted calculating a path;
    private int updateRate = 0;

    //for movement
    public float moveSpeed;
    private int stepsTaken = 0; //aka cell in the pathToTake list
    private float Timer;
    private Vector3 nextCell;


    void Start(){
        //setups
        hasHaltedMovement = false;
        hasHaltedCalculating = false;

        //temporarily here
        pathToTake = FindHPAPath(transform.position, goal.position);
        //pathToTake = FindCellPath(transform.position, goal.position);
    }


    /* Used for moving the agent from its origin to its goal 
     * if it has found a path and is allowed to move.
     */
    void LateUpdate(){
        if (hasFoundPath && !hasHaltedMovement)
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
                    if (updateRate != 0){
                        if (stepsTaken % updateRate == 0 && stepsTaken != 0){
                        //update the path
                        }
                    }
                }
            }
        }
    }

    /* Helper function used to move the agent.
     */
    private void CheckCell()
    {
        Timer = 0;
        nextCell = pathToTake[stepsTaken].worldPosition;
    }


    /* Halts the agent from moving on its path.
     * Input: desired status.
     */
    public void HaltMovement(bool status)
    {
        hasHaltedMovement = status;
    }


    /* Halts the agent from calculating its path.
     * Input: desired status
     */
    public void HaltCalculating(bool status){
        hasHaltedCalculating = status;
    }

    /* Change the rate at which the agent recalculates the path.
     * Rate is equvalent to the number of cells the agent travels in
     * order to get to its goal.
     * Input: desired status.
     */
    public void ChangeUpdateRate(int rate){
        updateRate = rate;
    }


    /* Helper function to allow the agent to halt path calculation.
     */
    private IEnumerator WaitUntilCanCalculate(){
        yield return new WaitUntil(() => hasHaltedCalculating == false);
    }

    /* Find a path from the agent's starting location to the desired goal
     * location. This uses the HPA* algorithm.
     * Input: start location, goal location.
     * Output: the calculated path as a list of cells the agent must visit to
     * get to the goal.
    */
    private List<Cell> FindHPAPath(Vector3 start, Vector3 goal)
    {
        //setups
        hasFoundPath = false;
        isCalculatingPath = true;
        List<Cell> finalPath = new List<Cell>();

        //add the start position and the goal position to the threshold graph
        //by finding which threshold is close to the two positions.
        Threshold thresholdStart = FindNeartestThreshold(start, goal);
        Threshold thresholdGoal = FindNeartestThreshold(goal, start);

        //find a path of thresholds that exist starting from thresholdStart and to thresholdGoal.
        List<Threshold> thresholdPath = FindThresholdPath(thresholdStart, thresholdGoal);
        tpathToTake = thresholdPath;

        //using all these thresholds, find a path to the goal from the start with the help of astar.
        //path from start threshold to between threshold.
        
        //tests:
        foreach(Threshold t in thresholdPath)
        {
            Debug.Log("threshold position:" + t.worldPosition);
            Cell c = map.CellFromWorldPos(t.worldPosition);
            Debug.Log("cellposition:" + c.worldPosition);
        }

        List<Cell> temp = FindCellPath(start, thresholdStart.worldPosition);
        finalPath.AddRange(temp);

        //path between the thresholds.
        for (int i = 1; i < thresholdPath.Count; i++)
        {
            //get the last position from the final path, this will be the new "start."
            Vector3 newStart = thresholdPath[i - 1].worldPosition;
            
            //the threshold will be the new goal.
            Vector3 newGoal = thresholdPath[i].worldPosition;
            temp = FindCellPath(newStart, newGoal);

            //add this path to the finalpath.
            finalPath.AddRange(temp);

        }
        //path from final threshold to goal.
        //temp = FindCellPath(thresholdPath[thresholdPath.Count - 1].worldPosition, goal);
        //finalPath.AddRange(temp);

        //done!
        pathToTake = finalPath;
        hasFoundPath = true;
        isCalculatingPath = false;
        return finalPath;


    }

    /* Given a position, find the threshold that is near to both the beginning
     * position and the goal position.
     * Input: position in question, goal position.
     * Output: threshold that is nearest to those positions.
     */
    private Threshold FindNeartestThreshold(Vector3 position, Vector3 goal)
    {
        //setups.
        Threshold threshold = null;

        //find the zone that the starting position belongs to.
        Cell currentCell = map.CellFromWorldPos(position);
        Cell goalCell = map.CellFromWorldPostest(goal);

        //get the thresholds list from the zone that has this cell.
        int id = currentCell.zoneId;
        Zone zone = map.GetZone(id);

        //find the threshold that is closest to the goal.
        float cost = Mathf.Infinity;
        int i = 0;
        foreach (Threshold t in zone.thresholds){
            float temp = GetCellCost(goalCell, t) + GetCellCost(currentCell, t);
            if (temp <= cost){
                
                cost = temp;
                threshold = t;
                i++;
            }
        }

        if (!threshold.isWalkable)
        {
            Debug.Log("hey now...");
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


    /* Find a path from the agent's starting location to the desired goal
     * location. This uses the A* algorithm.
     * Input: start location, goal location.
     * Output: the calculated path as a list of cells the agent must visit to
     * get to the goal.
    */
    private List<Cell> FindCellPath (Vector3 startPos, Vector3 goalPos){


        Cell startCell = map.CellFromWorldPos(startPos);
        Cell goalCell = map.CellFromWorldPos(goalPos);

        Debug.Log("goalcell:" + goalCell.worldPosition + ". this is " + goalCell.isWalkable);

        PriorityQueue<Cell> frontier = new PriorityQueue<Cell>();
        cameFrom = new Dictionary<Cell, Cell>();
        costSoFar = new Dictionary<Cell, float>();
        List<Cell> path = null;

        frontier.Enqueue(startCell, 0);
        cameFrom[startCell] = startCell;
        costSoFar[startCell] = 0;
        int temp = 0;
        while (frontier.Count() > 0){
            temp++;

            //Check if the agent is able to calculate their path.
            if (hasHaltedCalculating){
                StartCoroutine(WaitUntilCanCalculate());
            }


            //else, continue to calculate the path.
            Cell current = frontier.Deqeueue();

            if (current.Equals(goalCell)){
                path = RetraceCellPath(startCell, goalCell);
                return path;
            }
            if (current.worldPosition == new Vector3(-130.5f, -21.0f, -3.8f))
            {
                Debug.Log("wow!");
                if (!current.Equals(goalCell))
                {
                    Debug.Log("HUH?");
                }
            }
            if (temp == 80)
            {
                break;
            }

            Debug.Log("We're looking at the current cell. this is:");
            Debug.Log(current.worldPosition);
            Debug.Log("Goal is:" + goalCell.worldPosition);

            foreach(Edge e in current.edgesToNeighbors){
                Cell neighbor = e.incident;

                //Debug.Log("We are looking at neighbors!. Some info about this neighbor:");
                //Debug.Log(neighbor.worldPosition);
                //Debug.Log(neighbor.isWalkable);

                float newCost = costSoFar[current] + GetCellCost(current, neighbor);
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]){
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + GetCellHeuristic(neighbor, goalCell);
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return path;
    }


    /* Given the start cell and the goal cell of the agent, give calculated path 
     * using the cells that are in cameFrom.
     * Input: start location, goal location.
     * Output: the calculated path as a list of cells the agent must visit to
     * get to the goal.
     */
    private List<Cell> RetraceCellPath(Cell startCell, Cell goalCell){
        
        //loop through the cameFrom dictionary.
        List<Cell> path = new List<Cell>();
        Cell currentCell = goalCell;
        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = cameFrom[currentCell];
        }

        path.Add(startCell);
        path.Reverse();
        return path;
    }


    /* Similar to the above method, but for thresholds rather than cells.
     */
    private List<Threshold> FindThresholdPath(Threshold startThreshold, Threshold goalThreshold)
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

            //Check if the agent is able to calculate their path.
            if (hasHaltedCalculating){
                StartCoroutine(WaitUntilCanCalculate());
            }


            //else, continue to calculate the path.
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



    /* Similar to the above method, but for thresholds rather than cells.
     */
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


    /* Calculate the cost from one cell to the next. 
     * This is the distance from the source cell to the sink cell
     * Input: source cell, sink cell
     * Output: cost
     */
    private float GetCellCost(Cell source, Cell sink){
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return cost;
    }

    /* Calculate the heuristic cost from one cell to the next. 
     * This is the distance from the source cell to the sink cell
     * Input: source cell, sink cell
     * Output: cost
     */
    private float GetCellHeuristic(Cell source, Cell sink){
        float h = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return h;
    }


    /* Same as above, but for thresholds.
     */
    private float GetThresholdCost(Threshold source, Threshold sink){
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return cost;
    }


    /* Same as above, but for thresholds.
     */
    private float GetThresholdHeuristic(Threshold source, Threshold sink){
        float h = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return h;
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
       if (tpathToTake != null)
       {
           foreach (Threshold c in tpathToTake)
           {
               Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
               Vector3 center = c.worldPosition + increment;
               Gizmos.color = Color.yellow;
               Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
           }
       }
    }


}