using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControls : MonoBehaviour
{
    private const int EDGE_PAN_BUFFER = 100;
    private const float CAMERA_PAN_SPEED = 10;
    private const float MIN_ZOOM_LEVEL = 3.5f;
    private const float MAX_ZOOM_LEVEL = 12f;
    private new Camera camera;
    private Vector3 middleMouseDownPosition;
    // Start is called before the first frame update
    void Start()
    {
        this.camera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        this.camera.orthographicSize = Mathf.Clamp(this.camera.orthographicSize - Input.mouseScrollDelta.y, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);

        if (Input.GetMouseButtonDown(2)) {
            middleMouseDownPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) {
            Vector3 target = -Camera.main.ScreenToViewportPoint(Input.mousePosition - middleMouseDownPosition) * CAMERA_PAN_SPEED;

            transform.Translate(target, Space.World);
            middleMouseDownPosition = Input.mousePosition;
        }

        if (Input.mousePosition.y >= Screen.height - EDGE_PAN_BUFFER) {
            this.transform.position += Vector3.up * CAMERA_PAN_SPEED * Time.deltaTime;
        }
        if (Input.mousePosition.y <= EDGE_PAN_BUFFER) {
            this.transform.position += Vector3.down * CAMERA_PAN_SPEED * Time.deltaTime;
        }
        if (Input.mousePosition.x >= Screen.width - EDGE_PAN_BUFFER) {
            this.transform.position += Vector3.right * CAMERA_PAN_SPEED * Time.deltaTime;
        }
        if (Input.mousePosition.x <= EDGE_PAN_BUFFER) {
            this.transform.position += Vector3.left * CAMERA_PAN_SPEED * Time.deltaTime;
        }
    }
    public static Vector3 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
