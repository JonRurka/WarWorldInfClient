using UnityEngine;
using System.Collections;
using WarWorldInfinity.Shared.Structures;

public class Radar : Structure {
    public RadarData data;
    public int gridRadius;
    public float unityRadius;

    private GameObject radarObj;

	// Use this for initialization
	void Start () {
        Init(StructureControl.StructureType.Radar);
        gridRadius = data.radius;
        unityRadius = gridRadius / 10f;
        if (Standings == Standing.Own || Standings == Standing.Ally)
            CreateGraphic();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateStructure(CameraControl.Instance.zoomPercent);
    }

    public void CreateGraphic() {
        radarObj = (GameObject)Instantiate(StructureControl.Instance.radarPrefab, transform.position, Quaternion.identity);
        radarObj.transform.parent = transform;
        radarObj.transform.localScale = new Vector3(unityRadius * 2, unityRadius * 2, 1);
    }
}
