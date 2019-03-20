using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staff : MonoBehaviour {


    //references
    private Agent agentComponent;
    public StaffTargetManager stm;

    //globals
    [HideInInspector]
    public List<GameObject> potentialTargets;
    private GameObject currentTarget;

    private int secondsToWait;
    private bool isWaiting;
    private bool isReadyForNewPath;




	// Use this for initialization
	void Start () {
        agentComponent = gameObject.GetComponent<Agent>();
        if (agentComponent == null)
        {
            Debug.Log("An object has the Staff script, but is not an Agent.");
        }
        isWaiting = false;
        isReadyForNewPath = true;

	}
	
	// Update is called once per frame
	void Update () {
        


        //the agent can find a new path
        if (isReadyForNewPath)
        {
            isReadyForNewPath = false;
            GoToTarget();
        }

        //the agent has completed its path, it must wait
        if (agentComponent.completedPath)
        {
            agentComponent.completedPath = false;

            //randomly choose an amount of time
            int time = Random.Range(2, 5 + 1);
            secondsToWait = time;

            StartCoroutine(WaitSomeTime());

        }




        if (isWaiting)
        {

        }


        

    }



    /* Helper functions used to make the agent wait a certain moment of time
     */
    IEnumerator WaitSomeTime()
    {
        isWaiting = true;

        yield return new WaitForSeconds(secondsToWait);

        isWaiting = false;
        isReadyForNewPath = true;
    }



    /* the staff member's main functions, which is go to a staffTarget,
     * wait there, then go to another one.
     */ 
    private void GoToTarget()
    {

        //check if StaffTargetManager is ready and has the list of targets for this agent
        potentialTargets = stm.targets;
        if (potentialTargets == null)
        {
            return;
        }

        //choose a target from the potentialTargets
        currentTarget = ChooseTarget();
        if (currentTarget == null)
        {
            //could not find a target. Wait for one to appear.
            return;
        }

        //a target has been found. send the agent to the target using hpa
        agentComponent.goal = currentTarget.transform;
        agentComponent.goalPosition = currentTarget.transform.position;
        agentComponent.FindPath();

    }


    /* Choose a target that's availabe from the potentialTargets.
     * Output: target
     */
    private GameObject ChooseTarget()
    {
        //randomly select a target from the potential targets
        int tries = 3;
        while (tries != 0)
        {
            int index = Random.Range(0, potentialTargets.Count);
            GameObject target = potentialTargets[index];

            //error checking
            StaffTarget st = target.GetComponent<StaffTarget>();
            if (st == null)
            {
                Debug.Log("A potential Target does not have the StaffTarget script");
                return null;
            }

            //check if this target is available
            if (st.isAvailable)
            {
                //if it is, it's a good target! if not, continue looking
                //change the target's availability to unavailable.
                st.isAvailable = false;
                return target;
            }

            tries--;
        }
        

        //return null if there is no target available
        return null;
    }
}
