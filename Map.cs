using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {



    //globals
    private int mapWidth;
    private int mapHeight;
    private Vector3 topLeftPos; //world position of the top left corner of the map
    [HideInInspector]
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
    public void BuildCellGrid(int cellSize)
    {

        //calculate the size of a cluster based on the dimensions of a map
        int numXCells = (mapHeight / cellSize);
        int numYCells = (mapWidth / cellSize);

        //optional
        gridHeight = numYCells;
        gridWidth = numXCells;

        //build the 2d grid of cells
        Cell[,] grid = new Cell[numXCells, numYCells];
        for (int x = 0; x < numXCells; x++)
        {
            for(int y = 0; y< numYCells; y++)
            {
                //get the positon of this cell we want to init
                Vector3 worldPos = topLeftPos + (Vector3.right * (x * (cellSize + (cellSize / 2)))) + (Vector3.forward * (y * (cellSize + (cellSize / 2))));
                grid[x, y] = new Cell(worldPos, x, y);
            }
        }


        //build all possible edges and neighbors for each cell
        foreach(Cell c in grid)
        {

            List<Edge> possibleEdges = new List<Edge>();

            //neighbors that are to the left and right of the current cell
            for (int x = -1; x <= 1; x++)
            {
                //neighbors that are to the up and down of the current cell
                for (int y = -1; y <= 1; y++)
                {
                    //we're looking at the center cell, which is the cell in question
                    //skip this iteration
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    int checkX = c.gridPositionX + x;
                    int checkY = c.gridPositionY + y;

                    //if the neighbor in question is within the bounds of the actual worldGrid
                    if ((checkX >= 0 && checkX < gridWidth) && (checkY >= 0 && checkY < gridHeight))
                    {
                        if (grid[checkX, checkY].isWalkable)
                        {
                            //it is a valid neighbor. add the neighor that is at the coordinates
                            //to the list
                            Edge edge = new Edge(c, grid[checkX, checkY], 1);
                            possibleEdges.Add(edge);
                        }
                    }
                }
            }

            c.AssignNeighbors(possibleEdges);
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
            //NOTE FOR THE FUTURE: would be a good idea to put all these calculations into a different method
            //get the dimensions of the zone
            float approxLeft = z.dimensions.bounds.center.x - (z.dimensions.bounds.size.x / 2);
            float approxRight = z.dimensions.bounds.center.x + (z.dimensions.bounds.size.x / 2);
            float approxBottom = z.dimensions.bounds.center.z - (z.dimensions.bounds.size.x / 2);
            float approxTop = z.dimensions.bounds.center.z + (z.dimensions.bounds.size.x / 2);
            float approxY = z.dimensions.bounds.center.y;

            //convert dimensions into Vector3s
            Vector3 approxTopLeft = new Vector3(approxLeft, approxY, approxTop);
            Vector3 approxTopRight = new Vector3(approxRight, approxY, approxTop);
            Vector3 approxBottomLeft = new Vector3(approxLeft, approxY, approxBottom);
            Vector3 approxBottomRight = new Vector3(approxRight, approxY, approxBottom);

            //convert Vector3 to Cells
            Cell topLeftCell = CellFromWorldPos(approxTopLeft);
            Cell topRightCell = CellFromWorldPos(approxTopRight);
            Cell bottomLeftCell = CellFromWorldPos(approxBottomLeft);
            Cell bottomRightCell = CellFromWorldPos(approxBottomRight);

            //mark the cells between the above bounds as belonging to this zone
            int id = z.zoneId;
            for (int i = topLeftCell.gridPositionY; i <= bottomLeftCell.gridPositionY; i++)
            {
                for (int j = topLeftCell.gridPositionX; j <= topRightCell.gridPositionX; j++)
                {
                    grid[i, j].zoneId = id;
                }
            }
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
            int randY = Random.Range(0, gridHeight - 1);
            startCell = grid[randX, randY];
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
        int y = c.gridPositionY;
        visited[x, y] = true;

        foreach(Edge edge in c.edgesToNeighbors)
        {
            Cell adjacent = edge.incident;
            if (!visited[adjacent.gridPositionX, adjacent.gridPositionY] && adjacent.isWalkable && adjacent.zoneId != c.zoneId)
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

        //find hwo far along the world position is on the grid
        float percentX = (worldPos.x + mapWidth / 2) / mapWidth;
        float percentY = (worldPos.y + mapHeight / 2) / mapHeight;

        //make sure the percent does not go beyond 100%
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //find the indeces of the cell in the grid using the world position
        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int y = Mathf.RoundToInt((gridHeight - 1) * percentY);

        return grid[x, y];
    }
}
