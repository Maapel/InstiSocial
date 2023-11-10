using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Net.Http;
using System;
using System.Linq;
using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;

[Serializable]
public class Event
{
    public int id;
    public string event_date;
    public string event_name;
    public string event_time;
    public string event_venue;
    
    public string latitude;
    public string longitude;
}

public class ScheduleHandle : MonoBehaviour

{   
   
    
    public GameObject anchor;
    private static readonly HttpClient client = new HttpClient();
    private List<EventInfo> eventInfos = new List<EventInfo>();
    public GameObject eventInfoPrefab;

    // Start is called before the first frame update
    async void  Start()
    {   
       
        var responseString = await client.GetStringAsync("https://maadhav17.pythonanywhere.com/get_events");
        Debug.Log(responseString);
        
        List<Event> infos = new List<Event>();
        foreach ( string item in responseString.Split("|")) { 
            var eve= JsonUtility.FromJson<Event>(item);
            infos.Add(eve); 
        }
        //Debug.Log(events.eventList);
        EventInfo eventInfo = new EventInfo(infos, eventInfoPrefab,this.transform,anchor);      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class EventInfo
{
    private GameObject obj;
    public EventInfo(List<Event> events, GameObject eventInfoPrefab , Transform parent,GameObject anchor)
    {
        
        int i =0;
        List<double[]> locations = new List<double[]>();
        List<GameObject> anchors = new List<GameObject>();
        foreach (Event e in events) 
        {
              
            double[] location = {double.Parse(e.latitude) , double.Parse(e.longitude) };
            
            if (!locations.Any(elem => elem.SequenceEqual(location))){
                locations.Add(location);
                obj = GameObject.Instantiate(anchor, parent);
                
                anchors.Add(obj);
                ARGeospatialCreatorAnchor creatorAnchor = obj.GetComponent<ARGeospatialCreatorAnchor>();
                creatorAnchor.Latitude = location[0] ;
                creatorAnchor.Longitude = location[1];
            }
            int ind = locations.FindIndex(elem => elem.SequenceEqual(location));
            
            obj = GameObject.Instantiate(eventInfoPrefab, anchors[ind].transform);
            obj.GetComponent<EventController>().setParams(e);
            obj.transform.parent.GetComponent<AnchorController>().arrange();
            

        }
    }

}
