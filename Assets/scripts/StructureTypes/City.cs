using UnityEngine;
using System.Collections;

public class City : Structure {

	// Use this for initialization
	void Start () {
        Init(StructureControl.StructureType.City);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateStructure(CameraControl.Instance.zoomPercent);
    }
}
