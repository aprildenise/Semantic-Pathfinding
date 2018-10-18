using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMaze : MonoBehaviour {


    //for testing only?
    //(Note: need to find a better why to calculate the dimensions of the map)
    public Renderer topright;
    public Renderer topleft;
    public Renderer bottomright;


    //list of clusters for every level of abstraction 
    //(NOTE: may be moved to another class; size of list is currently only 1. Need to expand to hold all sets of clusters)
    public List<Cluster> C;


    //for testing
    private void Start()
    {
        Vector2 dimensions = GetSizeOfMap();
        InitAbstractMaze(dimensions.x, dimensions.y, 2); 
    }




    /* Determine the entire size of the map.
     * Output: the width and the height of the map, stored in a Vector2
     */
    private Vector2 GetSizeOfMap()
    {

        //use the Renderers of the corners to find where they are in world coordinates
        float approxLeft = (topleft.bounds.center.x - (topleft.bounds.size.x /2));
        float approxRight = (topright.bounds.center.x + (topright.bounds.size.x / 2));
        float approxBottom = (bottomright.bounds.center.z - (bottomright.bounds.size.z / 2));
        float approxTop = (topleft.bounds.center.z + (topleft.bounds.size.z / 2));

        //use the world coordinates to get the width and the height
        float approxWidth = Mathf.Round(Mathf.Abs(approxLeft - approxRight));
        float approxHeight = Mathf.Round(Mathf.Abs(approxTop - approxBottom));

        return new Vector2(approxWidth, approxHeight);

    }




    /* Initialze the abstract maze
     * Input: width of the map, height of the map, the length of a SQUARE cluster
     */
    public void InitAbstractMaze(float mapWidth, float mapHeight, float clusterSize) 
    {
        //calculate the size of a cluster based on the dimensions of a map
        float clusterHeight = mapHeight / clusterSize;
        float clusterWidth = mapWidth / clusterSize;

        //get the set of 1-clusters (the lowest possible level)
        C = BuildClusters(1, clusterWidth, clusterHeight);
        

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

        }


        return clusters; 
    }

}
