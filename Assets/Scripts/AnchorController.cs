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
        this.transform.GetChild(index).localPosition = new Vector3 (0,index*1.2f,0);
    }
    // Update is called once per frame
    void Update()
    {
        if (_anchorResolution == AnchorResolutionState.NotStarted)
        {
            AddGeoAnchorAtRuntime();
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
            string errorReason =
                "The Session Origin's AnchorManager is null.";
            Debug.LogError("Unable to place ARGeospatialCreatorAnchor " + name + ": " +
                errorReason);
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
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;
        transform.SetParent(resolvedAnchor.transform,false);

        _anchorResolution = AnchorResolutionState.Complete;
        Debug.Log("Geospatial Anchor resolved: " + name);
    }
    private IEnumerator ResolveTerrainAnchor()
    {
       
        double altitudeAboveTerrain = 2;
        ARGeospatialAnchor anchor = null;
        ResolveAnchorOnTerrainPromise promise =
            _anchorManager.ResolveAnchorOnTerrainAsync(
                Latitude, Longitude, altitudeAboveTerrain, transform.rotation);

        yield return promise;
        var result = promise.Result;
        if (result.TerrainAnchorState == TerrainAnchorState.Success)
        {
            anchor = result.Anchor;
        }

        FinishAnchor(anchor);
        yield break;
    }
    
}
