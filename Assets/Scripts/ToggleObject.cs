using UnityEngine;
using System.Collections;

public class ToggleObject : MonoBehaviour 
{
  public void Toggle(GameObject o)
  {
    o.SetActive(!o.activeSelf);
  }
}
