using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffTargetManager : MonoBehaviour {


    //references 
    public GameObject targetObjectsOne;
    public GameObject targetObjectsTwo;
    public GameObject targetObjectsThree;

    //globals
    [HideInInspector]
    public List<GameObject> targets;

	// Use this for initialization
	void Start () {

        //get all the targets from the reference gameObjects
        List<GameObject> temp = new List<GameObject>();

        int count = targetObjectsOne.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = targetObjectsOne.transform.GetChild(i);
            temp.Add(child.gameObject);
        }

        count = targetObjectsTwo.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = targetObjectsTwo.transform.GetChild(i);
            temp.Add(child.gameObject);
        }

        count = targetObjectsThree.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = targetObjectsThree.transform.GetChild(i);
            temp.Add(child.gameObject);
        }

        targets = temp;
    }
	
}
