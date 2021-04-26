using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SimpleDragable : MonoBehaviour
{
    public GameObject target;
    private List<Vector3> path;
    private bool dragging = false;



    void Update() {
        if (target == null) return;

        if (path == null && !dragging) {
            path = Pathfinding.Instance.FindPath(this.transform.position, target.transform.position);
        }

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
        dragging = true;
    }

    void OnMouseUp() {
        if (dragging) {
            dragging = false;
            path = null;
        }
    }
}
