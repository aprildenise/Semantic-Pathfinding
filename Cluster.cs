using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster {

    //if the cluster is a valid cluster(ie. actually on the map)
    public bool valid;

    //dimensions and position of the cluster
    public float width;
    public float height;
    public Vector3 position;

    //hierarchical level that the cluster belongs to (NOTE: may not be needed)
    public int level;

    


    /* Constructor for a Cluster
     */
    public Cluster(float width, float height, Vector3 position, int level)
    {
        this.width = width;
        this.height = height;
        this.position = position;
        this.level = level;
        
    }

}
