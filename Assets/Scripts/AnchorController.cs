using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions.Internal;
using UnityEngine.XR.ARSubsystems;

public class AnchorController : MonoBehaviour
{
    public double Latitude;

    /// <summary>Gets or sets the longitude of this anchor.</summary>
    public double Longitude;
    public GameObject _ARSessionOrigin ;
    private ARAnchorManager _anchorManager=null;
    private ARGeospatialAnchor rGeospatialAnchor=null;
    //private ARGeospatialAnchor anchor;
   
    private AnchorResolutionState _anchorResolution = AnchorResolutionState.NotStarted;
    private enum AnchorResolutionState
    {
        NotStarted,
        InProgress,
        Complete
    }
    // Start is called before the first frame update
    void Start()
    {
        _anchorManager = _ARSessionOrigin.GetComponent<ARAnchorManager>();   
    }
    public void arrange()
    {
        int index = this.transform.childCount - 1;
        this.transform.GetChild(index).localPosition = new Vector3 (0,index*1.2f * this.transform.GetChild(index).localScale.y, 0);
    }
    // Update is called once per frame
    void Update()
    {
        TrackingState trackingState = _ARSessionOrigin.GetComponent<AREarthManager>().EarthTrackingState;
        if ((_anchorResolution == AnchorResolutionState.NotStarted) & Time.timeSinceLevelLoad>4)
        {
            
            AddGeoAnchorAtRuntime();
            

        }
        if (rGeospatialAnchor != null)
        {
            if (!rGeospatialAnchor.trackingState.Equals(TrackingState.Tracking))
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
               
                transform.GetChild(0).gameObject.SetActive(Vector3.Distance(Camera.main.transform.position,transform.position)<=20);

            }
            Debug.Log("Tracking State :  " + trackingState.ToString());
            Debug.Log("Anchor Tracking State :  " + rGeospatialAnchor.trackingState.ToString());
            Debug.Log("Anchor Prefab Position : " + transform.position.ToString());
            //Debug.Log("Venue Position : " + transform.GetChild(0).position.ToString());
            //Debug.Log("Event Position : " + transform.GetChild(0).GetChild(0).position.ToString());
            Debug.Log("Resolved Anchor Position : " + rGeospatialAnchor.transform.position.ToString());
            Debug.Log("Camera Position : " + Camera.main.transform.position.ToString());
            Debug.Log("  ");
        }
        

    }
    private void AddGeoAnchorAtRuntime()
    {
        

        // During boot this will return false a few times.
        

        // Geospatial anchors cannot be created until the AR Session is stable and tracking:
        // https://developers.google.com/ar/develop/unity-arf/geospatial/geospatial-anchors#place_a_geospatial_anchor_in_the_real_world
        if (_ARSessionOrigin.GetComponent<AREarthManager>().EarthTrackingState != TrackingState.Tracking)
        {
            Debug.Log("Waiting for AR Session to become stable.");
            return;
        }


        if (_anchorManager == null)  
        {
            string errorReason = "The Session Origin's AnchorManager is null.";
            Debug.LogError("Unable to place ARGeospatialCreatorAnchor " + name + ": " + errorReason);
            _anchorResolution = AnchorResolutionState.Complete;
            return;
        }
        StartCoroutine(ResolveTerrainAnchor());
    }

    private void FinishAnchor(ARGeospatialAnchor resolvedAnchor)
    {
        if (resolvedAnchor == null)
        {
            // If we failed once, resolution is likley to keep failing. Don't retry endlessly.
            Debug.LogError("Failed to make Geospatial Anchor for " + name);
            _anchorResolution = AnchorResolutionState.Complete;
            return;
        }

        // Maintain an association between the ARGeospatialCreatorAnchor and the resolved
        // ARGeospatialAnchor by making the creator anchor a child of the runtime anchor.
        // We zero out the pose & rotation on the creator anchor, since the runtime
        // anchor will handle that from now on.
        rGeospatialAnchor = resolvedAnchor;
        transform.rotation = Quaternion.identity;
       
        transform.parent = resolvedAnchor.gameObject.transform;
        transform.localPosition= new Vector3(0, 0, 0);
        transform.LookAt(new Vector3(-Camera.main.transform.position.x, transform.position.y,-Camera.main.transform.position.z));
        _anchorResolution = AnchorResolutionState.Complete;
        Debug.Log("Geospatial Anchor resolved: " + name+ "  "+ resolvedAnchor.trackingState.ToString());
    }
   
    private IEnumerator ResolveTerrainAnchor()
    {
       
        double altitudeAboveTerrain = 0;
        ARGeospatialAnchor anchor = null;
        ResolveAnchorOnTerrainPromise promise =_anchorManager.ResolveAnchorOnTerrainAsync( Latitude, Longitude, altitudeAboveTerrain, transform.rotation);
        yield return promise;

        var result = promise.Result;
        
        
        if (result.TerrainAnchorState == TerrainAnchorState.Success &&
               result.Anchor != null)
        {
            anchor = result.Anchor;
            
        }
        

            FinishAnchor(anchor);
        yield break;
    }
    
}
