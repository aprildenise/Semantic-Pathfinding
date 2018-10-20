using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMaze : MonoBehaviour {


    //for testing only?
    //(Note: need to find a better why to calculate the dimensions of the map)
    public Renderer topright;
    public Renderer topleft;
    public Renderer bottomright;
    [HideInInspector]
    public float mapWidth;
    [HideInInspector]
    public float mapHeight;
    [HideInInspector]
    public Vector3 mapTopLeft;


    //list of clusters for every level of abstraction 
    //(NOTE: may be moved to another class; size of list is currently only 1. Need to expand to hold all sets of clusters)
    public List<Cluster> C;

    public List<Node[,]> clusterNodes;


    //for testing
    private void Start()
    {
        GetSizeOfMap();

        /*
        mapWidth = dimensions.x;
        mapHeight = dimensions.y;
        InitAbstractMaze(dimensions.x, dimensions.y, 5);
        */

        clusterNodes = new List<Node[,]>();
        InitAbstractMaze(5);
    }




    /* Determine the entire size of the map using the corners of the map
     * Output: the width and the height of the map, stored in a Vector2
     */
    private void GetSizeOfMap()
    {

        //use the Renderers of the corners to find where they are in world coordinates
        float approxLeft = (topleft.bounds.center.x - (topleft.bounds.size.x /2));
        float approxRight = (topright.bounds.center.x + (topright.bounds.size.x / 2));
        float approxBottom = (bottomright.bounds.center.z - (bottomright.bounds.size.z / 2));
        float approxTop = (topleft.bounds.center.z + (topleft.bounds.size.z / 2));

        //use the world coordinates to get the width and the height
        mapWidth = Mathf.Round(Mathf.Abs(approxLeft - approxRight));
        mapHeight = Mathf.Round(Mathf.Abs(approxTop - approxBottom));
        mapTopLeft = new Vector3(approxLeft, topleft.bounds.center.y, approxTop);

    }




    /* Initialze the abstract maze
     * Input: width of the map, height of the map, the length of a SQUARE cluster
     */
    public void InitAbstractMaze(int clusterSize)
    {

        /* Old code
        //calculate the size of a cluster based on the dimensions of a map
        float clusterHeight = mapHeight / clusterSize;
        float clusterWidth = mapWidth / clusterSize;

        //get the set of 1-clusters (the lowest possible level)
        C = BuildClusters(1, clusterWidth, clusterHeight);
        */

        //get the set of the lowest level clusters (2D grid)
        clusterNodes.Add(BuildClusterGraph(1, 5));

    }




    /* Get the clusters from given level
     * Input: desired hierarchical level of clusters, clusterWidth, clusterHeight
     * Output: list of clusters at the given level :P
     */
    public List<Cluster> BuildClusters(int level, float clusterWidth, float clusterHeight) {
        List<Cluster> clusters = new List<Cluster>();


        //Build the lowest level of clusters: the 1-clusters
        if (level == 1)
        {
            Vector3 worldTopLeft = mapTopLeft;

            for (int i = 0; i < (mapWidth / clusterWidth); i++)
            {
                //iterate through the entire map to fill it with clusters
                for (int j = 0; j < (mapHeight / clusterHeight); j++)
                {
                    Vector3 position = new Vector3(worldTopLeft.x + (clusterWidth * j), worldTopLeft.y, worldTopLeft.z - (clusterHeight * i));

                    Cluster cluster = new Cluster(clusterWidth, clusterHeight, position, level);
                    clusters.Add(cluster);
                    //Debug.Log("cluster added at:" + position);
                }
            }
        }

        return clusters; 
    }


    /* Graph implementation of BuildClusters
     */
    public Node[,] BuildClusterGraph(int level, int clusterSize)
    {

        //calculate the size of a cluster based on the dimensions of a map
        int numXClusters = (int)Mathf.Round(mapHeight / clusterSize);
        int numYClusters = (int)Mathf.Round(mapWidth / clusterSize);

        Node[,] grid = new Node[numXClusters, numYClusters];

        if (level == 1)
        {
            //Build a 2d grid of cluster nodes
            for (int x = 0; x < numXClusters; x++)
            {
                for (int y = 0; y < numYClusters; y++)
                {
                    //find the position of this node
                    Vector3 worldPoint = mapTopLeft + (Vector3.right * (x * (clusterSize + (clusterSize/2)))) + (Vector3.forward * (y * (clusterSize + (clusterSize/2))));

                    grid[x, y] = new Node(worldPoint, x, y, 1, "");

                    /* old code
                    //see if we should place a wall there
                    PlaceWalls(worldPoint);

                    //find if this node is walkable, depending if there is an unwakabe object on it
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    //create the node
                    grid[x, y] = new Node(walkable, worldPoint, x, y);
                    */
                }
            }
        }
        return grid;
    }


}
