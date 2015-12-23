using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnLevelWasLoaded(int level) {
		string levelStr = Application.loadedLevelName;
		if (levelStr == "Game") {
			NetworkController.Instance.RequestTerrain();
		}
	}
}
