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



public class ScheduleHandleTest : MonoBehaviour

{
    //BingMapsApi Key : AoN0J8xMFNrX9FcL0go-l9QfWRL6sbOYoJ0dK4gErp46Fx3XtovdnXgxiM6eYs4e


   
    

    private static readonly HttpClient client = new HttpClient();
    //private List<EventInfo> eventInfos = new List<EventInfo>();
    public GameObject eventInfoPrefab;
    
    

    // Start is called before the first frame update
    async void  Start()
    {   
        string directions_link = "https://dev.virtualearth.net/REST/V1/Routes/Walking?wp.0=12.990065205335194,80.23224717894841&wp.1=12.985991040520409,80.233811067139982&key=AoN0J8xMFNrX9FcL0go-l9QfWRL6sbOYoJ0dK4gErp46Fx3XtovdnXgxiM6eYs4e";
        var responseString = await client.GetStringAsync("https://maadhav17.pythonanywhere.com/get_events");

        Debug.Log(responseString);
        responseString = await client.GetStringAsync(directions_link);

        Debug.Log(responseString);  

        List<Event> events = new List<Event>();
        foreach ( string item in responseString.Split("|")) { 
            var eve= JsonUtility.FromJson<Event>(item);
            events.Add(eve); 
        }
        //Debug.Log(events.eventList);
        //EventInfo eventInfo = new EventInfo(infos, eventInfoPrefab,this.transform,anchor_obj);

        int i = 0;
        
       
    }
    
    
    private void Update()
    {
        
     
        
        
        // Your code came here
        

    }

    

    // Update is called once per frame

}

