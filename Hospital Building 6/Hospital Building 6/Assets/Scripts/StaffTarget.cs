using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffTarget: MonoBehaviour{

    [HideInInspector]
    public Vector3 location;
    [HideInInspector]
    public bool isAvailable;


	// Use this for initialization
	void Start () {
        location = this.transform.position;
        isAvailable = true;
	}
	
}
