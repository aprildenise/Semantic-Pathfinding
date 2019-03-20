using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThresholdEdge
{


    public Threshold origin;
    public Threshold incident;

    public bool isActive; //optional for now
    public float weight; //change to distance later


    //constructor
    public ThresholdEdge(Threshold t1, Threshold t2, float weight)
    {
        isActive = true;
        origin = t1;
        incident = t2;
        this.weight = weight;
    }

}