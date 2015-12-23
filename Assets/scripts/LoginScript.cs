using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoginScript : MonoBehaviour {
    public static LoginScript Instance { get; private set; }
	public InputField userName;
	public InputField password;
    public bool keyboardOpen = false;
    private TouchScreenKeyboard keybaord;

    void Awake() {
        Instance = this;
    }

	// Use this for initialization
	void Start () {
        userName.Select();
        userName.ActivateInputField();
    }
	
	// Update is called once per frame
	void Update () {
        if (userName.isFocused) {
            if (!keyboardOpen) {
                keybaord = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
                keyboardOpen = true;
                Debug.Log("text keyboard opened");
            }
        }
        if (!password.isFocused) {
            if (!keyboardOpen) {
                keybaord = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true);
                keyboardOpen = true;
            }
        }
        if (Input.GetKeyDown (KeyCode.Return)) {
            NetworkController.Instance.Connect(userName.text, password.text);
		}
	}

    void OnGUI() {
        if (GUI.Button(new Rect(Screen.width / 2, 10, 500, 100), "echo")) {
            StartCoroutine(Echo());
        }
    }

    public void Login_Click() {
        NetworkController.Instance.Connect(userName.text, password.text);
    }

    IEnumerator Echo() {
        WebSocket w = new WebSocket(new Uri("ws://192.168.0.3:11010/echo"));
        yield return StartCoroutine(w.Connect());
        w.SendString("test");
        int i = 0;
        while (true) {
            string reply = w.RecvString();
            if (reply != null) {
                Debug.Log("Received: " + reply);
                break;
            }
            if (w.error != null) {
                Debug.LogError("Error: " + w.error);
                break;
            }
            yield return 0;
        }
        w.Close();
    }
}
