using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threshold : Cell{

    
    public int tzoneID; //zoneid belonging to this threshold. this is the zoneID that this threshold is connecting
    public List<ThresholdEdge> tedgesToNeighbors;
    //public Threshold tparent; //temporary

    public string belongsTo;

    public new float iCost;

    //Constructor 
    public Threshold(int tzoneID, int czoneID, float iCost, Vector3 worldPos, int x, int y, float size) : base(worldPos, x, y, size)
    {
        this.iCost = iCost;
        this.tzoneID = tzoneID;
        this.zoneId = czoneID;
        this.isWalkable = true;
        this.belongsTo = null;
    }


    /* Assign the given list of threshold edges, add those edges to this threshold.
    */
    public void TAssignNeighbors(List<ThresholdEdge> t)
    {
        tedgesToNeighbors = t;
    }


}
