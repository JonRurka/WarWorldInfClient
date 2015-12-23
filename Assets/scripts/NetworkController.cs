using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using LibNoise;
using LibNoise.SerializationStructs;

public class NetworkController : MonoBehaviour {
	public enum PermissionLevel
	{
		None,
		Observer,
		User,
		Moderator,
		Admin
	}
	
	public static NetworkController Instance{ get; private set;}

	public string host;
	public int serverPort;
	public bool Connected;
    public int timeoutTime = 5;
    public List<string> messges = new List<string>();
    public NetClient _client;
    public int tick = 0;

	private string _username;
	private string _password;
	private string _SessionKey;
	private bool _playing;
	private ManualResetEvent _wait;
	private string _key;

	// Use this for initialization
	void Awake () {
		Instance = this;
        if (_client == null)
		    _client = GetComponent<NetClient>();
		DontDestroyOnLoad (this);
		_wait = new ManualResetEvent (false);
	}

	void Start(){
		_playing = true;
		_key = HashHelper.RandomKey (100);
        AddNetworkCommands();
        //InvokeRepeating("UpdateClient", 0, 1);
        //Debug.Log (_key.Length * sizeof(char));
    }
	
	// Update is called once per frame
	void Update () {
		//SendTraffic ();
        
	}

    public void OnGUI() {
        for (int i = 0; i < messges.Count; i++) {
            GUI.Label(new Rect(10, i * 20, Screen.width, 20), messges[i]);
        }
    }

    void UpdateClient()
    {
        Debug.LogFormat("sent: {0}, {1}\nReceived: {2}, {3}",
        _client.SentPs, _client.GetUpRate(), _client.ReceivedPs, _client.GetDownRate());
    }

	void OnApplicationQuit() {
		_playing = false;
        Logout();
        _client.Close();
	}

	public void SendTrafficLoop(){
		while (_playing) {
			_wait.WaitOne(0);
			SendTraffic();
		}
	}

	public string JsonStr(object value){
		return JsonConvert.SerializeObject (value);
	}

    public void AddNetworkCommands() {
        _client.AddCommand("loginresponse", LoginResponse_CMD);
        _client.AddCommand("completelogin", CompleteLogin_CMD);
        _client.AddCommand("setstructurecommands", SetStructureCommands_CMD);
        _client.AddCommand("echo", Echo_CMD);
        _client.AddCommand("setterraindata", SetTerrainData_CMD);
        _client.AddCommand("setimage", SetImage_CMD);
        _client.AddCommand("setterrainmodule", SetTerrainModule_CMD);
        _client.AddCommand("setstructures", SetStructures_CMD);
        _client.AddCommand("opcreatesuccess", StructureCreateSuccess_CMD);
        _client.AddCommand("inchat", InChat_CMD);
        _client.AddCommand("tick", Tick_CMD);
        _client.AddCommand("message", Message_CMD);
    }

    public void Connect(string user, string password) {
        _username = user;
        _password = password;
        _client.Connect(host, serverPort, user);
        //StartCoroutine(Login_int());
    }

	public void Login(){
        _client.Send("getsalt", _username);
	}

	public void SendTraffic(){
		_client.Send ("traffic", _key);
	}

	public void RequestTerrain(){
		_client.Send ("getterrain", _SessionKey);
	}

    public void RequestOps() {
		GetStructures ops = new GetStructures(_SessionKey, GetStructures.RequestType.All, false, new Vector2Int(0,0), new Vector2Int(0,0));
		_client.Send("getstructures", JsonConvert.SerializeObject(ops));
    }

    public void RequestUserOpChanges() {
        GetStructures ops = new GetStructures(_SessionKey, GetStructures.RequestType.Owned, true, new Vector2Int(0, 0), new Vector2Int(0, 0));
        _client.Send("getstructures", JsonConvert.SerializeObject(ops));
    }

    public void RequestOpChanges() {
        GetStructures ops = new GetStructures(_SessionKey, GetStructures.RequestType.All, true, new Vector2Int(0, 0), new Vector2Int(0, 0));
        _client.Send("getstructures", JsonConvert.SerializeObject(ops));
    }

    public void CreateOp(Vector2Int location, StructureControl.StructureType type) {
        SetStructure setStructure = new SetStructure(_SessionKey, location, type.ToString());
        _client.Send("createstructure", JsonConvert.SerializeObject(setStructure));
    }

    public void ChangeOp(Vector2Int location, StructureControl.StructureType type) {
        SetStructure setStructure = new SetStructure(_SessionKey, location, type.ToString());
        _client.Send("changestructure", JsonConvert.SerializeObject(setStructure));
    }

    public void SetStructureCommand(Vector2Int location, string command) {
        StructureCommand cmd = new StructureCommand(_SessionKey, location, command);
        _client.Send("structurecommand", JsonConvert.SerializeObject(cmd));
    }

    public void SubmitChat(string message) {
        ChatMessage msg = new ChatMessage(_SessionKey, _username, message);
        _client.Send("inchat", JsonConvert.SerializeObject(msg));
    }

    public void Logout() {
        _client.Send("logout", _SessionKey);
    }

	// COMMANDS

	private void CompleteLogin_CMD(string args){
		Login login = new Login (_username, HashHelper.HashPasswordClient(_password, args), args);
        _client.Send ("login", JsonStr(login));
	}

	private void LoginResponse_CMD(string args){
		//Debug.Log (args);
		LoginResponse response = JsonConvert.DeserializeObject<LoginResponse> (args);
		Debug.Log (string.Format ("login response: {0}, {1}, {2}, {3}", response.response, response.permission, response.sessionKey, response.message));
        messges.Add(string.Format("login response: {0}, {1}, {2}, {3}", response.response, response.permission, response.sessionKey, response.message));
        if (response.response == ResponseType.Successfull) {
			_SessionKey = response.sessionKey;
            tick = response.tick;
			Application.LoadLevel("Game");
		}
	}

    private void SetStructureCommands_CMD(string args) {
        CommandList[] commands = JsonConvert.DeserializeObject<CommandList[]>(args);
        StructureControl.Instance.SetStructureCommands(commands);
    }

	private void Echo_CMD(string args){
        Debug.Log(args);
	}

    private void SetTerrainModule_CMD(string args){
		IModule module = JsonConvert.DeserializeObject<IModule> (args, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
		TerrainBuilder.Instance.module = module;
        //Debug.Log("terrain module set");
	}

    private void SetTerrainData_CMD(string args){
		MapData data = JsonConvert.DeserializeObject<MapData> (args, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
		Debug.LogFormat ("Map data: {0}, {1}, {2}, {3}", data.response, data.width, data.height, data.message);
        if (data.response == ResponseType.Successfull) {
			TerrainBuilder.Instance.InitMap (data);
            //Debug.Log("terrain data set");
        } else {
			Debug.LogFormat("Failed-{0}: {1}", data.response , data.message);
		}
	}

    private void SetImage_CMD(string args){
		ImageFileData imageData = JsonConvert.DeserializeObject<ImageFileData> (args);
        if (TerrainBuilder.Instance.textureFiles.Contains(imageData.file)) {
            TerrainBuilder.Instance.AddTexture(imageData.file, imageData.image);
        }
        else
            Debug.LogFormat("received invalid texture: {0}", imageData.file);
    }

    private void SetStructures_CMD(string args) {
        LibNoise.SerializationStructs.Structure[] structures = JsonConvert.DeserializeObject<LibNoise.SerializationStructs.Structure[]>(args);
        Debug.Log("structures received: " + structures.Length);
        for (int i = 0; i < structures.Length; i++) {
            Debug.Log("Type: " + structures[i].type.ToString());
        }
        StructureControl.Instance.SetStructures(structures);
    }

    private void StructureCreateSuccess_CMD(string args) {
        if (args == "success") {
            Debug.Log("op create success!");
            RequestUserOpChanges();
        }
    }

    private void Tick_CMD(string args) {
        tick = int.Parse(args);
        RequestOpChanges();
        // TODO: request unit and resource changes.
    }

    private void InChat_CMD(string args) {
        ChatMessage message = JsonConvert.DeserializeObject<ChatMessage>(args);
        ChatControl.Instance.AddText(message.player + ": " + message.message);
    }

    private void Message_CMD(string args) {
        Message serverMessage = JsonConvert.DeserializeObject<Message>(args);
        Debug.Log(serverMessage.type.ToString());
    }
    
    IEnumerator Login_int() {
        if (_client != null) {
            Debug.Log("Logging in...");
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (true) {
                if (_client.connected) {
                    Debug.Log("Connected");
                    Connected = true;
                    Login();
                    break;
                }
                if (watch.Elapsed.TotalSeconds >= timeoutTime) {
                    Debug.LogError("Login timeout.");
                    break;
                }
                yield return 0;
            }
        }
        else
            Debug.LogError("Client null failed to log in.");
    }
}
