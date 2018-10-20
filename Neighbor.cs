using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighbor
{

    public int vertexNum;
    public Neighbor next;

    public Neighbor(int vertexNum, Neighbor next)
    {
        this.vertexNum = vertexNum;
        this.next = next;
    }

}
