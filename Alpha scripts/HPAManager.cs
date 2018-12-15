using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HPAManager : MonoBehaviour
{


    //references
    public Map m;
    public Transform agent; //temp
    public Transform goal; //temp
    public ZoneManager zm;


    public List<Cell> path; //temp

    // used for testing and other debugging
    // Use this for initialization
    void Start()
    {
        m.InitMap();

    }

    private void LateUpdate()
    {
        //HPAFindPath(agent.position, goal.position);

    }



    //rewrote a clearer version below 
    /*
    void ThresholdPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell start = m.CellFromWorldPos(startPos);
        Cell target = m.CellFromWorldPos(targetPos);

       // Debug.Log("Starting cell's zone id:" + start.zoneId);
       // Debug.Log("Target cell's zone id:" + target.zoneId);

        //define start and goal zones
        //Zone startZ = new Zone(start.zoneId, GetComponent<Renderer>());
        //Zone targetZ = new Zone(target.zoneId, GetComponent<Renderer>());
        Zone startZ = zm.GetZone(start.zoneId);
        Zone targetZ = zm.GetZone(target.zoneId);

        Debug.Log("starting zone: " + startZ.zoneId);
        Debug.Log("ending zone: " + targetZ.zoneId);

        //distance tracker
        int shortestDistStart = 2147483647; //holding value
        int shortestDistEnd = 2147483647; //holding value

        //Find threshold cloest to target
        Cell startThreshold = start;
        Cell targetThreshold = target;

        foreach (Threshold t in m.thresholdGraph)
        {
            Debug.Log("Threshold zoneid:" + t.zoneId);
        }

        //traverse threshold graph
        Vector3 agentThreshold = FindThresholdCell(startPos);
        Vector3 goalThreshold = FindThresholdCell(startPos);

        for (int i = 0; i < startZ.thresholds.Count; i++)
        {
            Threshold t = startZ.thresholds[i];

            //find target cell for threshold A*
            foreach (Edge e in t.edgesToNeighbors)
            {
                Cell neighbor = e.incident; //new code
                                            // To determine starting cell
                if (neighbor.zoneId == start.zoneId)
                {
                    //temp is the distance fo the neighbor cell compared to the shortestDist
                    int tempDist = GetDistance(neighbor, start);
                    if (tempDist <= shortestDistStart)
                    {
                        shortestDistStart = tempDist;
                        startThreshold = neighbor;
                        Debug.Log("Starting threshold found. Zone id is: " + startThreshold.zoneId);
                        break;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        //find goal threshold cell
        for (int i = 0; i < targetZ.thresholds.Count; i++)
        {


            Threshold t = targetZ.thresholds[i];

            //find target cell for threshold A*
            foreach (Edge e in t.edgesToNeighbors)
            {
                Cell neighbor = e.incident; //new code
                                            // To determine starting cell
                if (neighbor.zoneId == target.zoneId)
                {
                    //temp is the distance fo the neighbor cell compared to the shortestDist
                    int tempDist = GetDistance(neighbor, target);
                    if (tempDist <= shortestDistEnd)
                    {
                        shortestDistEnd = tempDist;
                        targetThreshold = neighbor;
                    }
                }
                else
                {
                    continue;
                }
            }
        }
        aStar.FindPath(startThreshold.worldPosition, targetThreshold.worldPosition);

        //Search the zones between thresholds in the found threshold path
        List<Cell> thresholdPath = aStar.retrievesPath(startThreshold, targetThreshold);
        for (int i = 0; i < thresholdPath.Count - 1; i++)
        {
            //find the path of the zones
            aStar.FindPath(thresholdPath[i].worldPosition, thresholdPath[i + 1].worldPosition);
        }
        
    }
    */

    //cleaner version of hte cod above
    /* Given an agent's position and its goal position, find the path the
     * agent must take in terms of the threshold graph
     * Input: starting position, goal position
     * Output: !!SHOULD BE THE PATH !!

    void ThresholdPath(Vector3 startPos, Vector3 targetPos)
    {

        //Finding starting and ending thresholds
        //Vector3 startThreshold = FindThresholdCell(startPos);
        //Vector3 targetThreshold = FindThresholdCell(targetPos);

        //find the threshold we have to start at and the threshold we have to reach to get to the goal
        Threshold startThreshold = FindThresholdCell(startPos);
        //Debug.Log("ThresholdPath: start threshold found");
        //Threshold goalThreshold = FindThresholdCell(targetPos);
        //Debug.Log("ThresholdPath: goal threshold found");

        //Finding Path
        //Vector3 startingPosition = startThreshold.worldPosition;
        //Vector3 goalPosition = goalThreshold.worldPosition;
        //aStar.FindPathT(startThreshold, goalThreshold);

        //Search the zones between thresholds in the found threshold path

        /*
        List<Cell> thresholdPath = aStar.RetracePathT(startThreshold, targetThreshold);
        for (int i = 0; i < thresholdPath.Count - 1; i++)
        {
            //find the path of the zones
            aStar.FindPath(thresholdPath[i].worldPosition, thresholdPath[i + 1].worldPosition);
        }



    }
*/



    /* Given a position, find the closest threshold to that position
     * Input: starting position and goal position
     * Output: the closest threshold

    private Threshold FindThresholdCell(Vector3 start)
    {

        //find the zone that belongs to this cell to get its thresholds
        Cell startingCell = m.CellFromWorldPos(start);
        //Cell goalCell = m.CellFromWorldPos(goal);
        Zone Z = zm.GetZone(startingCell.zoneId);

        //distance tracker
        int shortestDist = 2147483647; //holding value


        //traverse the threshold graph to find the appropriate threshold
        Threshold cThreshold = null;
        for (int i = 0; i < Z.thresholds.Count; i++)
        {

            //find the closest threshold to the agent and to the goal. this is the most approriate threshold
            Threshold t = Z.thresholds[i];

            //Debug.Log("This threshold is at:" + t.worldPosition);

            int tempDist = (int)Vector3.Distance(t.worldPosition, startingCell.worldPosition);
            if (tempDist <= shortestDist)
            {
                cThreshold = t;
                shortestDist = tempDist;
            }
            else
            {
                continue;
            }




            /* 
            foreach (ThresholdEdge e in t.tedgesToNeighbors)
            {
                Threshold neighbor = e.incident; //new code
                                            // To determine starting cell
                if (neighbor.zoneId == startingCell.zoneId)
                {
                    //temp is the distance fo the neighbor cell compared to the shortestDist
                    //int tempDist = GetDistance(neighbor, c);
                    int tempDist = (int)Vector3.Distance(neighbor.worldPosition, startingCell.worldPosition);
                    if (tempDist <= shortestDist)
                    {
                        shortestDist = tempDist;
                        cThreshold = neighbor;
                    }
                }
                else
                {
                    continue;
                }
            }





        }
        return cThreshold;
    }
*/
    // //calculate distance between two cells
    // int GetDistance(Cell cellA, Cell cellB)
    // {


    //     int dstX = Mathf.Abs(cellA.gridPositionX - cellB.gridPositionX);
    //     int dstZ = Mathf.Abs(cellA.gridPositionZ - cellB.gridPositionZ);

    //     if (dstX > dstZ)
    //         return 14 * dstZ + 10 * (dstX - dstZ);
    //     return 14 * dstX + 10 * (dstZ - dstX);
    // }



    /* Given the position of the agent and its goal, use the hpa algo
     * to find a path from the agent to the goal
     * Input: starting position, ending position
     * Output: !!SHOULD BE THE PATH FOUND!!
     
    public void HPAFindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell start = m.CellFromWorldPos(startPos);
        Cell target = m.CellFromWorldPos(targetPos);

        //check if target and start are in the same zone
        //If yes run A*
        if (start.zoneId == target.zoneId)
        {
            Debug.Log("Agent and target are in the same zone.");
            aStar.FindPath(startPos, targetPos);
            return;
        }
        //if not
        //travel from the starting position to the appropriate threshold first
        Threshold startT = FindThresholdCell(startPos);
        Vector3 startTPos = startT.worldPosition;
        aStar.FindPath(startPos, startTPos);

        //then, search the graph of thresholds to find which zones to go through
        //connect start zone threshold to target zone threshold
        ThresholdPath(startPos, targetPos);

        //travel from target threshold to target
        /*
        Vector3 targetTPos = FindThresholdCell(targetPos);
        aStar.FindPath(targetTPos, targetPos);



    }
    */

    /* An alternate implementation for hpa
    // */
    //public List<Cell> HPAAlt(Vector3 start, Vector3 goal)
    //{
    //    List<Cell> finalPath = new List<Cell>();

    //    //add the start position and the goal position to the threshold graph
    //    //by finding which threshold is close to the two positions
    //    Threshold thresholdStart = FindNeartestThreshold(start, goal);
    //    Threshold thresholdGoal = FindNeartestThreshold(goal, start);

    //    //find a path of thresholds that exist starting from thresholdStart and to thresholdGoal
    //    List<Threshold> thresholdPath = aStar.FindPathT(thresholdStart, thresholdGoal);

    //    //using all these thresholds, find a path to the goal from the start with the help of astar
    //    //first node
    //    finalPath.Add(m.CellFromWorldPos(start));
    //    //start to first threshold
    //    List<Cell> temp = aStar.FindPath(start, thresholdStart.worldPosition);
    //    finalPath.AddRange(temp);
    //    //between thresholds 
    //    for (int i = 1; i < thresholdPath.Count; i++)
    //    {
    //        //get the last position from the final path, this will be the new "start"
    //        Vector3 newStart = thresholdPath[i-1].worldPosition;
    //        //the threshold will be the new goal
    //        Vector3 newGoal = thresholdPath[i].worldPosition;
    //        temp = aStar.FindPath(newStart, newGoal);

    //        //add this path to the finalpath
    //        finalPath.AddRange(temp);
            
    //    }
    //    //final threshold to the end
    //    temp = aStar.FindPath(thresholdPath[thresholdPath.Count - 1].worldPosition, goal);
    //    finalPath.AddRange(temp);

    //    //done
    //    path = finalPath; //temp
    //    return finalPath;


    //}


    /* Given a position, find the threshold that is near to both the beginning
     * position and the goal position
     * Input: position in question, goal position
     * Output: threshold that is nearest to those positions
     */

    public Threshold FindNeartestThreshold(Vector3 position, Vector3 goal)
    {
        Vector3 p = position;
        Vector3 g = goal;


        Threshold threshold = null;

        //find the zone that the starting position belongs to
        Cell cell;
        cell = m.CellFromWorldPos(p);

        //traverse through the threshold graph to find the appropriate threshold
        //the appropriate threshold is:
        //in the same zone as or around the cell, and close to the goal (if possible)
        float distanceFromPosition = Mathf.Infinity;
        foreach (Threshold t in m.thresholdGraph)
        {


            //find if this threshold is close to the starting position
            if (t.tzoneID == cell.zoneId || t.zoneId == cell.zoneId)
            {
                //this threshold is in or connected with the same zone as the current position
                //we know it is close to the starting position

                //see if this threshold is close to the goal
                float distance = Vector3.Distance(t.worldPosition, g);
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