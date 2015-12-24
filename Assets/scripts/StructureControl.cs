using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WarWorldInfinity.Shared;
using tmpStructure = WarWorldInfinity.Shared.Structure;
using WarWorldInfinity.Shared.Structures;

public class StructureControl : MonoBehaviour {
    [Serializable]
    public enum StructureType {
        None,
        Outpost,
        City,
        Radar,
    }

    [Serializable]
    public struct ImagePreset {
        public StructureType type;
        public List<PresetData> presets;
        
        public ImagePreset(StructureType type, List<PresetData> presets) {
            this.type = type;
            this.presets = new List<PresetData>(presets);
        }
    }

    [Serializable]
    public struct PresetData {
        public Vector2 zoomLevelRamge;
        public Sprite ownImage;
        public Sprite allyImage;
        public Sprite nuetralImage;
        public Sprite enemyImage;
        public Sprite noneImage;

        public PresetData(Vector2 zoomLevelRamge, Sprite[] images) {
            this.zoomLevelRamge = zoomLevelRamge;
            ownImage = images[0];
            allyImage = images[1];
            nuetralImage = images[2];
            enemyImage = images[3];
            noneImage = images[4];
        }
    }

    public static StructureControl Instance { get; private set; }

    public List<ImagePreset> images;
    public Vector2 Size = new Vector2(0.5f, 0.5f);
    public List<string> textureNames;
    public GameObject structurePrefab;
    public GameObject radarPrefab;
    public Dictionary<Vector2Int, Structure> outposts;
    public bool createOp = false;

    private Dictionary<StructureType, ImagePreset>_structureTextures;
    private Dictionary<string, List<string>> _structureCommands;

    void Awake() {
        Instance = this;
        outposts = new Dictionary<Vector2Int, Structure>();
        _structureTextures = new Dictionary<StructureType, ImagePreset>();
        _structureCommands = new Dictionary<string, List<string>>();
    }

	// Use this for initialization
	void Start () {
        for (int i = 0; i < images.Count; i++) {
            if (!_structureTextures.ContainsKey(images[i].type))
                _structureTextures.Add(images[i].type, images[i]);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (createOp) {
            Ray ray = CameraControl.Instance.cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100)) {
                if (Input.GetMouseButton(0)) {
                    CreateStructure(hit.point.x * 10f, hit.point.y * 10f);
                }
            }
        }
	}

    void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 300, 20), "create structure")) {
            createOp = true;
        }
    }

    public void CreateStructure(float x, float y) {
        if (TerrainBuilder.Instance.Value((int)x, (int)y) >= 0) {
            NetworkController.Instance.CreateOp(new Vector2Int((int)x, (int)y), StructureType.Outpost);
            createOp = false;
        }
    }

    public void Init() {
        Invoke("Init_int", 1);
    }

    public void SetStructures(tmpStructure[] structures) {
        for (int i = 0; i < structures.Length; i++) {
            Vector2Int pixelPos = structures[i].position;
            Vector2 worldPos = new Vector2(pixelPos.x / 10f, pixelPos.y / 10f);
            StructureType type = (StructureType)Enum.Parse(typeof(StructureType), structures[i].type);
            GameObject structureObj = (GameObject)Instantiate(structurePrefab, new Vector3(worldPos.x, worldPos.y, structurePrefab.transform.position.z),
                                                    Quaternion.identity);
            structureObj.transform.localScale = Size;
            Structure str = null;
            switch (type) {
                case StructureType.City:
                    str = structureObj.AddComponent<City>();
                    break;

                case StructureType.Outpost:
                    str = structureObj.AddComponent<Outpost>();
                    break;

                case StructureType.Radar:
                    str = structureObj.AddComponent<Radar>();
                    Radar radar = (Radar)str;
                    RadarData data = (RadarData)structures[i].extraData;
                    radar.data = data;
                    break;
                    
                case StructureType.None:
                default:
                    str = structureObj.AddComponent<None>();
                    break;
            }
            if (str != null) {
                if (outposts.ContainsKey(pixelPos)) {
                    Destroy(outposts[pixelPos].gameObject);
                    outposts.Remove(pixelPos);
                }

                str.Owner = structures[i].owner;
                str.Standings = (Structure.Standing)Enum.Parse(typeof(Structure.Standing), structures[i].standings, true);
                str.Location = structures[i].position;
                outposts.Add(pixelPos, str);
            }
            else {
                Debug.Log("Structure is null!");
            }
        }
    }

    public ImagePreset GetPresets(StructureType type) {
        if (_structureTextures.ContainsKey(type))
            return _structureTextures[type];
        return _structureTextures[StructureType.None];
    }

    public void SetStructureCommands(CommandList[] commands) {
        for (int i = 0; i < commands.Length; i++) {
            string type = commands[i].type;
            if (!_structureCommands.ContainsKey(type)) {
                _structureCommands.Add(type, new List<string>());
                _structureCommands[type].AddRange(commands[i].commands);
            }
        }
    }

    public string[] GetCommands(StructureType type) {
        if (_structureCommands.ContainsKey(type.ToString()))
            return _structureCommands[type.ToString()].ToArray();
        return new string[0];
    }

    private void Init_int() {
        Vector2 topLeft = CameraControl.Instance.topLeft * 10;
        Vector2 bottomRight = CameraControl.Instance.bottomRight * 10;
        NetworkController.Instance.RequestOps();
        //    new Vector2Int((int)topLeft.x, (int)topLeft.y),
        //    new Vector2Int((int)bottomRight.x, (int)bottomRight.y));
    }
}
