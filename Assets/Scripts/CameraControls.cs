using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControls : MonoBehaviour
{
    private Bounds cameraBounds;
    private const int EDGE_PAN_BUFFER = 100;
    private const float CAMERA_PAN_SPEED = 10;
    private const float MIN_ZOOM_LEVEL = 3.5f;
    private const float MAX_ZOOM_LEVEL = 12f;
    private new Camera camera;
    private Vector3 middleMouseDownPosition;
    // Start is called before the first frame update
    void Start()
    {
        camera = this.GetComponent<Camera>();
        cameraBounds =  new Bounds(Vector3.zero, new Vector3(20, 20f, Mathf.Infinity));
    }

    // Update is called once per frame
    void Update()
    {
        // Zoom camera based on how much the scroll wheel has moved
        this.camera.orthographicSize = Mathf.Clamp(this.camera.orthographicSize - Input.mouseScrollDelta.y, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);

        if (Input.GetMouseButton(2)) {
            if (Input.GetMouseButtonDown(2)) {
                // If this is the first frame that middle mouse button was pressed,
                // clobber the last position as it is no longer valid
                middleMouseDownPosition = Input.mousePosition;
            }

            // Move camera based on the offset from the current mouse position and the mouse
            // position in the previous frame.
            Vector3 target = -Camera.main.ScreenToViewportPoint(Input.mousePosition - middleMouseDownPosition) * CAMERA_PAN_SPEED;
            transform.Translate(target, Space.World);

            // Update to the current mouse position so we so we can compute the new delta on the next frame
            middleMouseDownPosition = Input.mousePosition;
        } else {
            // Handle edge pan base on mouse position
            if (Input.mousePosition.y >= Screen.height - EDGE_PAN_BUFFER) {
                transform.position += Vector3.up * CAMERA_PAN_SPEED * Time.deltaTime;
            }
            if (Input.mousePosition.y <= EDGE_PAN_BUFFER) {
                transform.position += Vector3.down * CAMERA_PAN_SPEED * Time.deltaTime;
            }
            if (Input.mousePosition.x >= Screen.width - EDGE_PAN_BUFFER) {
                transform.position += Vector3.right * CAMERA_PAN_SPEED * Time.deltaTime;
            }
            if (Input.mousePosition.x <= EDGE_PAN_BUFFER) {
                transform.position += Vector3.left * CAMERA_PAN_SPEED * Time.deltaTime;
            }
        }

        // Constrain the new position to fit within the camera bounds
        transform.position = cameraBounds.ClosestPoint(transform.position);
    }
    public static Vector3 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public float GetZoomLevel() {
        var zoomLevel = (this.camera.orthographicSize - MIN_ZOOM_LEVEL) / (MAX_ZOOM_LEVEL - MIN_ZOOM_LEVEL);
        Debug.Assert(zoomLevel >= 0 && zoomLevel <= 1.0);
        return zoomLevel;
    }
}
