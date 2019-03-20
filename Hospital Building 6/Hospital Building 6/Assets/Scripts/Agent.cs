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
    private List<Threshold> agentThresholds; //temp

    //the goal the agent wants to move to
    public Transform goal;
    public Vector3 goalPosition; //NEED TO CHANGE

    //current signals to update or pause pathfinding
    [HideInInspector]
    public bool isCalculatingPath; //agent is calculating a path
    [HideInInspector]
    public bool hasFoundPath; //agent has finished calculating a path
    private bool hasHaltedMovement; //agent has halted following the path it has found
    private bool hasHaltedCalculating; //agent has halted calculating a path;
    private int updateRate = 10;

    public bool completedPath; //NEED TO CHANGE

    //for movement
    public float moveSpeed = 1f;
    private int stepsTaken = 0; //aka cell in the pathToTake list
    private float Timer;
    private Vector3 nextCell;
    private Vector3 prevCell;


    //for debugging
    public bool callFindPath = true;
    


    void Start(){
        if (map == null){
            map = GameObject.Find("MapManager").GetComponent<Map>();
        }

        //setups
        hasHaltedMovement = false;
        hasHaltedCalculating = false;
        completedPath = true;

        //debugging
        //if (goal != null)
        //{
        //    pathToTake = NewFindHPAPath(transform.position, goalPosition);
        //}

    }



    //does not work
    //private void CopyThresholdGraph()
    //{
    //    List<Threshold> mapThresholds = map.thresholdGraph;
    //    List<Threshold> temp = new List<Threshold>();

    //    foreach (Threshold t in mapThresholds)
    //    {
    //        //copy the data EXACTLY from the mapThresholds into the temp list
    //        Threshold copy = new Threshold(t.tzoneID, t.zoneId, t.iCost, t.worldPosition, t.gridPositionX, t.gridPositionZ, t.cellSize);
    //        temp.Add(copy);

    //        //for each thresholds, also copy their neighbors
    //        List<ThresholdEdge> possibleNeighbors = new List<ThresholdEdge>();
    //        foreach (ThresholdEdge edge in t.tedgesToNeighbors)
    //        {
    //            ThresholdEdge newEdge = new ThresholdEdge(edge.origin, edge.incident, edge.weight);
    //            possibleNeighbors.Add(newEdge);
    //        }
    //        copy.TAssignNeighbors(possibleNeighbors);

    //    }

    //    //finished, add the temp into the global variable
    //    agentThresholds = temp;
    //}



    //for debugging
    //private void Update()
    //{
        
    //    if (callFindPath)
    //    {
    //        FindPath();
    //    }
    //}


    /* A public method that other classes can call. 
     * Calls FindHPAPath and finds a path from the current agent's location to its goal location
     */
    public void FindPath()
    {
        if (goal != null)
        {
            completedPath = false;
            pathToTake = NewFindHPAPath(transform.position, goal.position);
        }

        //added because goal is a transform. !!SHOULD CHANGE LATER!!
        else if (goalPosition != null)
        {
            completedPath = false;
            pathToTake = NewFindHPAPath(transform.position, goalPosition);
        }
    }




    /* Used for moving the agent from its origin to its goal 
  * if it has found a path and is allowed to move.
  */
    void LateUpdate()
    {
        if (hasFoundPath && !hasHaltedMovement && !completedPath)
        {
            Timer += Time.deltaTime * moveSpeed;
            //error checking
            if (stepsTaken > pathToTake.Count - 1)
            {
                return;
            }
            nextCell = pathToTake[stepsTaken].worldPosition;
            if (transform.position != nextCell)
            {
                transform.position = Vector3.Lerp(transform.position, nextCell, Timer);
            }


            //changes here
            //else if (transform.position == nextCell)
            //{
            //    pathToTake = null;
            //    pathToTake = NewFindHPAPath(nextCell, goalPosition);
            //    return;
            //}


            else
            {
                if (stepsTaken < pathToTake.Count - 1)
                {
                    stepsTaken++;
                    CheckCell();
                    if (updateRate != 0)
                    {
                        if (stepsTaken % updateRate == 0 && stepsTaken != 0)
                        {
                            //update the path
                        }
                    }
                }
                else
                {
                    completedPath = true;
                    stepsTaken = 0;
                    hasFoundPath = false;
                    //Debug.Log("agent has completed its path");
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


    //TEMP FOR NOW. FIX UP LATER
    //NEW VERSTION OF HPA BELOW
    private List<Cell> NewFindHPAPath(Vector3 startPosition, Vector3 goalPosition)
    {
        //setups 
        hasFoundPath = false;
        isCalculatingPath = true;
        List<Cell> finalPath = new List<Cell>();

        //get cell versions of the start and goal position
        Cell startCell = map.CellFromWorldPos(startPosition);
        Cell goalCell = map.CellFromWorldPos(goalPosition);

        //error checking
        if (!startCell.isWalkable)
        {
            startCell = map.CellFromWorldPosSearch(startPosition);
        }
        if (!goalCell.isWalkable)
        {
            goalCell = map.CellFromWorldPosSearch(goalPosition);
        }


        //check if the goalCell is the same as the startCell
        if (startCell.worldPosition == goalCell.worldPosition)
        {
            //we don't need to find a path 
            return null;
        }

        //check if the goalCell is in the same zone has the startCell
        //use this path as the final path 
        if (map.GetZone(startCell.zoneId).zoneId == map.GetZone(goalCell.zoneId).zoneId)
        {
            //simply use the find cell path function to get to the goal
            List<Cell> path = FindCellPath(startCell, goalCell);

            //error checking
            if (path != null)
            {
                finalPath.AddRange(path);
            }

            pathToTake = finalPath;
            hasFoundPath = true;
            isCalculatingPath = false;
            return finalPath;
        }

        //add the startCell and the goalCell to the agentThreshold graph
        Threshold startThresold = AddStartThreshold(startCell);
        Threshold goalThreshold = AddGoalThreshold(goalCell);

        //Debug.Log("start and goal thresholds have been created.");
        //Debug.Log("these thresholds are at:" + startThresold.worldPosition + " and at:" + goalThreshold.worldPosition);

        //find the threshold path using these new thresholds
        List<Threshold> thresholdPath = FindThresholdPath(startThresold, goalThreshold);
        if (thresholdPath == null)
        {
            //Debug.Log("Big fucking yike");
            return null;
        }


        tpathToTake = thresholdPath;

        //using the threshold path, find the lower level cell path
        if (thresholdPath.Count >= 2)
        {
            for (int i = 1; i < thresholdPath.Count; i++)
            {
                //Get the position of the previous threshold. This will be our new "start"
                Cell newStart = map.CellFromThreshold(thresholdPath[i - 1]);

                //Get the position of the current threshold. This will be our new "goal"
                Cell newGoal = map.CellFromThreshold(thresholdPath[i]);

                //find the cell path
                List<Cell> path = FindCellPath(newStart, newGoal);
                if (path != null)
                {
                    finalPath.AddRange(path);
                }
            }
        }

        //done!
        Debug.Log("path found");
        DeleteGoalThreshold(goalThreshold);
        pathToTake = finalPath;
        hasFoundPath = true;
        isCalculatingPath = false;
        return finalPath;

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


        //check if the start is already in the same zone as the goal
        Cell startCell = map.CellFromWorldPos(start);
        Cell goalCell = map.CellFromWorldPos(goal);

        //more debugging
        if (!startCell.isWalkable)
        {
            startCell = map.CellFromWorldPosSearch(start);
        }
        //if (!goalCell.isWalkable)
        //{
        //    goalCell = map.CellFromWorldPosSearch(goal);
        //}

        if (map.GetZone(startCell.zoneId).zoneId == map.GetZone(goalCell.zoneId).zoneId)
        {
            //simply use the find cell path function to get to the goal
            List<Cell> tmp = FindCellPath(startCell, goalCell);
            //temporarily here for now
            if (tmp != null)
            {
                finalPath.AddRange(tmp);
            }
            
            pathToTake = finalPath;
            hasFoundPath = true;
            isCalculatingPath = false;
            return finalPath;
        }

        //add the start position and the goal position to the threshold graph
        //by finding which threshold is close to the two positions.
        Threshold thresholdStart = FindNeartestThreshold(start, goal);
        Threshold thresholdGoal = FindNeartestThreshold(goal, start);

        //find a path of thresholds starting from thresholdStart and to thresholdGoal.
        List<Threshold> thresholdPath = FindThresholdPath(thresholdStart, thresholdGoal);
        tpathToTake = thresholdPath;

        //using all these thresholds, find a path to the goal from the start with the help of astar.
        //path from start threshold to between threshold.
        Cell nextGoal = map.CellFromThreshold(thresholdStart);



        List<Cell> temp = FindCellPath(startCell, nextGoal);

        //debugging
        //if (temp == null)
        //{
        //    Debug.Log("nextGoal:" + nextGoal.worldPosition);
        //    Debug.Log("startCell:" + startCell.worldPosition);
        //    Debug.Log("start cell is " + startCell.isWalkable);
        //    Debug.Log("nextgoal is " + nextGoal.worldPosition);
        //}


        finalPath.AddRange(temp);

        //Debug.Log(thresholdPath.Count);

        //path between the thresholds.
        for (int i = 1; i < thresholdPath.Count; i++)
        {

            //get the last position from the final path, this will be the new "start."
            Cell newStart = map.CellFromThreshold(thresholdPath[i-1]);
            
            //the threshold will be the new goal.
            Cell newGoal = map.CellFromThreshold(thresholdPath[i]);

            temp = FindCellPath(newStart, newGoal);

            //add this path to the finalpath.
            finalPath.AddRange(temp);

        }
        //path from final threshold to goal.


        nextGoal = map.CellFromThreshold(thresholdPath[thresholdPath.Count - 1]);
        temp = FindCellPath(nextGoal, goalCell);
        if (temp != null)
        {
            finalPath.AddRange(temp);
        }
        

        //done!
        pathToTake = finalPath;
        hasFoundPath = true;
        isCalculatingPath = false;


        return finalPath;


    }


    /* Create a new threshold using the starting Cell that will be used to find a path
     * on the agentThreshold graph.
     * Input: Start cell
     * Output: Threshold made from start cell. used for deletion when done
     */
    private Threshold AddStartThreshold(Cell startCell)
    {
        //convert the startCell into a threshold
        //only the neighbors, weights, and position are important. The rest is not important
        //and will be given an arbitary value
        Threshold startThreshold = new Threshold(-1, -1, startCell.iCost, startCell.worldPosition, -1, -1, -1);
        startThreshold.belongsTo = gameObject.name;

        //find the neighbors of this new threshold
        Zone startZone = map.GetZone(startCell.zoneId);
        List<ThresholdEdge> possibleNeighbors = new List<ThresholdEdge>();
        foreach (Threshold neighbor in startZone.thresholds)
        {

            //make edges that connect this neighbor and the startThreshold
            float weight = map.SearchForDistance(startThreshold, neighbor);
            ThresholdEdge newEdge = new ThresholdEdge(startThreshold, neighbor, weight);

            //add the new edge to the list of possibleNeighbors
            possibleNeighbors.Add(newEdge);
            

            //debug!!
            //Debug.Log("This zone from the MAIN THRESHOLD GRAPH has this many neighbors:" + startZone.thresholds.Count);

            //find the corresponding threshold in the agentThreshold list
            //we can find this just by checking if they have the same coordinates
            //foreach (Threshold agentThreshold in agentThresholds)
            //{
            //    if (agentThreshold.worldPosition == neighbor.worldPosition)
            //    {

            //    }
            //}

        }

        //add the possible neighbors to the threshold and we're done!
        startThreshold.TAssignNeighbors(possibleNeighbors);
        //agentThresholds.Add(startThreshold);
        return startThreshold;
        
    }


    /* Create a new threshold using the goal Cell that will be used to find a path on
     * the agentThreshold graph
     * Input: goal cell
     * Output: Threshold made from the goal cell. used for deletion when done
     */
    private Threshold AddGoalThreshold(Cell goalCell)
    {
        //convert the goal cell into a threshold.
        //only the neighbors, weights, and position are important. The rest is not
        //and will be given arbitrary values
        Threshold goalThreshold = new Threshold(-1, goalCell.zoneId, goalCell.iCost, goalCell.worldPosition, -1, -1, -1);
        goalThreshold.belongsTo = gameObject.name;

        //find the neighbors of this new threshold
        Zone goalZone = map.GetZone(goalCell.zoneId);
       //List<ThresholdEdge> possibleNeighbors = new List<ThresholdEdge>();

        //debug
        //if (goalZone.thresholds == null)
        //{
        //    Debug.Log("this goal has no neighbors. this is:" + goalZone.zoneId);
        //}

        foreach (Threshold neighbor in goalZone.thresholds)
        {


            //make edges that connect all of these threshold neighbors to the goalThreshold
            float weight = map.SearchForDistance(neighbor, goalThreshold);
            ThresholdEdge newEdge = new ThresholdEdge(neighbor, goalThreshold, weight);
            //add this edge to the agentThreshold
            neighbor.tedgesToNeighbors.Add(newEdge);

            //find the corresponding threshold in the agentThresholds
            //we can find this just by checking if they have the same coordinates
            //foreach (Threshold agentThreshold in agentThresholds)
            //{
            //    if (agentThreshold.worldPosition == neighbor.worldPosition)
            //    {
                 



            //        //continue, since there will only be one corresponding threshold
            //        break;
            //    }
            //}
        }

        //add the threshold to the agentThreshold and we're done!
        //agentThresholds.Add(goalThreshold);
        return goalThreshold;

    }


    /* Delete the goal threshold that was created during the HPA algorithm
     * Input: goalThreshold to be deleted
     */
    private void DeleteGoalThreshold(Threshold goalThreshold)
    {
        //find the goal zone and all the neighbors it is connected to
        Zone goalZone = map.GetZone(goalThreshold.zoneId);
        foreach (Threshold neighbor in goalZone.thresholds)
        {
            //delete the edges that have the goalThreshold as a neighbor.
            //this is actually the last threshold in each list
            List<ThresholdEdge> edges = neighbor.tedgesToNeighbors;
            edges.RemoveAt(edges.Count - 1);
            
        }
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
        Cell goalCell = map.CellFromWorldPos(goal);

        //get the thresholds list from the zone that has this cell.
        int id = currentCell.zoneId;
        Zone zone = map.GetZone(id);

        //find the threshold that is closest to the goal.
        float cost = Mathf.Infinity;
        int i = 0;
        foreach (Threshold t in zone.thresholds){

            //NEW CODE
            Threshold tempThreshold = new Threshold(-1, currentCell.zoneId, currentCell.iCost, currentCell.worldPosition, currentCell.gridPositionX, currentCell.gridPositionZ, currentCell.cellSize);
            float weight = map.SearchForDistance(tempThreshold, t);
            float temp = weight + GetICostFromThreshold(t) + GetCellCost(goalCell, t);

            //float temp = GetCellCost(goalCell, t) + GetCellCost(currentCell, t) + GetICostFromThreshold(t);
            if (temp <= cost){

                cost = temp;
                threshold = t;
                i++;
            }
        }

        return threshold;
    }



    ///* Add the starting position to the threshold graph. This is done by creating a new threshold
    // * at the starting position, and then adding its necessary neighbors that exist in the graph.
    // * Input: Starting position cell
    // * Output: Threshold made at starting position for deletion
    // */
    //private Threshold AddStartToThresholds(Cell start)
    //{
    //    Threshold startThreshold = null;

    //    //find the zone that the start position is in
    //    Zone startZone = map.GetZone(start.zoneId);

    //    //create a new threshold and connect it to the other thresholds that belong to this zone
    //    Threshold temp = new Threshold(-1, start.zoneId, start.iCost, start.worldPosition, start.gridPositionX, start.gridPositionZ, start.cellSize);
    //    List<ThresholdEdge> possibleNeighbors = new List<ThresholdEdge>();
    //    foreach (Threshold threshold in startZone.thresholds)
    //    {
    //        //float weight = map.SearchForDistance(temp, threshold);
    //        ThresholdEdge e = new ThresholdEdge(temp, threshold, 0);
    //        if (!possibleNeighbors.Contains(e))
    //        {
    //            possibleNeighbors.Add(e);
    //        }

    //    }

    //    temp.TAssignNeighbors(possibleNeighbors);
    //    startThreshold = temp;

    //    return startThreshold;
    //}


    ///* Add the goal Cell to the threshold graph. 
    // */ 
    //private Threshold AddGoalToThresholds(Cell goal)
    //{
    //    Threshold goalThreshold = null;

    //    return goalThreshold;
    //}






    /* Find a path from the agent's starting location to the desired goal
     * location. This uses the A* algorithm.
     * Input: start location, goal location.
     * Output: the calculated path as a list of cells the agent must visit to
     * get to the goal.
    */
    private List<Cell> FindCellPath (Cell startCell, Cell goalCell){


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


            foreach(Edge e in current.edgesToNeighbors){
                Cell neighbor = e.incident;

                float newCost = costSoFar[current] + GetCellCost(current, neighbor);
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]){
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + GetCellHeuristic(neighbor, goalCell) + GetICostFromCell(neighbor);
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

            //check if the agent can calculate on this threshold
            if (current.belongsTo != null)
            {
                if (!current.belongsTo.Equals(gameObject.name))
                {
                    continue;
                }
            }

            if (current.Equals(goalThreshold))
            {
                path = RetraceThresholdPath(startThreshold, goalThreshold);
                break;
            }

            foreach (ThresholdEdge e in current.tedgesToNeighbors)
            {
                Threshold neighbor = e.incident;

                //check if the agent can calculate on this threshold
                if (neighbor.belongsTo != null)
                {
                    if (!neighbor.belongsTo.Equals(gameObject.name))
                    {
                        continue;
                    }
                }


                //CHANGED!!
                //float newCost = tcostSoFar[current] + GetThresholdCost(current, neighbor);
                float newCost = tcostSoFar[current] + e.weight;

                if (!tcostSoFar.ContainsKey(neighbor) || newCost < tcostSoFar[neighbor])
                {
                    tcostSoFar[neighbor] = newCost;
                    float priority = newCost + GetThresholdHeuristic(neighbor, goalThreshold) + GetICostFromThreshold(neighbor);
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


    /* Get the Influence Cost from the given cell.
     * Input: Cell in question
     * Output: influence cost of that cell
     */
    private float GetICostFromCell(Cell cell)
    {
        float cost = cell.iCost;
        //Debug.Log("this is cell:" + cost);
        return cost;
    }


    /* Get the Influence Cost from the given threshold.
     * Input: Threshold in question
     * Output: influence cost of that threshold
     */
    private float GetICostFromThreshold(Threshold threshold)
    {
        float cost = threshold.iCost;
        //Debug.Log("this is threshold:" + cost);
        return cost;
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


    //for debugging
    private void OnDrawGizmos()
    {

        if (pathToTake != null)
        {
            foreach (Cell c in pathToTake)
            {
                Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
                Vector3 center = c.worldPosition + increment;
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));

            }
        }
        //if (tpathToTake != null)
        //{
        //    foreach (Threshold c in tpathToTake)
        //    {
        //        Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
        //        Vector3 center = c.worldPosition + increment;
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
        //    }
        //}

        ////another way to draw the paths
        //if (tpathToTake != null)
        //{
        //    for (int i = 1; i < tpathToTake.Count; i++)
        //    {
        //        Gizmos.color = Color.black;
        //        Gizmos.DrawLine(tpathToTake[i - 1].worldPosition, tpathToTake[i].worldPosition);
        //    }
        //}
    }


}