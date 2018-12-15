using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour {

    public List<Zone> zones; //all the zones in the scene

    public List<GameObject> zonesObjects = new List<GameObject>(6);

    public List<GameObject> zonesObjectsAlt = new List<GameObject>(50);
    public List<GameObject> zonesTest = new List<GameObject>(2);
    //an alternative list of zones. Has more zones. to use, make the loop below iterate on this list instead of the above one


    /* With the list of zoneObjects, initialize the Zone Objects and assign to them a zoneID
     */
    public List<Zone> InitZones()
    {
        if (this.zones != null)
        {
            return this.zones;
        }
        List<Zone> zones = new List<Zone>();
        int num = 0;
        foreach (GameObject z in zonesObjectsAlt)
        {
            Zone zone = new Zone(num, z.GetComponent<Renderer>());
            zones.Add(zone);
            num++;
        }
        this.zones = zones;
        return zones;

    }


    /* Given a zone id, return the zone that belongs to that id
     * Input: zone id
     * Output: zone object with that id
     */
    public Zone GetZone(int id)
    {
        Zone z = zones[id];
        return z;
    }

}
