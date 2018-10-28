using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour {


    public List<GameObject> zonesObjects = new List<GameObject>(6);
    public List<Zone> zones;

    /* Find the dimensions of each zone in the zones list
     */
    public List<Zone> FindZoneBounds()
    {
        List<Zone> zones = new List<Zone>();
        int num = 0;
        foreach (GameObject z in zonesObjects)
        {
            Zone zone = new Zone(num, z.GetComponent<Renderer>());
            zones.Add(zone);
            num++;
        }
        this.zones = zones;
        return zones;

    }

}
