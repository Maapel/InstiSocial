using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public class EventController : MonoBehaviour
{
    public TMP_Text eventNameHolder;
    public TMP_Text dateHolder;
    public TMP_Text detailsHolder;

    public TMP_Text timeHolder;
    public void setParams(Event eventObj) {
        eventNameHolder.text = eventObj.event_name;
        dateHolder.text = eventObj.event_date;
        timeHolder.text = eventObj.event_time + " - " + eventObj.event_time;
        detailsHolder.text = eventObj.event_details;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
