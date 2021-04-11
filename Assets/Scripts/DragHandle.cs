using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DragHandle : MonoBehaviour
{
    private new Collider2D collider;
    private float start;
    void Awake() {
        collider = this.GetComponent<Collider2D>();
    }

    void OnMouseDrag() {
        Debug.Log($"Dragging: {this.GetInstanceID()}");
        UnitSelector.Drag(collider);
    }

}
