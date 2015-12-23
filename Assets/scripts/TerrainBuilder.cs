using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.SerializationStructs;

public class TerrainBuilder : MonoBehaviour {
	public static TerrainBuilder Instance { get; private set; }
	public int seed;
	public int width;
	public int height;
    public bool Generated;
	public IModule module;
	public List<GradientPresets.GradientKeyData> preset;
	public LibNoise.Gradient gradient;
	public List<string> textureFiles;
	public List<Texture2D> terrainTextures;
	public Camera cam;
	public Texture2D texture;
	public Renderer render;
    public string time = "...";

	public Noise2D NoiseMap { get; private set; }

	private bool initiated;
	private bool allTexturesLoaded;
    private float[,] data;
    UnityEngine.Color[] colors;

    // Use this for initialization
    void Awake () {
		Instance = this;
		render = GetComponent<Renderer> ();
		render.enabled = false;
		//texture = (Texture2D)render.material.mainTexture;
	}
	
	// Update is called once per frame
	void Update () {
		if (initiated) {
			if (!allTexturesLoaded){
				if (terrainTextures.Count >= textureFiles.Count){
					allTexturesLoaded = true;
					GenerateImage();
				}
			}
		}
	}

    void OnGUI() {
        //GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 500, 100), "<size=40>Time: " + time + "</size>");
    }

	public void InitMap(MapData mapData){
		seed = mapData.seed;
		width = mapData.width;
		height = mapData.height;
		float imageWidth = width / 10;
		float imageHeight = height / 10;
		preset = mapData.gradient;
		textureFiles = new List<string>(mapData.terrainImages);
		CameraControl.Instance.SetSize (imageWidth, imageHeight);
		SetMesh (imageWidth, imageHeight);
		initiated = true;
	}

	public void AddTexture(string file, LibNoise.Color[] imageColors){
		GradientCreator.TextureFiles.Add(file, imageColors);
		UnityEngine.Color[] colors = ConvertColor(imageColors);
		Texture2D tex = new Texture2D(10, 10);
		tex.SetPixels(colors);
		tex.Apply();
		terrainTextures.Add(tex);
	}

	public void GenerateImage(){
		Debug.Log ("Generating Image");
		TaskQueue.QueueAsync ("generateTexture", () => {
			try {
                Noise2D.RunAsync += TaskQueue.QueueAsync;
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				gradient = GradientCreator.CreateGradientClient(preset);
				NoiseMap = new Noise2D (width, height, module);
                watch.Start();
                NoiseMap.GenerateSpherical (-90, 90, -180, 180);
                watch.Stop();
                time = watch.Elapsed.ToString();
                TaskQueue.QueueMain(()=>Debug.Log("Generate Time: " + time));
                data = NoiseMap.GetData();
                LibNoise.Color[] sColors = NoiseMap.GetTexture (gradient);
				colors = ConvertColor (sColors);
				TaskQueue.QueueMain(()=>{
					try {
						texture = new Texture2D(width, height);
						texture.SetPixels(colors);
						texture.Apply();
						render.material.mainTexture = texture;
						render.enabled = true;
                        Generated = true;
                        StructureControl.Instance.Init();
                    }
					catch (System.Exception e){
						Debug.LogException(e);
					}
					Debug.Log ("Image Generated.");
				});
			}
			catch (System.Exception e){
				TaskQueue.QueueMain(()=>Debug.LogException(e));
			}
		});
	}

    public float Value(int x, int y) {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return data[x, y];
        return 0;
    }

    public void SetPixel(int x, int y, UnityEngine.Color color) {
        colors[x + y * width] = color;
    }

    public void Apply() {
        texture = new Texture2D(width, height);
        texture.SetPixels(colors);
        texture.Apply();
        render.material.mainTexture = texture;
        render.enabled = true;
        Generated = true;
    }

	public static UnityEngine.Color[] ConvertColor(LibNoise.Color[] dColors){
        UnityEngine.Color[] newColors = new UnityEngine.Color[dColors.Length];
		for (int i = 0; i < newColors.Length; i++) {
			newColors[i] = new UnityEngine.Color((float)dColors[i].R / 255f, (float)dColors[i].G / 255f, (float)dColors[i].B / 255f, 1);
			//newColors[i] = new Color(100 / 255, 100 / 255, 100 / 255, 1);
		}
		return newColors;
	}

	private void SetMesh(float width, float height){
		List<Vector3> verts = new List<Vector3> ();
		verts.Add (new Vector3 (0, 0, 0));
		verts.Add (new Vector3 (0, 0, height));
		verts.Add (new Vector3 (width, 0, height));
		verts.Add (new Vector3 (width, 0, 0));

		List<int> tris = new List<int> (0);
		tris.Add (0);
		tris.Add (1);
		tris.Add (3);
		tris.Add (1);
		tris.Add (2);
		tris.Add (3);

		List<Vector2> UVs = new List<Vector2> ();
        UVs.Add(new Vector2(0, 0));
        UVs.Add(new Vector2(0, 1));
        UVs.Add(new Vector2(1, 1));
        UVs.Add(new Vector2(1, 0));

        //UVs.Add (new Vector2 (0, 1));
		//UVs.Add (new Vector2 (0, 0));
		//UVs.Add (new Vector2 (1, 0));
		//UVs.Add (new Vector2 (1, 1));

		Mesh mesh = new Mesh ();
		mesh.vertices = verts.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.uv = UVs.ToArray ();
		mesh.RecalculateNormals();

		GetComponent<MeshFilter> ().mesh = mesh;
		GetComponent<MeshCollider> ().sharedMesh = mesh;
	}
}
