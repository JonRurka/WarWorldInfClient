using UnityEngine;
using System.Collections;
using System;

public class Outpost : Structure {  

    // Use this for initialization
    void Start () {
        Init(StructureControl.StructureType.Outpost);
        AddCommand("Upgrade", Upgrade_CMD);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateStructure(CameraControl.Instance.zoomPercent);
	}

    void Upgrade_CMD() {
        ShowButtons = true;
        buttonOverwrite.Add(StructureControl.StructureType.Radar.ToString(), Upgrade_OVerride);
    }

    void Upgrade_OVerride(string newStructure) {
        StructureControl.StructureType type = (StructureControl.StructureType)Enum.Parse(typeof(StructureControl.StructureType), 
                                                                                         newStructure, true);
        NetworkController.Instance.ChangeOp(Location, type);
    }
}
