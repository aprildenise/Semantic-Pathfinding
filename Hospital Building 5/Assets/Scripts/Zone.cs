using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone{


    public int zoneId;
    public new Renderer renderer;
    //public int width; //!!MAY NOT BE NEEDED!!
    //public int height;
    public Vector3 topLeft; //the 4 corners of the zone
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;

    public List<Threshold> thresholds; //list of thresholds that belong to this zone
    public float iCost;

    //constructor
    public Zone(int zoneId, Renderer renderer, float iCost)
    {
        this.zoneId = zoneId;
        this.renderer = renderer;
        this.iCost = iCost;
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
