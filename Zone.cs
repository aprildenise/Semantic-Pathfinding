using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone{


    public int zoneId;
    public Renderer dimensions;

    public Zone(int zoneId, Renderer dimensions)
    {
        this.zoneId = zoneId;
        this.dimensions = dimensions;
    }


	
}
