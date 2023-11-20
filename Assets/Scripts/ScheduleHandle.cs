using System.Collections.Generic;

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
    public string event_details;

    public string event_time;
    public string event_time_end;
    public string event_venue;

    public string latitude;
    public string longitude;
}

public class ScheduleHandle : MonoBehaviour

{


    public GameObject anchor_obj;
    private GameObject obj;

    private static readonly HttpClient client = new HttpClient();
    //private List<EventInfo> eventInfos = new List<EventInfo>();
    public GameObject eventInfoPrefab;
    List<double[]> locations = new List<double[]>();
    List<GameObject> anchors = new List<GameObject>();
    [SerializeField] private GameObject _ARSessionOrigin;
    private bool _isLocalizing;
    private float _localizationPassedTime;
    private AREarthManager EarthManager;
    private double _orientationYawAccuracyThreshold;
    private double _horizontalAccuracyThreshold;
    private float _timeoutSeconds = 180;
    [SerializeField] GameObject currentEventPrefab;

    private void Awake()
    {
        EarthManager = _ARSessionOrigin.GetComponent<AREarthManager>();
    }
    // Start is called before the first frame update
    async void Start()
    {

        var responseString = await client.GetStringAsync("https://maadhav17.pythonanywhere.com/get_events");
        Debug.Log(responseString);

        List<Event> events = new List<Event>();
        foreach (string item in responseString.Split("|"))
        {
            var eve = JsonUtility.FromJson<Event>(item);
            events.Add(eve);
        }
        //Debug.Log(events.eventList);
        //EventInfo eventInfo = new EventInfo(infos, eventInfoPrefab,this.transform,anchor_obj);

        foreach (Event e in events)
        {

            double[] location = { double.Parse(e.latitude), double.Parse(e.longitude) };

            if (!locations.Any(elem => elem.SequenceEqual(location)))
            {
                locations.Add(location);
                obj = GameObject.Instantiate(anchor_obj);

                anchors.Add(obj);
                AnchorController anchorController = obj.GetComponent<AnchorController>();


                var latitude = location[0];
                var longitude = location[1];
                anchorController.Latitude = latitude;
                anchorController.Longitude = longitude;
                anchorController._ARSessionOrigin = _ARSessionOrigin;
            }
            int ind = locations.FindIndex(elem => elem.SequenceEqual(location));
            var lower_lim = DateTime.Parse(e.event_date +" " +e.event_time);
            var upper_lim = DateTime.Parse(e.event_date +" "+ e.event_time_end);

            if ( (DateTime.Now >= lower_lim ) &&(DateTime.Now <  upper_lim )){
                obj = GameObject.Instantiate(currentEventPrefab, anchors[ind].transform.GetChild(0));
                obj.transform.localPosition = new Vector3(0, 0, 0);
            }
            else if (DateTime.Now < lower_lim)
            {
                obj = GameObject.Instantiate(eventInfoPrefab, anchors[ind].transform.GetChild(0).GetChild(1));
                anchors[ind].GetComponent<AnchorController>().arrange();
                Debug.Log("Arrange");
            }
            else
            {
                continue;
            }
            
            
            obj.GetComponent<EventController>().setParams(e);
            


        }
        foreach( var anchor in anchors) { anchor.transform.GetChild(0).GetChild(1).gameObject.SetActive(false); }
        
    }


    private void Update()
    {
        //Touch for more options
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);



            // Raycast against planes and feature points.

           
            // Perform the raycast.
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray  = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit raycastHit;
                if (Physics.Raycast(ray,out raycastHit))
                {
                    // Raycast hits are sorted by distance, so the first one will be the closest hit.
                    
                    //Debug.Log("HIT");
                    //Debug.Log(raycastHit.transform.name);
                    
                    if (raycastHit.transform.name.Equals("More"))
                    {
                        var anchorDisp = raycastHit.transform.parent;
                        var menuOpen = !anchorDisp.GetChild(1).gameObject.activeSelf;
                        anchorDisp.GetChild(1).gameObject.SetActive(menuOpen);
                        anchorDisp.GetChild(0).GetChild(2).gameObject.SetActive(menuOpen);
                        anchorDisp.GetChild(0).GetChild(1).gameObject.SetActive(!menuOpen);


                        if (anchorDisp.childCount > 2)
                        {
                            anchorDisp.GetChild(2).gameObject.SetActive(!menuOpen);

                        }
                    }
                    
                    // Do something with hit.
                }
            }
        }

        bool isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
               Input.location.status == LocationServiceStatus.Running;
        var earthTrackingState = EarthManager.EarthTrackingState;
        var pose = earthTrackingState == TrackingState.Tracking ?
            EarthManager.CameraGeospatialPose : new GeospatialPose();
       /*Debug.Log(string.Format(
                "Latitude/Longitude: {1}°, {2}°{0}" +
                "Horizontal Accuracy: {3}m{0}" +
                "Altitude: {4}m{0}" +
                "Vertical Accuracy: {5}m{0}" +
                "Eun Rotation: {6}{0}" +
                "Orientation Yaw Accuracy: {7}°",
                Environment.NewLine,
                pose.Latitude.ToString("F6"),
                pose.Longitude.ToString("F6"),
                pose.HorizontalAccuracy.ToString("F6"),
                pose.Altitude.ToString("F2"),
                pose.VerticalAccuracy.ToString("F2"),
                pose.EunRotation.ToString("F1"),
                pose.OrientationYawAccuracy.ToString("F1")));*/
       
          

        if (!isSessionReady || earthTrackingState != TrackingState.Tracking ||
            pose.OrientationYawAccuracy > _orientationYawAccuracyThreshold ||
            pose.HorizontalAccuracy > _horizontalAccuracyThreshold)
        {
            // Lost localization during the session.
            if (!_isLocalizing)
            {
                _isLocalizing = true;
                _localizationPassedTime = 0f;
                foreach (var go in anchors)
                {
                    go.SetActive(false);
                }
            }

            if (_localizationPassedTime > _timeoutSeconds)
            {
                Debug.LogError("Geospatial sample localization timed out.");

            }
            else
            {
                _localizationPassedTime += Time.deltaTime;

            }
        }
        else if (_isLocalizing)
        {
            // Finished localization.
            _isLocalizing = false;
            _localizationPassedTime = 0f;
            foreach (var go in anchors)
            {
                go.SetActive(true);
            }


        }



        // Your code came here


    }



    // Update is called once per frame

}