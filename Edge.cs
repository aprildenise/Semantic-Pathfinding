using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge {


    public Cell origin;
    public Cell incident;

    //optional for now
    public bool isActive;
    public float weight;


    //constructor
    public Edge(Cell cell1, Cell cell2, float weight)
    {
        isActive = true;
        origin = cell1;
        incident = cell2;
        this.weight = weight;
    }

}


//optional
public enum EdgeType
{
    intra,
    inter,
    threshold
}