using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnchorController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void arrange()
    {
        int index = this.transform.childCount - 1;
        this.transform.GetChild(index).localPosition = new Vector3 (0,index*1.2f,0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
