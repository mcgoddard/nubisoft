using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SimpleDragable : MonoBehaviour
{
    public GameObject target;
    private List<Vector3> path;

    private Vector3 position;

    void FixedUpdate() {
        if (target == null) return;

        if (path == null || position != transform.position) {
            path = Pathfinding.Instance.FindPath(this.transform.position, target.transform.position, 0.5f);
        }
    }

    void Update() {
        if (path == null) return;

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
