using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCP : Component
{
  // Editable properties
  // Button position
  // Button rotation
  // Cylinder color
  // Cylinder texture
  // Plunger color
  // Plunger texture

  public ColorPicker colorPicker;

  public MeshRenderer cylinder;
  public MeshRenderer plunger;

  void Awake()
  {
    componenttype = ComponentType.BUTTON;

    // Try to grab our subcomponents if they weren't set
    if (cylinder == null || plunger == null)
      GrabSubComponents();

    if(colorPicker == null)
      colorPicker = FindObjectOfType<ColorPicker>();
  }

  /// <summary>
  /// Try and find our subcomponents based on object names in our children.
  /// </summary>
  void GrabSubComponents()
  {
    GameObject child;
    for(int i=0; i<transform.childCount; i++)
    {
      child = transform.GetChild(i).gameObject;
      if(child.name.ToLower().Contains("cylinder") && child.GetComponent<MeshRenderer>() != null)
      {
        cylinder = child.GetComponent<MeshRenderer>();
      }
      else if (child.name.ToLower().Contains("plunger") && child.GetComponent<MeshRenderer>() != null)
      {
        plunger = child.GetComponent<MeshRenderer>();
      }
    }
  }

  public void OnChooseCylinderColor()
  {
    if (colorPicker == null)
      return;

    
  }

  public void SetButtonPos(Vector3 _pos)
  {

  }

  public void SetButtonRot(Vector3 _rot)
  {

  }

  public void SetCylinderColor(Color _color)
  {
    cylinder.sharedMaterial.color = _color;
  }

  public void SetCylinderTexture(Texture _texture)
  {

  }

  public void SetPlungerColor(Color _color)
  {

  }

  public void SetPlungerTexture(Texture _texture)
  {

  }
}
