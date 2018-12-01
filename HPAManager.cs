using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAManager : MonoBehaviour {


    //references
    public Map m;
    public Transform agent; //temp
    public Transform goal; //temp
    public AStarAlgo aStar;
    public ZoneManager zm;

    
    // used for testing and other debugging
    // Use this for initialization
    void Start () {
        m.InitMap();
        
    }

    private void LateUpdate()
    {
        HPAFindPath(agent.position, goal.position);
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
    void ThresholdPath(Vector3 startPos, Vector3 targetPos)
    {
       
        //Finding starting and ending thresholds
        Vector3 startThreshold = FindThresholdCell(startPos);
        Vector3 targetThreshold = FindThresholdCell(targetPos);

        //Finding Path
        aStar.FindPath(startThreshold, targetThreshold);

        //Search the zones between thresholds in the found threshold path
        
        //List<Cell> thresholdPath = aStar.retrievesPath(startThreshold, targetThreshold);
        //for (int i = 0; i < thresholdPath.Count - 1; i++)
        //{
        //    //find the path of the zones
        //    aStar.FindPath(thresholdPath[i].worldPosition, thresholdPath[i + 1].worldPosition);
        //}

    }



    /* Given a position, find the closest threshold to that position
     * Input: starting position 
     * Output: position of the chosen threshold
     */
    private Vector3 FindThresholdCell(Cell c)
    {
  
        //define start and goal zones
        //find the zone that belongs to this cell to get its thresholds
        Zone Z = zm.GetZone(c.zoneId);

        //distance tracker
        int shortestDist = 2147483647; //holding value


        //traverse the threshold graph to find the appropriate threshold
        Cell cThreshold = null;
        for (int i = 0; i < Z.thresholds.Count; i++)
        {

            //find the closest threshold. this is the post approriate threshold
            Threshold t = Z.thresholds[i];
            foreach (Edge e in t.edgesToNeighbors)
            {
                Cell neighbor = e.incident; //new code
                                            // To determine starting cell
                if (neighbor.zoneId == c.zoneId)
                {
                    //temp is the distance fo the neighbor cell compared to the shortestDist
                    int tempDist = GetDistance(neighbor, c);
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
        return cThreshold.worldPosition;
    }

    //calculate distance between two cells
    int GetDistance(Cell cellA, Cell cellB)
    {
        int dstX = Mathf.Abs(cellA.gridPositionX - cellB.gridPositionX);
        int dstZ = Mathf.Abs(cellA.gridPositionZ - cellB.gridPositionZ);

        if (dstX > dstZ)
            return 14 * dstZ + 10 * (dstX - dstZ);
        return 14 * dstX + 10 * (dstZ - dstX);
    }


    public void HPAFindPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell start = m.CellFromWorldPos(startPos);
        Cell target = m.CellFromWorldPos(targetPos);

        //check if target and start are inthe same zone
        //If yes run A*
        if (start.zoneId == target.zoneId)
        {
            Debug.Log("Agent and target are in the same zone.");
            aStar.FindPath(startPos, targetPos);
            return;
        }
        //if not
        //travel from startPos to 
        Vector3 startTPos = FindThresholdCell(start);

        /*
        aStar.FindPath(startPos, startTPos);

        //connect start zone threshold to target zone threshold
        ThresholdPath(startPos, targetPos);
        
        //travel from target threshold to target
        Vector3 targetTPos = FindThresholdCell(targetPos);
        aStar.FindPath(targetTPos, targetPos);
        */

    }

}