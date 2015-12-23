using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using LibNoise.SerializationStructs;

public class NetClient : MonoBehaviour {
    public delegate void CMD(string data);
    public static NetClient Instance { get; private set; }
    public string userName;
    public string domain;
    public bool connected;
    public int DownBps;
    public int UpBps;
    public int ReceivedPs;
    public int SentPs;

    private WebSocket socket;
    private string host = "localhost";
    private int serverPort = 11010;
    private bool _run;
    private int _receivedBytes;
    private int _sentBytes;
    private int _received;
    private int _sent;

    private string _encryptionPass = "QH5SnB7eXckcAqa8yUGPbqEsQ1XL9eo";

    public Dictionary<string, CMD> Commands { get; private set; }

    void Awake() {
        Instance = this;
        Commands = new Dictionary<string, CMD>();
    }

    /*IEnumerator Start() {
        WebSocket echoSocket = new WebSocket(new Uri("ws://localhost:11010/echo"));
        yield return StartCoroutine(echoSocket.Connect());
        string sendstr = JsonConvert.SerializeObject(new Traffic("test", "hello"));
        echoSocket.Send(Encoding.UTF8.GetBytes(sendstr));
        while(true) {
            string reply = echoSocket.RecvString();
            if (reply != null) {
                Debug.Log(reply);
                break;
            }
            if (echoSocket.error != null) {
                Debug.LogErrorFormat("Error: {0}", echoSocket.error);
            }
            yield return 0;
        }
        echoSocket.Close();
    }*/

    public void Start() {
        /*string text = "{000:000:000:000:000:000:000:000}";
        //string text = "test";
        Debug.Log(text);
        string encrypted = HashHelper.Encrypt(text, "012345678");
        Debug.Log(encrypted);
        Debug.Log(HashHelper.Decrypt(encrypted, "012345678"));*/
    }

    void Update() {
        DownBps = _receivedBytes;
        _receivedBytes = 0;

        UpBps = _sentBytes;
        _sentBytes = 0;

        ReceivedPs = _received;
        _received = 0;

        SentPs = _sent;
        _sent = 0;
    }

    public void Connect(string host, int port, string user) {
        this.host = host;
        serverPort = port;
        userName = user;
        StartCoroutine(Connect_int());
    }

    public void AddCommand(string cmd, CMD callback) {
        if (!CommandExists(cmd.ToLower())) {
            Commands.Add(cmd.ToLower(), callback);
        }
    }

    public void RemoveCommand(string cmd) {
        if (CommandExists(cmd.ToLower()))
            Commands.Remove(cmd.ToLower());
    }

    public bool CommandExists(string Cmd) {
        return Commands.ContainsKey(Cmd.ToLower());
    }

    public void Send(string command, string data) {
        Send(new Traffic(command, data));
    }

    public void Send(Traffic traffic) {
        if (socket != null && connected) {
            _sentBytes += traffic.data.Length;
            _sent++;
            //Debug.Log("pre-encrypt");
            //string dataToSend = HashHelper.Encrypt(JsonConvert.SerializeObject(traffic), _encryptionPass);
            //Debug.Log("encrypt finished");
            //socket.Send(Convert.FromBase64String(dataToSend));
            //Debug.Log("pre-serialize");
            string sendStr = JsonConvert.SerializeObject(traffic);
            //Debug.Log("serialize finished.");
            socket.SendString(sendStr);
        }
        else
            Debug.LogError("Not connected!");
    }

    public string GetDownRate() {
        return GetDataRate(DownBps);
    }

    public string GetUpRate() {
        return GetDataRate(UpBps);
    }

    public string GetDataRate(float bytes) {
        float kBytes = bytes / 1024f;
        float mBytes = kBytes / 1024f;
        float gBytes = mBytes / 1024f;

        if (bytes < 1024)
            return bytes + " B/s";
        else if (kBytes < 1024)
            return kBytes.ToString("0.000") + " KB/s";
        else if (mBytes < 1024)
            return mBytes.ToString("0.000") + " MB/s";
        else
            return gBytes.ToString("0.000") + " GB/s";
    }

    public void Close() {
        if (socket != null)
            socket.Close();
        _run = false;
        connected = false;
        Debug.Log("network client closed");
    }

    IEnumerator Connect_int() {
        string hostStr = string.Format("ws://{0}:{1}/echo", host, serverPort);
        WebSocket connectSocket = new WebSocket(new Uri(hostStr));
        yield return StartCoroutine(connectSocket.Connect());
        connectSocket.SendString("echotest");
        while (true) {
            string reply = connectSocket.RecvString();
            if (reply != null) {
                if (reply == "echotest") {
                    Debug.Log("echo success");
                    connected = true;
                    StartCoroutine(Loop_int());
                    NetworkController.Instance.Login();
                    break;
                }
                /*if (reply.Contains("domain=") && reply.Contains("key=")) {
                    string[] parts = reply.Split('&');
                    string[] domainStr = parts[0].Split('=');
                    string[] keyStr = parts[1].Split('=');
                    domain = domainStr[1];
                    //_encryptionPass = keyStr[1];
                    connected = true;
                    StartCoroutine(Loop_int());
                }*/
            }
            if (connectSocket.error != null) {
                Debug.LogErrorFormat("Error: {0}", connectSocket.error);
                break;
            }
            yield return 0;
        }
        Debug.Log("Echo Socket closing...");
        connectSocket.Close();
    }

    IEnumerator Loop_int() {
        _run = true;
        string hostStr = string.Format("ws://{0}:{1}/server", host, serverPort);
        socket = new WebSocket(new Uri(hostStr));
        yield return StartCoroutine(socket.Connect());
        while (_run) {
            string reply = socket.RecvString();
            if (reply != null) {
                ProcessData(reply);
            }
            if (socket.error != null) {
                Debug.LogErrorFormat("Error: {0}", socket.error);
                _run = false;
            }
            yield return 0;
        }

        if (socket != null)
            socket.Close();
        connected = false;
    }

    private void ProcessData(string data) {
        _receivedBytes += data.Length * sizeof(char);
        _received++;
        //string decryptStr = HashHelper.Decrypt(data, _encryptionPass);
        Traffic traffic = JsonConvert.DeserializeObject<Traffic>(data);
        if (CommandExists(traffic.command)) {
            Commands[traffic.command.ToLower()](traffic.data);
        }
    }

    /*public const char Delimiter = '★';
	private const char endStr = '❤';

	public static NetClient Instance { get; private set;}

	public string ServerIp { get; set; }
	public int ServerPort { get; set; }
	public int ClientPort { get; set; }

	private UdpClient _client;
	private bool _run;
    private int _receivedBytes;
    private int _sentBytes;
    private int _received;
    private int _sent;

    public int DownBps { get; private set; }
    public int UpBps { get; private set; }
    public int ReceivedPs { get; private set; }
    public int SentPs { get; private set; }

    public Dictionary<string, CMD> Commands { get; private set; }

	public NetClient (string serverIp, int serverPort, int clientPort)
	{
		Instance = this;
		ServerIp = serverIp;
		ServerPort = serverPort;
		ClientPort = clientPort;
		_client = new UdpClient (ClientPort);
		Commands = new Dictionary<string, CMD> ();
		TaskQueue.QeueAsync ("NetThread", () => Receive());
	}

    public void Update()
    {

        DownBps = _receivedBytes;
        _receivedBytes = 0;

        UpBps = _sentBytes;
        _sentBytes = 0;

        ReceivedPs = _received;
        _received = 0;

        SentPs = _sent;
        _sent = 0;

        /*if (_downLabel != null && _upLabel != null) {
            _downLabel.Text = "down: " + GetDataRate(DownBps);
            _upLabel.Text = "up: " + GetDataRate(UpBps);
            _receivedLabel.Text = "received: " + ReceivedPs + " /s";
            _sentLabel.Text = "sent: " + SentPs + " /s";
        }
        
    }

    public string GetDownRate()
    {
        return GetDataRate(DownBps);
    }

    public string GetUpRate()
    {
        return GetDataRate(UpBps);
    }

    public string GetDataRate(float bytes)
    {
        float kBytes = bytes / 1024f;
        float mBytes = kBytes / 1024f;
        float gBytes = mBytes / 1024f;

        if (bytes < 1024)
            return bytes + " B/s";
        else if (kBytes < 1024)
            return kBytes.ToString("0.000") + " KB/s";
        else if (mBytes < 1024)
            return mBytes.ToString("0.000") + " MB/s";
        else
            return gBytes.ToString("0.000") + " GB/s";
    }

    public IPAddress DnsResolve(string host){
		return Dns.GetHostEntry (host).AddressList [0];
	}

	public void Send(string data){
		Send (Encoding.Unicode.GetBytes (data + endStr));
	}

	public void Send(byte[] data){
		if (_client != null) {
            _sentBytes += data.Length;
            _sent++;
            IPEndPoint endpoint = new IPEndPoint (IPAddress.Parse (ServerIp), ServerPort);
			_client.Send (data, data.Length, endpoint);
		} else
			Debug.Log ("Client is null!");
	}

	public void AddCommand(string cmd, CMD callback){
		if (!CommandExists (cmd.ToLower())) {
			Commands.Add(cmd.ToLower(), callback);
		}
	}
	
	public void RemoveCommand(string cmd){
		if (CommandExists (cmd.ToLower()))
			Commands.Remove (cmd.ToLower());
	}
	
	public bool CommandExists(string Cmd){
		return Commands.ContainsKey (Cmd.ToLower ());
	}

    public void Close() {
        _run = false;
        if (_client != null)
            _client.Close();
        Debug.Log("network client closed");
    }

    private void Receive()
    {
        _run = true;
        while (_run)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);
                byte[] data = _client.Receive(ref endPoint);
                TaskQueue.QueueMain(() => ProcessData(endPoint, data));
            }
            catch (SocketException e)
            {
                // do nothing.
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
            }
        }
    }

    private void ProcessData(IPEndPoint endPoint, byte[] data){
        _receivedBytes += data.Length;
        _received++;
        string inString = Encoding.Unicode.GetString (data);
		//Debug.Log (inString);
		if (inString.Contains (Delimiter.ToString()) && inString.Contains (endStr.ToString())) {
			string[] parts = inString.Substring(0, inString.IndexOf(endStr)).Split(Delimiter);
			if (parts.Length == 2){
				string cmd = parts[0];
				string args = parts[1];
				if (CommandExists (cmd.ToLower())){
					Commands[cmd.ToLower()](endPoint, args);
				}
			}
		}
	}*/


}


