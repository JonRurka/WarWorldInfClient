using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WarWorldInfinity.Shared;
using tmpStructure = WarWorldInfinity.Shared.Structure;
using WarWorldInfinity.Shared.Structures;

public class Structure : MonoBehaviour {
    public enum Standing {
        None,
        Own,
        Ally,
        Nuetral,
        Enemy,
    }
    
    public delegate void Command();
    public StructureControl.StructureType Type;
    public Texture2D texture;
    public Vector2Int Location;
    public StructureControl.ImagePreset images;
    public string Owner;
    public Standing Standings;
    public bool ShowButtons;

    public List<string> serverCommands;
    public Dictionary<string, System.Action> clientCommands;
    public Dictionary<string, System.Action<string>> buttonOverwrite;
    private float oldZoom = -1;
    private float zoom;
    private SpriteRenderer render;
    private Vector2 optionLocation;

    void OnGUI() {
        if (ShowButtons) {
            List<string> commands = new List<string>(serverCommands);
            commands.AddRange(clientCommands.Keys);
            if (buttonOverwrite.Count > 0)
                commands = new List<string>(buttonOverwrite.Keys);
            GUI.Box(new Rect(optionLocation.x, optionLocation.y, 110, commands.Count * 25), "");
            for (int i = 0; i < commands.Count; i++) {
                if (GUI.Button(new Rect(optionLocation.x + 5, optionLocation.y + (i * 25), 100, 20), commands[i])) {
                    CallCommand(commands[i]);
                }
            }
        }
    }

    public void Init(StructureControl.StructureType type) {
        Type = type;
        images = StructureControl.Instance.GetPresets(Type);
        render = GetComponent<SpriteRenderer>();
        zoom = CameraControl.Instance.zoomPercent;
        serverCommands = new List<string>(StructureControl.Instance.GetCommands(Type));
        clientCommands = new Dictionary<string, System.Action>();
        buttonOverwrite = new Dictionary<string, System.Action<string>>();
        UpdateTextures();
    }

    public void UpdateStructure(float zoom) {
        this.zoom = zoom;
        // set texture based on zoom.
        if (zoom != oldZoom) {
            UpdateTextures();
            oldZoom = zoom;
        }
    }

    public void UpdateTextures() {
        for (int i = 0; i < images.presets.Count; i++) {
            if (zoom >= images.presets[i].zoomLevelRamge.x && zoom <= images.presets[i].zoomLevelRamge.y) {
                switch (Standings) {
                    case Standing.Own:
                        render.sprite = images.presets[i].ownImage;
                        break;

                    case Standing.Ally:
                        render.sprite = images.presets[i].allyImage;
                        break;

                    case Standing.Nuetral:
                        render.sprite = images.presets[i].nuetralImage;
                        break;

                    case Standing.Enemy:
                        render.sprite = images.presets[i].enemyImage;
                        break;

                    default:
                        render.sprite = images.presets[i].noneImage;
                        break;
                }
            }
        }
    }

    public void CallCommand(string cmd) {
        if (serverCommands.Contains(cmd)) {
            ShowButtons = false;
            NetworkController.Instance.SetStructureCommand(Location, cmd);
        }else if (clientCommands.ContainsKey(cmd)) {
            ShowButtons = false;
            clientCommands[cmd]();
        }else if (buttonOverwrite.ContainsKey(cmd)){
            buttonOverwrite[cmd](cmd);
            buttonOverwrite.Clear();
        }
    }

    public void AddCommand(string cmd, System.Action action) {
        if (!clientCommands.ContainsKey(cmd))
            clientCommands.Add(cmd, action);
    }

    public void ShowOptions(Vector2 mousePosition) {
        ShowButtons = true;
        optionLocation = mousePosition;
    }
}
