using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visitor : MonoBehaviour {


    //references
    private Agent agentComponent;
    public PatientRoom startingPatientRoom;
    public Map map;

    //globals 
    public Zone patientRoomZone;
    public Zone outsideRoomZone;
    private Cell currentTarget;
    private bool isInPatientRoom;




    // Use this for initialization
    void Start () {
        //make sure the agent component is attached
        agentComponent = gameObject.GetComponent<Agent>();
        if (agentComponent == null)
        {
            Debug.Log("An object has the Visitor script, but is not an Agent.");
        }

        //make sure the map component is attached
        if (map == null)
        {
            map = GameObject.Find("MapManager").GetComponent<Map>();
        }

        //make sure that this visitor knows what patient room it is in
        if (startingPatientRoom == null)
        {
            Debug.Log("A Visitor does not know what PatientRoom it belongs to.");
        }
        else
        {
            isInPatientRoom = true;
        }

        //make sure that the visitor has a reference to the ZONE of the patient room
        if (patientRoomZone == null)
        {
            //get the zone that it is in
            int id = startingPatientRoom.zoneid;
            Zone zone = map.GetZone(id);
            patientRoomZone = zone;
        }

        //make sure that the visitor has a reference to the ZONE outside the patient room
        if (outsideRoomZone == null)
        {
            int id = startingPatientRoom.outsideZone;
            //get the ZONE component
            Zone outsideZone = map.GetZone(id);
            outsideRoomZone = outsideZone;
        }

        currentTarget = null;
    }


    // Update is called once per frame
    void Update () {
		
        //check if a Staff member has entered the Patient Room
        if (startingPatientRoom.staffChecksPatient)
        {
            if (isInPatientRoom)
            {
                MoveOutOfPatientRoom();
                isInPatientRoom = false;
            }
            
            
        }
        else
        {
            //staff is not present in the room
            if (isInPatientRoom)
            {
                //wait
            }
            // if staff leaves patient room
            if (!isInPatientRoom && !startingPatientRoom.staffChecksPatient)
            {
                //come back into the patient room
                MoveIntoPatientRoom();
                isInPatientRoom = true;
            }
        }

	}


    /* Move the visitor out of the patient room
     */
    private void MoveOutOfPatientRoom()
    {
        List<Cell> lstTargetZoneCells = outsideRoomZone.cellsInZone;
        int lstSize = lstTargetZoneCells.Count;

        Cell c = null;
        int tries = 3;
        while (tries > 0)
        {
            int randInt = Random.Range(0, lstSize - 1);
            c = lstTargetZoneCells[randInt];
            if (c.isWalkable)
            {
                break;
            }
            else
            {
                c = null;
            }
        }
 
        if (c == null)
        {
            Debug.Log("Visitor failed to find a cell to move to");
            return;
        }
        Vector3 goal = c.worldPosition;

        agentComponent.goalPosition = goal;
        agentComponent.FindPath();
    }


    private void MoveIntoPatientRoom()
    {
        List<Cell> lstTargetZoneCells = patientRoomZone.cellsInZone;
        int lstSize = lstTargetZoneCells.Count;


        Cell c = null;
        int tries = 3;
        while (tries > 0)
        {
            int randInt = Random.Range(0, lstSize - 1);
            c = lstTargetZoneCells[randInt];
            if (c.isWalkable)
            {
                break;
            }
            else
            {
                c = null;
            }
        }

        if (c == null)
        {
            Debug.Log("Visitor failed to find a cell to move to");
            return;
        }
        Vector3 goal = c.worldPosition;

        agentComponent.goalPosition = goal;
        agentComponent.FindPath();
    }


}
