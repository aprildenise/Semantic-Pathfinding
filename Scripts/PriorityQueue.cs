using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> {

    private List<Tuple<T, float>> data;

    //Constructor
    public PriorityQueue(){
        this.data = new List<Tuple<T, float>>();

    }

    public void Enqueue(T item, float priority){
        Tuple<T, float> i = Tuple.Create(item, priority);
        data.Add(i);

        //sort the queue
        int ci = data.Count - 1;
        while (ci > 0){
            int pi = (ci - 1)/2;
            //if this new data does not have a higher priority 
            //resort data so the higher priority is in the front of the list
            if (Compare(data[ci], data[pi]) >= 0){
                break;
            }
            Tuple<T, float> temp = data[ci];
            data[ci] = data[pi]; 
            data[pi] = temp;
            ci = pi;
        }
    }

    public T Deqeueue(){
        if (data.Count == 0){
            return default(T);
        }

        int li = data.Count - 1;
        Tuple<T, float> frontItem = data[0];
        data[0] = data[li];
        data.RemoveAt(li);

        --li;
        int pi = 0;
        //resort
        while(true){
            int ci = pi*2+1;
            if (ci > li){
                break;
            }
            int rc = ci + 1; //index of the right child
            if (rc <= li && Compare(data[rc], data[ci]) < 0){
                ci = rc;
            }
            if (Compare(data[pi], data[ci]) <= 0){
                break;
            }
            Tuple<T, float> temp = data[pi];
            data[pi] = data[ci];
            data[ci] = temp;
            pi = ci;
        }

        return frontItem.Item1;


    }


    public int Count(){
        return data.Count;
    }

    
    private int Compare(Tuple<T, float> item1, Tuple<T, float> item2){
        if (item1.Item2 < item2.Item2)
        {
            return -1;
        }
        else if (item1.Item2 > item2.Item2)
        {
            return 1;
        }
        else return 0;
    }

    public Tuple<T, float> Peek(){
        if (data.Count == 0){
            return null;
        }
        Tuple<T, float> frontItem = data[0];
        return frontItem;
    }

	
}


/* Tuple datastructure used to help data things during pathfinding
 */
public class Tuple<T, U>
{
    public T Item1 { get; private set; }
    public U Item2 { get; private set; }

    public Tuple(T item1, U item2)
    {
        Item1 = item1;
        Item2 = item2;
    }
}

public static class Tuple
{
    public static Tuple<T, U> Create<T, U>(T item1, U item2)
    {
        return new Tuple<T, U>(item1, item2);
    }
}