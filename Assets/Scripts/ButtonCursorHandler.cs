using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonCursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.ResetCursor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Hover, force: true, looping: true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Click, force: true);
    }
}
