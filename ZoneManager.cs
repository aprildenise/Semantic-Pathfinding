using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour {



    public List<GameObject> zonesObjects = new List<GameObject>(6);
    public List<Zone> zones; //the original list of zones. has 10 zones.

    public List<GameObject> zonesObjectsAlt = new List<GameObject>(50);
    public List<GameObject> zonesTest = new List<GameObject>(2);
    //an alternative list of zones. Has more zones. to use, make the loop below iterate on this list instead of the above one

    /* Find the dimensions of each zone in the zones list
     */
    public List<Zone> FindZoneBounds()
    {
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

}
