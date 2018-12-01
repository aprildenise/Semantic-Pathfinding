using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threshold : Cell{

    //zoneid belonging to this threshold
    //this is the zoneID that this threshold is connecting
    public int tzoneID;

    //Constructor 
    public Threshold(int tzoneID, int czoneID, Vector3 worldPos, int x, int y, float size) : base(worldPos, x, y, size)
    {
        this.tzoneID = tzoneID;
        this.zoneId = czoneID;
        this.isWalkable = true;
    }

}
