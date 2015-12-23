using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	public static CameraControl Instance { get; private set;}

	public Camera cam;
	public float imageWidth;
	public float imageHeight;
	public float width;
	public float height;
	public float currentSize;
	public float minSize = 5;
	public float maxSize;
	public float scrollSpeed = 10;
	public float moveSpeed = 10;
    public float zoomPercent = 100;
	public Vector3 originPosition;
	public Vector3 mousePosition;
	public Vector2 scrollMouseDelta;
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;

    public float mouseX;
	public float mouseY;
	public bool mouseInGrid;

    private bool selectingStructure;
    private Structure curSelectedStructure;
    private float curZoom;
    private Vector3 curCamPos;
    private Vector2 onGuiMousePos;
	
	// Use this for initialization
	void Start () {
		Instance = this;
		cam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		height = 2 * cam.orthographicSize;
		width = height * cam.aspect;

		RaycastHit hit;
		Ray ray = cam.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit, 150)) {
			mousePosition = new Vector3 (hit.point.x, hit.point.y, 0);
			scrollMouseDelta = Input.mouseScrollDelta;
			float scroll = scrollMouseDelta.y;
			if (scroll != 0) {
				currentSize -= scroll * scrollSpeed * Time.deltaTime;
				if (currentSize <= minSize) {
					currentSize = minSize;
				}
				if (currentSize >= maxSize) {
					currentSize = maxSize;
				}
				cam.orthographicSize = currentSize;
				//Vector3 newCamPos = Vector3.Lerp(originPosition, mousePosition, GetPercent(minSize, currentSize, maxSize));
				//transform.position = new Vector3(newCamPos.x, newCamPos.y, -10);
			}

			Vector2 topLeft = new Vector2(transform.position.x - width / 2, transform.position.y + height / 2);
			Vector2 topRight = new Vector2(transform.position.x + width / 2, transform.position.y + height / 2);
			Vector2 bottomLeft = new Vector2(transform.position.x - width / 2, transform.position.y - height / 2);
			Vector2 bottomRight = new Vector2(transform.position.x + width / 2, transform.position.y - height / 2);
            zoomPercent = GetPercent(0, currentSize, maxSize);

            if (topLeft.x <= 0)
				transform.position = new Vector3(width / 2, transform.position.y, -10);
			if (topLeft.y >= imageHeight)
				transform.position = new Vector3(transform.position.x, imageHeight - height / 2, -10);
			
			if (bottomRight.x >= imageWidth)
				transform.position = new Vector3(imageWidth - width / 2, transform.position.y, -10);
			if (bottomRight.y <= 0)
				transform.position = new Vector3(transform.position.x, height / 2, -10);

            if (!selectingStructure) {
                if (Input.GetMouseButtonDown(0)) {
                    if (hit.collider.tag == "structure" ) {
                        curSelectedStructure = hit.collider.GetComponent<Structure>();
                        curSelectedStructure.ShowOptions(onGuiMousePos);
                        curZoom = zoomPercent;
                        curCamPos = transform.position;
                        selectingStructure = true;
                    }
                }
                if (Input.GetMouseButton(0)) {
                    mouseX = Input.GetAxis("Mouse X");
                    mouseY = Input.GetAxis("Mouse Y");
                    if (!StructureControl.Instance.createOp)
                        transform.Translate(
                            -mouseX * moveSpeed * Time.deltaTime * zoomPercent,
                            -mouseY * moveSpeed * Time.deltaTime * zoomPercent, 0);
                }
            }
            

            if (curSelectedStructure != null){
                if (!curSelectedStructure.ShowButtons) {
                    curSelectedStructure = null;
                    selectingStructure = false;
                }
                else if (curZoom != zoomPercent || curCamPos != transform.position) {
                    curSelectedStructure.ShowButtons = false;
                }
            }
            Debug.DrawLine(ray.origin, hit.point, Color.red);
			mouseInGrid = true;
		} else
			mouseInGrid = false;
	}

    void OnGUI() {
        onGuiMousePos = Event.current.mousePosition;
    }

	public void SetSize(float imageWidth, float imageHeight){
		this.imageWidth = imageWidth;
		this.imageHeight = imageHeight;
		float halfWidth = imageWidth / 2;
		float halfHeight = imageHeight / 2;
		transform.position = new Vector3 (halfWidth, halfHeight, -10);
		originPosition = transform.position;
		cam.aspect = imageWidth / (float)imageHeight;
		maxSize = ((imageWidth / 2) / cam.aspect);
		currentSize = maxSize;
        cam.orthographicSize = maxSize;
        zoomPercent = GetPercent(0, currentSize, maxSize);
        //width = (2 * orthSize) * apsect.
    }

	private float GetPercent(float bottom, float middle, float top){
		float range = top - bottom;
		float bottomRange = middle - bottom;
		return bottomRange / range;
	}

}
