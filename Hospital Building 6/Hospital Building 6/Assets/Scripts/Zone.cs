using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone{

    
    //references
    public new Renderer renderer;
    public Collider collider;

    public Vector3 topLeft; //the 4 corners of the zone
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;

    //globals
    [HideInInspector]
    public int zoneId;
    [HideInInspector]
    public List<Threshold> thresholds; //list of thresholds that belong to this zone
    public float iCost;
    [HideInInspector]
    public List<Cell> cellsInZone; //list of all the cells that belong to this zone

    //constructor
    public Zone(int zoneId, Renderer renderer, Collider collider, float iCost)
    {
        this.zoneId = zoneId;
        this.renderer = renderer;
        this.collider = collider;
        this.iCost = iCost;
        cellsInZone = new List<Cell>();
        FindZoneCorners();
    }


    private void FindZoneCorners()
    {
        //get the dimensions of this zone
        float approxLeft = renderer.bounds.center.x - (renderer.bounds.size.x / 2);
        float approxRight = renderer.bounds.center.x + (renderer.bounds.size.x / 2);
        float approxBottom = renderer.bounds.center.z - (renderer.bounds.size.z / 2);
        float approxTop = renderer.bounds.center.z + (renderer.bounds.size.z / 2);
        float approxY = renderer.bounds.center.y;

        //convert dimensions into Vector3s
        topLeft = new Vector3(approxLeft, approxY, approxTop);
        topRight = new Vector3(approxRight, approxY, approxTop);
        bottomLeft = new Vector3(approxLeft, approxY, approxBottom);
        bottomRight = new Vector3(approxRight, approxY, approxBottom);

        if (topLeft == topRight)
        {
            //something wrong has happened
            //!! will fix another time//
            //try using collider bounds instead
            approxLeft = collider.bounds.center.x - (collider.bounds.size.x / 2);
            approxRight = collider.bounds.center.x + (collider.bounds.size.x / 2);
            approxBottom = collider.bounds.center.z - (collider.bounds.size.z / 2);
            approxTop = collider.bounds.center.z + (collider.bounds.size.z / 2);
            approxY = collider.bounds.center.y;

            topLeft = new Vector3(approxLeft, approxY, approxTop);
            topRight = new Vector3(approxRight, approxY, approxTop);
            bottomLeft = new Vector3(approxLeft, approxY, approxBottom);
            bottomRight = new Vector3(approxRight, approxY, approxBottom);

        }

        
    }


    /* Given a threshold, add it to this zone's threshold list
     */
    public void AddThresholdToZone(Threshold t)
    {
        if (thresholds == null)
        {
            thresholds = new List<Threshold>();
        }

        if (!thresholds.Contains(t)){
            thresholds.Add(t);
        }
        
    }



	
}
