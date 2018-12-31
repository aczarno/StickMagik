using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
  /// <summary>
  /// Transform of the actual panel we want to drag, default is the owner of this script
  /// </summary>
  public Transform panelToDrag;
  Vector2 pointerOffset;
  RectTransform canvasRectTrans;
  RectTransform panelRectTrans;

  void Awake()
  {
    Canvas can = GetComponentInParent<Canvas>();
    if (can != null)
    {
      if (panelToDrag == null)
        panelToDrag = transform;

      canvasRectTrans = (RectTransform)can.transform;
      panelRectTrans = (RectTransform)panelToDrag;
    }
  }

  public void OnPointerDown(PointerEventData e)
  {
    // Set as last thing to be rendered aka bring to front
    panelRectTrans.SetAsLastSibling();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTrans, e.position, e.pressEventCamera, out pointerOffset);

  }

  public void OnDrag(PointerEventData e)
  {
    if (panelRectTrans == null)
      return;

    Vector2 pointerPos = clampToWindow(e);
    Vector2 localPointerPos;
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTrans, pointerPos, e.pressEventCamera, out localPointerPos))
      panelRectTrans.localPosition = localPointerPos - pointerOffset;
  }

  Vector2 clampToWindow(PointerEventData e)
  {
    Vector2 rawPointerPos = e.position;

    Vector3[] canvasCorners = new Vector3[4];
    canvasRectTrans.GetWorldCorners(canvasCorners);

    float x = Mathf.Clamp(rawPointerPos.x, canvasCorners[0].x, canvasCorners[2].x);
    float y = Mathf.Clamp(rawPointerPos.y, canvasCorners[0].y, canvasCorners[2].y);

    return new Vector2(x, y);
  }
}
