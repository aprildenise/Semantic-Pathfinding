using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {



    //globals
    private int mapWidth;
    private int mapHeight;
    private Vector3 topLeftPos; //world position of the top left corner of the map
    public Cell[,] grid; //2d grid of cells that cover the map

    //references
    public ZoneManager zm;
    //used to find the dimensions of the map
    public Renderer topLeft;
    public Renderer topRight;
    public Renderer bottomRight;

    //optional
    private int gridWidth; //number of cells across
    private int gridHeight; //number of cells down


    /* Determine the entire size of the map using the corners of the map
    */
    public void GetMapDimensions()
    {
        //use the Renderers of the corners to find where they are in world coordinates
        float approxLeft = (topLeft.bounds.center.x - (topLeft.bounds.size.x / 2));
        float approxRight = (topRight.bounds.center.x + (topRight.bounds.size.x / 2));
        float approxBottom = (bottomRight.bounds.center.z - (bottomRight.bounds.size.z / 2));
        float approxTop = (topLeft.bounds.center.z + (topLeft.bounds.size.z / 2));

        //use the world coordinates to get the width and the height
        mapWidth = (int)Mathf.Round(Mathf.Abs(approxLeft - approxRight));
        mapHeight = (int)Mathf.Round(Mathf.Abs(approxTop - approxBottom));
        topLeftPos = new Vector3(approxLeft, topLeft.bounds.center.y, approxTop);

    }



    /* Build a 2d grid of cells that cover the entire map
     * Input: size of the cell in respect to world coordinates
     */
    public void BuildCellGrid(float cellSize)
    {

        //calculate the size of a cluster based on the dimensions of a map
        int numXCells = Mathf.RoundToInt(mapWidth / cellSize);
        int numZCells = Mathf.RoundToInt(mapHeight / cellSize);

        //optional
        gridHeight = numZCells;
        gridWidth = numXCells;

        //build the 2d grid of cells
        grid = new Cell[numZCells, numXCells];
        for (int z = 0; z < numZCells; z++)
        {
            for(int x = 0; x < numXCells; x++)
            {
                //get the positon of this cell we want to init
                Vector3 increment = new Vector3((cellSize * x) + (cellSize/2f), 0, -1 * ((cellSize*z) + (cellSize/2f)));
                //Vector3 worldPos = topLeftPos + (Vector3.right * (x * (cellSize + (cellSize / 2)))) + (Vector3.forward * (z * (cellSize + (cellSize / 2))));
                Vector3 worldPos = topLeftPos + increment;
                grid[z,x] = new Cell(worldPos, x, z, cellSize);
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
                        //we're looking at the center cell, which is the cell in question
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
                c.AssignNeighbors(possibleEdges);
            }
        }
        
    }


    /* Given a list of zones, find all the cells that belong to that zone
     * and assign them to that zone
     */
    public void DefineZones()
    {
        List<Zone> zones = zm.FindZoneBounds();
        foreach(Zone z in zones)
        {


            //convert the 4 corners of the zone to cells
            Cell topLeftCell = CellFromWorldPos(z.topLeft);
            Cell topRightCell = CellFromWorldPos(z.topRight);
            Cell bottomLeftCell = CellFromWorldPos(z.bottomLeft);
            //Cell bottomRightCell = CellFromWorldPos(z.bottomRight);

            //mark the cells between the above bounds as belonging to this zone
            int id = z.zoneId;
            for (int i = topLeftCell.gridPositionZ; i <= bottomLeftCell.gridPositionZ; i++)
            {
                for (int j = topLeftCell.gridPositionX; j <= topRightCell.gridPositionX; j++)
                {
                    grid[i, j].zoneId = id;
                }
            }

            break;
        }
    }



    /* Within each zone, find the thresholds that connect two different zones together
     * NOTE: this will only MARK cells as thresholds for the moment
     * ALSO NOTE: implementation is not optimal at the moment
     */
    public void FindThresholds()
    {
        //randomly pick a cell to start with
        bool validStart = false;
        Cell startCell;
        do
        {
            int randX = Random.Range(0, gridWidth - 1);
            int randZ = Random.Range(0, gridHeight - 1);
            startCell = grid[randX, randZ];
            if (startCell.isWalkable)
            {
                validStart = true;
            }
        } while (!validStart);

        bool[,] visited = new bool[mapWidth, mapHeight];
        FindThresholdsSearch(visited, startCell);

    }


    /* The recursive floodfill function used to search through the 2d grid
     * for thresholds
     */
    public void FindThresholdsSearch(bool[,] visited, Cell c)
    {
        int x = c.gridPositionX;
        int z = c.gridPositionZ;
        visited[z,x] = true;

        foreach(Edge edge in c.edgesToNeighbors)
        {
            Cell adjacent = edge.incident;
            if (!visited[adjacent.gridPositionX, adjacent.gridPositionZ] && adjacent.isWalkable && adjacent.zoneId != c.zoneId)
            {
                //this is a valid threshold
                adjacent.threshold = true;
            }
            //continue to recurse through its neighbors
            FindThresholdsSearch(visited, adjacent);
        }

    }


    /* Given some world position, give the cell that is at that position
     */
    public Cell CellFromWorldPos(Vector3 worldPos) {

        Debug.Log("worldpos: " + worldPos);

        //find hwo far along the world position the cell is on the grid
        //float percentX = Mathf.Abs((worldPos.x + (float)mapWidth / 2f) / (float)mapWidth);
        float percentZ = Mathf.Abs((worldPos.z - (float)mapHeight / 2f) / (float)mapHeight);

        float approxLeft = (topLeft.bounds.center.x - (topLeft.bounds.size.x / 2));
        float approxRight = (topRight.bounds.center.x + (topRight.bounds.size.x / 2));
        float worldDistance = Mathf.Sqrt((approxRight - approxLeft) * (approxRight - approxLeft));
        float posDistance = Mathf.Sqrt((worldPos.x - approxLeft) * (worldPos.x - approxLeft));
        float percentX = posDistance / worldDistance;

        //make sure the percent does not go beyond 100%
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);


        Debug.Log("somecomputation: " + ((worldPos.x + (float)mapWidth / 2f) / (float)mapWidth));
        Debug.Log("precentx" + percentX);
        Debug.Log("percentz" + percentZ);
        

        //find the indeces of the cell in the grid using the world position
        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int z = Mathf.RoundToInt((gridHeight - 1) * percentZ);

        
        Debug.Log("x " + x);
        Debug.Log("z " + z);
        

        return grid[z,x];
    }



    //for debugging and testing only. used to draw the cell grid
    //note: the grid may be slightly off?
    
    public void OnDrawGizmos()
    {
        //draw the frame of the grid 
        //Vector3 gridCenter = new Vector3(-130.5668f, 0, -6.210784f);
        //Gizmos.DrawWireCube(gridCenter, new Vector3(gridWidth, 1, gridHeight));

        if (grid != null)
        {
            for(int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    Cell c = grid[j,i];
                    if (c.isWalkable && c.threshold)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else if(c.isWalkable && !c.threshold)
                    {
                        Gizmos.color = Color.white;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    if (c.zoneId == 0) {
                        Gizmos.color = Color.green;
                    }

                    Vector3 increment = new Vector3(c.cellSize / 2f, 0, -1f * c.cellSize / 2f);
                    Vector3 center = c.worldPosition + increment;
                    Gizmos.DrawCube(center, new Vector3(1, 1, 1));
                }
            }
        }
    }
    
    
    
}
