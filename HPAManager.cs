using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAManager : MonoBehaviour {


    //references
    public Map m;
    public Transform agent;
    public Transform goal;
    //private Renderer renderer;
    public AStarAlgo aStar;
    public ZoneManager zm;


    // used for testing and other debugging
    // Use this for initialization
    void Start () {
        m.GetMapDimensions(); 
        m.BuildCellGrid(.1f);
        m.DefineZones();
        m.FindThresholds();
        m.AddThresholdToZone();
        ThresholdPath(agent.position, goal.position);
        //Tester();
        //m.CreateImage();
       
    }



    void Tester()
    {
        foreach (Threshold t in m.thresholdGraph)
        {
            Debug.Log("Threshold zoneid:" + t.zoneId);
        }
    }


    void ThresholdPath(Vector3 startPos, Vector3 targetPos)
    {
        Cell start = m.CellFromWorldPos(startPos);
        Cell target = m.CellFromWorldPos(targetPos);

        Debug.Log("Starting cell's zone id:" + start.zoneId);
        Debug.Log("Target cell's zone id:" + target.zoneId);

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

        //traverse threshold graph
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
    }


    Vector3 FindThresholdCell(Vector3 cPos)
    {
        Cell c = m.CellFromWorldPos(cPos);

        //define start and goal zones
        Zone Z = new Zone(c.zoneId, GetComponent<Renderer>());

        //distance tracker
        int shortestDist = 2147483647; //holding value



        Cell cThreshold = c;

        //traverse threshold graph
        for (int i = 0; i < Z.thresholds.Count; i++)
        {


            Threshold t = Z.thresholds[i];
            //find target cell for threshold A*
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

        //Zone z = new Zone(start.zoneId, GetComponent<Renderer>());


        //check if target and start are inthe same zone
        //If yes run A*
        if (start.zoneId == target.zoneId)
        {
            //AStarAlgo.FindPath(startPos, targetPos);
            aStar.FindPath(startPos, targetPos);
            return;
        }
        //if not
        //travel from start to start
        Vector3 startTPos = FindThresholdCell(startPos);
        aStar.FindPath(startPos, startTPos);

        //connect start zone threshold to target zone threshold
        ThresholdPath(startPos, targetPos);

        //travel from target threshold to target
        Vector3 targetTPos = FindThresholdCell(targetPos);
        aStar.FindPath(targetTPos, targetPos);

    }



}
