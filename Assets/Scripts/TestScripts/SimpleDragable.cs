using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SimpleDragable : MonoBehaviour
{
    public GameObject target;
    private List<Vector3> path;
    private const float ROTATION_ANGLE_PER_SECOND = 20f;
    private const float MOVEMENT_SPEED = 1f;

    void Start() {
        path = Pathfinding.Instance.FindPath(this.transform.position, target.transform.position, 0.5f);
    }
    void FixedUpdate() {
        if (target == null) return;

        if (path.Count == 0) {
            path = Pathfinding.Instance.FindPath(this.transform.position, target.transform.position, 0.5f);
        } else {
            if (Vector3.Distance(path[0], transform.position) < 0.1f) {
                path.RemoveAt(0);
            } else {
                var rigidbody = this.GetComponent<Rigidbody2D>();
                Vector3 vectorToTarget = path[0] - transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * ROTATION_ANGLE_PER_SECOND);
                rigidbody.velocity = transform.right * MOVEMENT_SPEED;
            }
        }
    }

    void Update() {
        if (path == null) return;

        Debug.DrawLine(transform.position, transform.position + transform.right, Color.red);
        var current = transform.position;
        for (int i = 0; i < path.Count; ++i) {
            var next = path[i];
            Debug.DrawLine(current, next, Color.green, 0, false);
            current = next;
        }
    }

    void OnMouseDrag() {
        var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        transform.position = position;
    }
}
