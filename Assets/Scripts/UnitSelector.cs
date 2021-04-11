using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    private static UnitSelector instance;
    private GameObject selectedMarker;
    private Vector2 selectionStart;
    private Dictionary<Collider2D, GameObject> selection = new Dictionary<Collider2D, GameObject>();
    private bool selecting = false;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            selectedMarker = Resources.Load("Prefabs/Tools/SelectedMarker") as GameObject;
            Debug.Assert(selectedMarker);
        }
        else if (instance != this)
        {
            // If we already have an instance destroy this new one
            Destroy(gameObject);
        }

    }

    void Update()
    {
        var mousePosition = GetMouseWorldPosition();

        if (Input.GetKeyDown(KeyCode.B))
        {
            var bunnies = FindObjectsOfType<Rabbit>().Select(rabbit => rabbit.GetComponent<Collider2D>());
            ChangeSelection(bunnies);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var selected = Physics2D.OverlapPoint(mousePosition)?.GetComponent<DragHandle>();

            if (selected)
            {
                // If a draggable unit is clicked then we don't to start selecting units
                selecting = false;
            }
            else
            {
                selecting = true;
                selectionStart = mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selecting)
            {
                var selectionEnd = GetMouseWorldPosition();

                var newSelection = Physics2D
                    .OverlapAreaAll(selectionStart, selectionEnd)
                    .Where(selected => selected.GetComponent<DragHandle>());

                ChangeSelection(newSelection);
            }
        }
    }

    private void ChangeSelection(IEnumerable<Collider2D> newSelection)
    {
        foreach (var marker in selection.Values)
        {
            Destroy(marker);
        }

        selection.Clear();

        foreach (var collider in newSelection)
        {
            Renderer parentRenderer = collider.GetComponent<SpriteRenderer>();
            Transform parent = collider.transform;
            GameObject marker = Instantiate(selectedMarker);
            marker.transform.parent = parent;
            marker.transform.localPosition = new Vector3(parentRenderer.bounds.size.x / 2, parentRenderer.bounds.size.y / 2);
            marker.transform.localScale = new Vector3(parent.localScale.x / 10, parent.localScale.y / 10);
            marker.SetActive(true);
            selection.Add(collider, marker);
        }
    }

    public static void Drag(Collider2D collider)
    {
        var offset = GetMouseWorldPosition() - collider.transform.position;

        if (instance.selection.ContainsKey(collider))
        {
            foreach (var selected in instance.selection.Keys)
            {
                selected.transform.position += offset;
            }
        }
        else
        {
            collider.transform.position += offset;
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Project mouse position to game surface
        return mousePosition;
    }
}
