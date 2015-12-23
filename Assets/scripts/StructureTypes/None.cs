using UnityEngine;
using System.Collections;

public class None : Structure {

	// Use this for initialization
	void Start () {
        Init(StructureControl.StructureType.None);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateStructure(CameraControl.Instance.zoomPercent);
    }
}
