using UnityEngine;
using System.Collections;
using System;

public class EchoTest : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		WebSocket w = new WebSocket(new Uri("ws://localhost:11010/echo"));
		yield return StartCoroutine(w.Connect());
		w.SendString("test");
		int i=0;
		while (true)
		{
			string reply = w.RecvString();
			if (reply != null)
			{
				Debug.Log ("Received: "+reply);
                break;
			}
			if (w.error != null)
			{
				Debug.LogError ("Error: "+w.error);
				break;
			}
			yield return 0;
		}
		w.Close();
	}
}
