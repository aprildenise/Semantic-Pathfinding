//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Agents : MonoBehaviour {


//    private HPAManager HPA;
//    //[System.Serializable]
//    private Map m;

//    public Transform agent;
//    public Transform goal;

//    public Transform topLeft;
//    public Transform topRight;
//    public Transform bottomRight;

//    public ZoneManager zm;


//    // Use this for initialization
//    void Start()
//    {
        
//        initalizeMap();
//        runHPA(agent, goal);
//    }
  

//	// Update is called once per frame
//	void Update () {
		
//	}
//    public void initalizeMap()
//    {
//        Map mp = gameObject.AddComponent(typeof(Map)) as Map;
//        mp.zm = GameObject.Find("ZoneManager").GetComponent<ZoneManager>();
//        mp.topLeft = GameObject.Find("Object_480").GetComponent<Renderer>();
//        mp.topRight = GameObject.Find("Object_444").GetComponent<Renderer>();
//        mp.bottomRight = GameObject.Find("Object_182").GetComponent<Renderer>();
//        mp.InitMap();
//    }
//    public void runHPA(Transform t1, Transform t2)
//    {
//        HPAManager hpa = gameObject.AddComponent(typeof(HPAManager)) as HPAManager;
//        hpa.HPAAlt(agent.position, goal.position);
//    }

//}
