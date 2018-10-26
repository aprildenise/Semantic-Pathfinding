using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAManager : MonoBehaviour {


    //references
    public Map m;



    // used for testing and other debugging
	// Use this for initialization
	void Start () {
        m.GetMapDimensions();
        m.BuildCellGrid(.25f);
        m.DefineZones();

	}

    
}
