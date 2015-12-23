using UnityEngine;
using System.Collections;

public class Radar : Structure {

	// Use this for initialization
	void Start () {
        Init(StructureControl.StructureType.Radar);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateStructure(CameraControl.Instance.zoomPercent);
    }
}
