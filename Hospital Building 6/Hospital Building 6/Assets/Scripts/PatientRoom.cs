using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientRoom : MonoBehaviour {


    //references
    private Staff staffPresent;
    public Map map;

    //globals
    public int zoneid; //the zone that is a patient room
    public int outsideZone; //the zone that is outside this room
    public int bathroomID;

    public bool staffIsPresent; //if there is a staff member in this room
    public bool staffChecksPatient;


	// Use this for initialization
	void Start () {
        RemoveBathroom();
	}
	
	// Update is called once per frame
	void Update () {

        

        //check if staff is checking the patient
        if (staffIsPresent)
        {
            //Debug.Log("staffPresent is not null");
            //if (staffPresent.gameObject.GetComponent<Agent>().completedPath)
            //{
                //Debug.Log("staff checks patient is marked true");
                staffChecksPatient = true;
            //}
        }
        else
        {
            //Debug.Log("staffPresent is null");
        }

	}


    /* Remove all the bathroom cells that are inside this patient room
     */
    private void RemoveBathroom()
    {
        //get the zone that contains the bathroom
        Zone bathroom = map.GetZone(bathroomID);

        //get the cells that are in the bathroom
        List<Cell> cellsToDelete = bathroom.cellsInZone;

        //get the zone that contains this patient room
        Zone patientRoom = map.GetZone(zoneid);
        List<Cell> patientRoomCells = patientRoom.cellsInZone;

        //delete the bathroom cells from this zone
        foreach (Cell bathroomCell in cellsToDelete)
        {
            if (patientRoomCells.Contains(bathroomCell))
            {
                //delete the cell from this list
                patientRoomCells.Remove(bathroomCell);
                
            }
        }
        
    }

    /* Check if a staff member has entered the patient room
     */
    private void OnTriggerStay(Collider other)
    {

        //Debug.Log(other.gameObject.name + " is staying");

        Staff staffComponent = other.gameObject.GetComponent<Staff>();
        //if what has collided is actually a staff member
        if (staffComponent != null)
        {
            //staff has entered the patient room
            staffIsPresent = true;
            staffPresent = staffComponent;
        }
    }

    /* Check if a staff member has exited the patien room
     */
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(other.gameObject.name + "has left the patient room");
        Staff staffComponent = other.gameObject.GetComponent<Staff>();
        //if what has collided is actually a staff member
        if (staffComponent != null)
        {
            //staff has exited the patient room
            staffIsPresent = false;
            staffPresent = null;

            if (staffChecksPatient)
            {
                staffChecksPatient = false;
            }
        }
    }
}
