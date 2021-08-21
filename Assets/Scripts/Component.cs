using UnityEngine;
using System.Collections;

public class Component : MonoBehaviour 
{
  public enum ComponentType
  {
    TRIM,
    BUTTON,
    JOYSTICK,
    BODY,
    PANEL,
    CORD,
    DECAL
  }

  public ComponentType componenttype;
  public Sprite icon = null;

  void OnMouseUpAsButton()
  {
    Messenger<GameObject>.Broadcast("ComponentSelected", gameObject);
  }
}
