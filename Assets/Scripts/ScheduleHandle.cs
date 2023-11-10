using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using System.Net.Http;
using System;
using System.Linq;
using Google.XR.ARCoreExtensions.GeospatialCreator.Internal;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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
   
    
    public GameObject anchor_obj;
    private GameObject obj;

    private static readonly HttpClient client = new HttpClient();
    private List<EventInfo> eventInfos = new List<EventInfo>();
    public GameObject eventInfoPrefab;
    List<double[]> locations = new List<double[]>();
    List<GameObject> anchors = new List<GameObject>();
    [SerializeField]private ARAnchorManager _anchorManager;

    // Start is called before the first frame update
    async void  Start()
    {   
       
        var responseString = await client.GetStringAsync("https://maadhav17.pythonanywhere.com/get_events");
        Debug.Log(responseString);
        
        List<Event> events = new List<Event>();
        foreach ( string item in responseString.Split("|")) { 
            var eve= JsonUtility.FromJson<Event>(item);
            events.Add(eve); 
        }
        //Debug.Log(events.eventList);
        //EventInfo eventInfo = new EventInfo(infos, eventInfoPrefab,this.transform,anchor_obj);

        int i = 0;
        
        foreach (Event e in events)
        {

            double[] location = { double.Parse(e.latitude), double.Parse(e.longitude) };

            if (!locations.Any(elem => elem.SequenceEqual(location)))
            {
                locations.Add(location);
                obj = GameObject.Instantiate(anchor_obj, this.transform);

                anchors.Add(obj);
                AnchorController anchorController = obj.GetComponent<AnchorController>();

                
                var latitude = location[0];
                var longitude = location[1];
                anchorController.Latitude = latitude;
                anchorController.Longitude = longitude;
                anchorController._anchorManager = _anchorManager;
            }
            int ind = locations.FindIndex(elem => elem.SequenceEqual(location));

            obj = GameObject.Instantiate(eventInfoPrefab, anchors[ind].transform);
            obj.GetComponent<EventController>().setParams(e);
            anchors[ind].GetComponent<AnchorController>().arrange();


        }
    }
    
    
    private void Update()
    {
        
     
        
        
        // Your code came here
        

    }

    

    // Update is called once per frame

}

