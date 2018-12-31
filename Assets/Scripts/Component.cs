using UnityEngine;
using System.Collections;

public class Component : MonoBehaviour 
{
  void OnMouseUpAsButton()
  {
    Messenger<GameObject>.Broadcast("ComponentSelected", gameObject);
  }
}
