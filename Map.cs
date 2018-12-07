using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Drawing;
using System;
using System.Drawing.Imaging;

public class Map : MonoBehaviour
{

    //globals
    private int mapWidth;
    private int mapHeight;
    private int gridWidth; //number of cells across
    private int gridHeight; //number of cells down
    private Vector3 topLeftPos; //world position of the top left corner of the map

    public Cell[,] grid; //2d grid of cells that cover the map
    public List<Threshold> thresholdGraph;

    //references
    public ZoneManager zm;

    //used to find the dimensions of the map
    public Renderer topLeft;
    public Renderer topRight;
    public Renderer bottomRight;


    private void Awake(){
        InitMap();
    }

    /* Initialize the map. This includes: 
     * initializing the cell grid, defining zones, finding thresholds, building the threshold map
     */
    public void InitMap()
    {
        GetMapDimensions();
        BuildCellGrid(.1f);
        DefineZones();
        FindThresholds();
        AddThresholdsToZone();
        //CreateImage();
    }



    /* Determine the entire size of the map using the corners of the map
    */
    public void GetMapDimensions()
    {
        //use the Renderers of the corners to find where they are in world coordinates

        float approxLeft = (topLeft.bounds.center.x - (topLeft.bounds.size.x / 2));
        float approxRight = (topRight.bounds.center.x + (topRight.bounds.size.x / 2));
        float approxBottom = (bottomRight.bounds.center.z - (bottomRight.bounds.size.z / 2));
        float approxTop = (topLeft.bounds.center.z + (topLeft.bounds.size.z / 2));

        //use the world coordinates to get the width and the height of the map
        mapWidth = (int)Mathf.Round(Mathf.Abs(approxLeft - approxRight));
        mapHeight = (int)Mathf.Round(Mathf.Abs(approxTop - approxBottom));
        topLeftPos = new Vector3(approxLeft, topLeft.bounds.center.y, approxTop);

    }



    /* Build a 2d grid of cells that cover the entire map
     * Input: size of the cell in respect to world coordinates
     */
    public void BuildCellGrid(float cellSize)
    {

        //calculate how many cells we can fit across the map
        int numXCells = Mathf.RoundToInt(mapWidth / cellSize);
        int numZCells = Mathf.RoundToInt(mapHeight / cellSize);
        gridHeight = numZCells;
        gridWidth = numXCells;

        //init the 2d grid of cells by iterating through the entire map
        grid = new Cell[numZCells, numXCells];
        for (int z = 0; z < numZCells; z++)
        {
            for (int x = 0; x < numXCells; x++)
            {
                //get the positon of this cell we want to init
                Vector3 increment = new Vector3((cellSize * x) + (cellSize / 2f), 0, -1 * ((cellSize * z) + (cellSize / 2f)));
                Vector3 worldPos = topLeftPos + increment;

                //add it to the cell grid
                grid[z, x] = new Cell(worldPos, x, z, cellSize);
            }
        }


        //build all possible edges and neighbors for each cell
        for (int z = 0; z < numZCells; z++)
        {
            for (int x = 0; x < numXCells; x++)
            {
                Cell c = grid[z, x];
                List<Edge> possibleEdges = new List<Edge>();

                //neighbors that are to the left and right of the current cell
                for (int horizontal = -1; horizontal <= 1; horizontal++)
                {
                    //neighbors that are to the up and down of the current cell
                    for (int vertical = -1; vertical <= 1; vertical++)
                    {
                        //look at the center cell, which is the cell in question
                        //skip this iteration
                        if (horizontal == 0 && vertical == 0)
                        {
                            continue;
                        }
                        int checkX = c.gridPositionX + horizontal;
                        int checkZ = c.gridPositionZ + vertical;

                        //if the neighbor in question is within the bounds of the actual worldGrid
                        if ((checkX >= 0 && checkX < gridWidth) && (checkZ >= 0 && checkZ < gridHeight))
                        {
                            if (grid[checkZ, checkX].isWalkable)
                            {
                                //it is a valid neighbor. add the neighor that is at the coordinates
                                //to the list
                                Edge edge = new Edge(c, grid[checkZ, checkX], 1);
                                possibleEdges.Add(edge);
                            }
                        }
                    }
                }
                //done. add the neighbors to the cell
                c.AssignNeighbors(possibleEdges);
            }
        }

    }


    /* Given a list of zones, find all the cells that belong to that zone
     * and assign them to that zone
     */
    public void DefineZones()
    {
        List<Zone> zones = zm.InitZones();
        foreach (Zone z in zones)
        {


            //convert the 4 corners of the zone to cells
            Cell topLeftCell = CellFromWorldPos(z.topLeft);
            Cell topRightCell = CellFromWorldPos(z.topRight);
            Cell bottomLeftCell = CellFromWorldPos(z.bottomLeft);

            //mark the cells between the above bounds as belonging to this zone
            int id = z.zoneId;
            for (int i = topLeftCell.gridPositionZ; i < bottomLeftCell.gridPositionZ; i++)
            {
                for (int j = topLeftCell.gridPositionX; j < topRightCell.gridPositionX; j++)
                {
                    grid[i, j].zoneId = id;
                }
            }
        }
    }



    /* Within each zone, find the thresholds that connect two different zones together
     */
    public void FindThresholds()
    {

        //table to see if we have visited a cell
        bool[,] visited = new bool[gridHeight, gridWidth];

        int leftBound = 0;
        int rightBound = gridWidth;
        int bottomBound = gridHeight;
        int topBound = 0;
        bool validStart = false;
        Cell startCell;

        //find a walkable random cell to start at
        do
        {
            int randX = UnityEngine.Random.Range(leftBound, rightBound);
            int randZ = UnityEngine.Random.Range(bottomBound, topBound);
            startCell = grid[randZ, randX];
            if (startCell.isWalkable)
            {
                validStart = true;
            }
        } while (!validStart);

        //start floodfilling through the map to find the thresholds
        List<Threshold> thresholdsList = new List<Threshold>();
        thresholdsList = FindThresholdSearch(visited, startCell, thresholdsList);

        //build the threshold graph with all the thresholds we have found
        BuildThresholdGraph(thresholdsList);

    }


    /* Given a visited array and a starting cell, floodfill traverse through
     * the map in order to find all the valid thresholds between the zones
     * Input: visited array for traversing, starting cell, list where thresholds will be places
     * Output: list of thresholds that were found
     */
    public List<Threshold> FindThresholdSearch(bool[,] visited, Cell start, List<Threshold> thresholdsList)
    {
        Queue<Cell> s = new Queue<Cell>();
        s.Enqueue(start);
        //traverse through the map
        while (s.Count != 0)
        {

            Cell cell = s.Dequeue();

            int x = cell.gridPositionX;
            int z = cell.gridPositionZ;

            //if we have already looked at this cell, we do not need to check it again
            if (visited[z,x])
            {
                continue;
            }

            visited[cell.gridPositionZ, cell.gridPositionX] = true;
            foreach (Edge e in cell.edgesToNeighbors)
            {
                Cell c = e.incident;
                //check if the cell can be a possible threshold
                if (c.zoneId < cell.zoneId)
                {
                    if (!c.threshold && !cell.threshold)
                    {
                        if (c.zoneId == -1)
                        {
                            //Debug.Log("Yikes! this id is -1! It is at:" + c.worldPosition);
                        }
                        c.threshold = true;
                        Threshold t = new Threshold(cell.zoneId, c.zoneId, c.worldPosition, c.gridPositionX, c.gridPositionZ, c.cellSize);
                        thresholdsList.Add(t);
                    }
                }
            }

            //add the neighbors to the queue so we can traverse
            foreach (Edge e in cell.edgesToNeighbors)
            {
                if (!visited[e.incident.gridPositionZ, e.incident.gridPositionX])
                {
                    s.Enqueue(e.incident);
                }
            }
        }

        //done. return the list of all possible thresholds
        return thresholdsList;
    }



    /* Given a list of thresholds, connect all the thresholds together in a graph.
     * Also assign thresholds to their respective zones
     * Input: threshold graph
     */
    public void BuildThresholdGraph(List<Threshold> thresholdsList)
    {
        //look at all the thresholds in the list. 
        //If threshold A and B have zones in common, then connect them
        for (int i = 0; i < thresholdsList.Count; i++)
        {

            Threshold threshold = thresholdsList[i];
            List<ThresholdEdge> possibleNeighbors = new List<ThresholdEdge>();

            for (int j = 0; j < thresholdsList.Count; j++)
            {
                if (j == i) //cannot connect with one's self
                {
                    continue; 
                }

                int thresholdAID1 = thresholdsList[i].tzoneID; //threshold A's IDs
                int thresholdAID2 = thresholdsList[i].zoneId;
                int thresholdBID1 = thresholdsList[j].tzoneID; //threshold B's IDS
                int thresholdBID2 = thresholdsList[j].zoneId;

                if (thresholdAID1 == -1 || thresholdAID2 == -1 || thresholdBID1 == -1 || thresholdBID2 == -1)
                {
                    //some weird edge case. will have to investigate later
                    continue; 
                }

                //if they share any ids in common, then B becomes a neighbor of A (A->B)
                if (thresholdAID1 == thresholdBID1 || thresholdAID1 == thresholdBID2 || thresholdAID2 == thresholdBID1 || thresholdAID2 == thresholdBID2)
                {
                    if (thresholdAID1 == thresholdBID1 && thresholdAID2 == thresholdBID2)
                    {
                        //however, we do not want to consider connecting thresholds that are in the same zones and connect the same zones
                        continue;
                    }

                    float weight = Vector3.Distance(thresholdsList[i].worldPosition, thresholdsList[j].worldPosition);
                    ThresholdEdge e = new ThresholdEdge(thresholdsList[i], thresholdsList[j], weight);
                    if (!possibleNeighbors.Contains(e))
                    {
                        possibleNeighbors.Add(e);
                    }
                }
            }

            //done looking at this threshold. add all found neighbors to the threshold cell
            threshold.TAssignNeighbors(possibleNeighbors);

        }

        //finished. add to the global variable
        thresholdGraph = thresholdsList;
    }


    /* Given the threshold list, assign to each zone the thresholds that belong to its zone
     */
    public void AddThresholdsToZone()
    {
        List<Threshold> thresholdList = thresholdGraph;
        //iterate through the list to find the zones the thresholds belong to
        foreach (Threshold t in thresholdList)
        {
            int id = t.zoneId;
            if (id == -1)
            {
                //some weird edge case. !!WE  WILL HAVE TO INVESTIGATE!!
                //Debug.Log("This threshold has -1 id:" + t.worldPosition);
                continue; 
            }
            Zone z = zm.GetZone(id);
            //add the threshold to the zone that the threshold is a part of 
            z.AddThresholdToZone(t);

            //do the same for the zone this threshold is connected to
            id = t.tzoneID;
            if (id == -1)
            {
                //some weird edge case. !!WE  WILL HAVE TO INVESTIGATE!!
                //Debug.Log("This threshold has -1 id:" + t.worldPosition);
                continue; 
            }
            z = zm.GetZone(id);
            //add the threshold to the zone that the threshold is a part of 
            z.AddThresholdToZone(t);
        }
        
    }


    /* Given some world position, give the cell that is at that position
     * Input: Vector3 of the desired cell
     * Output: cell at that position
     */
    public Cell CellFromWorldPos(Vector3 worldPos)
    {

        float approxLeft = (topLeft.bounds.center.x - (topLeft.bounds.size.x / 2));
        float approxRight = (topRight.bounds.center.x + (topRight.bounds.size.x / 2));
        float worldDistance = Mathf.Sqrt((approxRight - approxLeft) * (approxRight - approxLeft));
        float posDistance = Mathf.Sqrt((worldPos.x - approxLeft) * (worldPos.x - approxLeft));
        float percentX = posDistance / worldDistance;

        float approxBottom = (bottomRight.bounds.center.z - (bottomRight.bounds.size.z / 2));
        float approxTop = (topLeft.bounds.center.z + (topLeft.bounds.size.z / 2));
        worldDistance = Mathf.Abs(approxTop - approxBottom);
        posDistance = Mathf.Abs(worldPos.z - approxTop);
        float percentZ = posDistance / worldDistance;

        //make sure the percent does not go beyond 100%
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        //find the indeces of the cell in the grid using the world position
        int x = (Mathf.RoundToInt(gridWidth * percentX)) - 1;
        int z = (Mathf.RoundToInt(gridHeight * percentZ)) - 1;

        if (x < 0)
        {
            x = 0;
        }
        if (z < 0)
        {
            z = 0;
        }


        return grid[z, x];
    }


    public Zone GetZone(int id)
    {
        Zone z = zm.zones[id];
        return z;
    }



    ////for debugging and testing only. used to draw the cell grid

    //public void OnDrawGizmos()
    //{

    //    if (grid != null)
    //    {
    //        for (int i = 0; i < gridWidth; i++)
    //        {
    //            for (int j = 0; j < gridHeight; j++)
    //            {
    //                Cell c = grid[j, i];

    //                //uncomment to see thresholds and iswalkables
    //                if (c.isWalkable && !c.threshold)
    //                {
    //                    Gizmos.color = UnityEngine.Color.clear;
    //                }
    //                else if (c.isWalkable && c.threshold)
    //                {
    //                    Gizmos.color = UnityEngine.Color.blue;
    //                }
    //                else if (!c.isWalkable)
    //                {
    //                    //Gizmos.color = UnityEngine.Color.red;
    //                }




    //                //another way to draw the zones
    //                /*
    //                int temp = c.zoneId % 5;
    //                if (temp == 0)
    //                {
    //                    Gizmos.color = Color.yellow;
    //                }
    //                else if (temp == 1)
    //                {
    //                    Gizmos.color = Color.blue;
    //                }
    //                else if (temp == 2)
    //                {
    //                    Gizmos.color = Color.magenta;
    //                }
    //                else if (temp == 3)
    //                {
    //                    Gizmos.color = Color.cyan;
    //                }
    //                else if (temp == 4)
    //                {
    //                    Gizmos.color = Color.grey;
    //                }
    //                else
    //                {
    //                    Gizmos.color = Color.green;
    //                }
    //                */


    //                Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
    //                Vector3 center = c.worldPosition + increment;
    //                Gizmos.DrawCube(center, new Vector3(c.cellSize, c.cellSize, c.cellSize));
    //            }
    //        }

    //    }
    //}


    /* Color zones, thresholds, and obstacles on an EMGU image
     * and export the image to a folder outside the Unity project
     */
    private void CreateImage()
    {
        Image<Bgr, Byte> img = new Image<Bgr, byte>(gridWidth, gridHeight);
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                if (!grid[i, j].isWalkable)
                {
                    img[i, j] = new Bgr(0, 0, 255);
                }
                else if (grid[i, j].threshold)
                {
                    img[i, j] = new Bgr(255, 0, 0);
                }
                else
                {
                    img[i, j] = new Bgr(0, 255, 255); 
                }
            }
        }


        //Size newSize = new Size(gridWidth * 2, gridHeight * 2);
        //CvInvoke.Resize(img, img, newSize);
        String windowName = "Test Window";
        CvInvoke.NamedWindow(windowName);
        CvInvoke.Imshow(windowName, img);
        img.ToBitmap().Save("filename.png");


    }

}