using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatControl : MonoBehaviour {
    public static ChatControl Instance;

    private Rect chatRect = new Rect(0, 8 * (Screen.height / 10), 500, 2 * (Screen.height / 10));
    private string inStr = "";
    private GUIContent guiContent = new GUIContent();
    private List<string> messages = new List<string>();
    private Vector2 ScrollPos;

    void Awake() {
        Instance = this;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI() {
        chatRect = GUI.Window(0, chatRect, ChatWindow, "Chat");
    }

    void ChatWindow(int id) {
        var evt = Event.current;
        inStr = GUI.TextField(new Rect(0, 9 * (chatRect.height / 10) - 5, (chatRect.width), 20), inStr);
        if (evt.isKey && evt.type == EventType.KeyUp) {
            if (evt.keyCode == KeyCode.Return) {
                NetworkController.Instance.SubmitChat(inStr);
                inStr = string.Empty;
            }
        }
        Rect scrollRect = new Rect(0, 17, chatRect.width, chatRect.height - 37);
        Rect innerRect = new Rect(0, 0, chatRect.width - 17, chatRect.height - 9);
        guiContent.text = GetMessageString();
        float calcHeight = GUI.skin.textArea.CalcHeight(guiContent, chatRect.width - 17);
        innerRect.height = calcHeight < scrollRect.height ? scrollRect.height : calcHeight;

        ScrollPos = GUI.BeginScrollView(scrollRect, ScrollPos, innerRect, false, true);
        GUI.TextArea(innerRect, guiContent.text);
        GUI.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 1000, 1000));
    }

    string GetMessageString() {
        string result = string.Empty;
        foreach (string str in messages) {
            result += str + "\n";
        }
        return result;
    }

    public void AddText(string message) {
        messages.Add(message);
    }
}
