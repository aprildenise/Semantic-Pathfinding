using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cpalgo : MonoBehaviour {

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
    private int updateRate = 1;

    public bool completedPath; //NEED TO CHANGE

    //for movement
    public float moveSpeed = 1f;
    private int stepsTaken = 0; //aka cell in the pathToTake list
    private float Timer;
    private Vector3 nextCell;
    private Vector3 prevCell;


    //for debugging
    public bool callFindPath = true;

    int cellCount = 0;

    // Use this for initialization
    void Start () {
        if (map == null)
        {
            map = GameObject.Find("MapManager").GetComponent<Map>();
        }

        //setups
        hasHaltedMovement = false;
        hasHaltedCalculating = false;
        completedPath = true;
    }

    public void FindPath()
    {

        if (goal != null)
        {
            completedPath = false;
            pathToTake = FindCellPath(transform.position, goal.position);
            Debug.Log(cellCount);

        }
    }
    void LateUpdate()
    {
        if (hasFoundPath && !hasHaltedMovement && !completedPath)
        {
            Timer += Time.deltaTime * moveSpeed;

            //error checking
            //if (stepsTaken > pathToTake.Count - 1)
            //{
            //    return;
            //}

            //check if the agent is at its goal
            if (transform.position == goalPosition)
            {
                completedPath = true;
                stepsTaken = 2;
                hasFoundPath = false;
                Debug.Log("path is finised!");
                return;
            }



            //move agent to the cell it needs to be in
            if (pathToTake == null)
            {
                return;
            }

            if (pathToTake.Count < 3)
            {
                completedPath = true;
                stepsTaken = 2;
                hasFoundPath = false;
                return;
            }

            nextCell = pathToTake[stepsTaken].worldPosition;
            if (prevCell != null)
            {
                if (nextCell == prevCell)
                {
                    stepsTaken++;
                    return;
                }
            }


            if (transform.position != nextCell)
            {
                Debug.Log("still have to move");
                transform.position = Vector3.Lerp(transform.position, nextCell, Timer);
                return;
            }


            //the agent is at the cell it needs to be in
            if (transform.position == nextCell)
            {
                //check if we can start at this cell

                Timer = 0;
                pathToTake = null;
                stepsTaken = 2;
                prevCell = nextCell;

                pathToTake = FindCellPath(nextCell, goalPosition);
                if (pathToTake == null)
                {
                    completedPath = true;
                    stepsTaken = 0;
                    hasFoundPath = false;
                    Debug.Log("path is finised!");
                    return;
                }
                Debug.Log("refershing path");
                return;
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

    /* Helper function used to move the agent.
     */



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
    public void HaltCalculating(bool status)
    {
        hasHaltedCalculating = status;
    }

    /* Change the rate at which the agent recalculates the path.
     * Rate is equvalent to the number of cells the agent travels in
     * order to get to its goal.
     * Input: desired status.
     */
    public void ChangeUpdateRate(int rate)
    {
        updateRate = rate;
    }


    /* Helper function to allow the agent to halt path calculation.
     */
    private IEnumerator WaitUntilCanCalculate()
    {
        yield return new WaitUntil(() => hasHaltedCalculating == false);
    }

    public List<Cell> FindCellPath(Vector3 startPos, Vector3 goalPos)
    {
        Cell startCell = map.CellFromWorldPos(startPos);
        Cell goalCell = map.CellFromWorldPos(goalPos);

        PriorityQueue<Cell> frontier = new PriorityQueue<Cell>();
        cameFrom = new Dictionary<Cell, Cell>();
        costSoFar = new Dictionary<Cell, float>();
        List<Cell> path = null;

        frontier.Enqueue(startCell, 0);
        cameFrom[startCell] = startCell;
        costSoFar[startCell] = 0;
        int temp = 0;
        while (frontier.Count() > 0)
        {
            temp++;
            cellCount++;
            //Check if the agent is able to calculate their path.
            if (hasHaltedCalculating)
            {
                StartCoroutine(WaitUntilCanCalculate());
            }

            if (hasHaltedCalculating){
                StartCoroutine(WaitUntilCanCalculate());
            }

            //else, continue to calculate the path.
            Cell current = frontier.Deqeueue();

            if (current.Equals(goalCell))
            {
                path = RetraceCellPath(startCell, goalCell);
                return path;
            }


            foreach (Edge e in current.edgesToNeighbors)
            {
                Cell neighbor = e.incident;

                float newCost = costSoFar[current] + GetCellCost(current, neighbor);
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
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
    private List<Cell> RetraceCellPath(Cell startCell, Cell goalCell)
    {

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







    /* Calculate the cost from one cell to the next. 
     * This is the distance from the source cell to the sink cell
     * Input: source cell, sink cell
     * Output: cost
     */
    private float GetCellCost(Cell source, Cell sink)
    {
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return cost;
    }

    /* Calculate the heuristic cost from one cell to the next. 
     * This is the distance from the source cell to the sink cell
     * Input: source cell, sink cell
     * Output: cost
     */
    private float GetCellHeuristic(Cell source, Cell sink)
    {
        float h = Vector3.Distance(source.worldPosition, sink.worldPosition);
        return h;
    }


    /* Same as above, but for thresholds.
     */
    private float GetThresholdCost(Threshold source, Threshold sink)
    {
        float cost = Vector3.Distance(source.worldPosition, sink.worldPosition);



        return cost;
    }


    /* Same as above, but for thresholds.
     */
    private float GetThresholdHeuristic(Threshold source, Threshold sink)
    {
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
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));

            }
        }
    }

}
