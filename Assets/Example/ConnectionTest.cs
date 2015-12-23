using UnityEngine;
using System.Collections;
using System;

public class ConnectionTest : MonoBehaviour {
    public string host = "localhost";
    public int port = 11010;
    public int clientCount = 100;
    private WebSocket[] sockets;

    // Use this for initialization
    IEnumerator Start () {    
        for (int i = 0; i < clientCount; i++) {
            WebSocket socket = new WebSocket(new Uri("ws://Warworldinfinity.com:11010/connect"));
            yield return StartCoroutine(socket.Connect());
            socket.SendString("user" + i.ToString());
            while (true) {
                string reply = socket.RecvString();
                if (reply != null) {
                    Debug.LogFormat("Received: {0}", reply);
                    break;
                }
                if (socket.error != null) {
                    Debug.LogErrorFormat("Error: {0}", socket.error);
                }
            }
            socket.Close();
        }
        sockets = new WebSocket[clientCount];
        for (int i = 0; i < clientCount; i++){
            sockets[i] = new WebSocket(new Uri("ws://Warworldinfinity.com:11010/user" + i));
            yield return StartCoroutine(sockets[i].Connect());
            sockets[i].SendString("user" + i);
            StartCoroutine(Loop(i));
            //socket.Close();
        }
    }

    IEnumerator Loop(int i) {
        while (true) {
            string reply = sockets[i].RecvString();
            if (reply != null) {
                Debug.LogFormat("Received: {0}", reply);
            }
            if (sockets[i].error != null) {
                Debug.LogErrorFormat("Error: {0}", sockets[i].error);
            }
            yield return 0;
        }
    }

    void OnApplicationQuit() {
        if (sockets != null) {
            for (int i = 0; i < sockets.Length; i++) {
                if (sockets[i] != null)
                    sockets[i].Close();
            }
        }
    }
}
