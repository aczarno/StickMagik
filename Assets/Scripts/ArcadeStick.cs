using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcadeStick : MonoBehaviour
{
  List<Component> components = new List<Component>();
  public ListBox componentsList;
  public Material highlightMat = null;
  public PreviewCamera previewCam = null;
  public EditButtonWindow editBtnWindow = null;

  Component curSelectedComponent = null;

  // Start is called before the first frame update
  void Start()
  {
    if (highlightMat == null)
      Debug.LogError("Please set highlight material, so we can highlight components currently selected.");

    parseComponents();
  }

  private void OnEnable()
  {
    Messenger<Component>.AddListener("ComponentSelected", SelectComponent);
  }
  private void OnDisable()
  {
    Messenger<Component>.RemoveListener("ComponentSelected", SelectComponent);
  }

  public Component GetCurrentSelectedComponenet()
  {
    return curSelectedComponent;
  }

  /// <summary>
  /// Goes through our arcade stick object and find all the components so we can send the proper information out to our GUI
  /// </summary>
  void parseComponents()
  {
    //List<Component> children = new List<Component>();
    GetComponentsInChildren<Component>(components);
   
    // Send to the component ListBox GUI
    if (componentsList != null && components.Count > 0)
    {
      componentsList.Reset();
      componentsList.SetOptions(components.Select(comp => comp.name).ToArray(), components.Select(comp => comp.icon).ToArray());
      // Set up our value changed method
      componentsList.onValueChanged = OnListBoxValueChanged;
    }
  }

  public void OnListBoxValueChanged(ListBox aListBox)
  {
    if (components == null || components.Count == 0)
      return;

    // Find our selected component
    Component selected = components.Find(comp => comp.name == aListBox.valueString);

    // Something went horibly wrong
    if (selected == null)
    {
      Debug.LogError("Cannot find component: " + aListBox.valueString + " in our list of components.");
      return;
    }

    SelectComponent(selected);
  }

  /// <summary>
  /// Deselects any currently selected component object and selects and adds an outline material to the new selection.
  /// </summary>
  /// <param name="_selected">The newly selected component.</param>
  void SelectComponent(Component _selected)
  {
    List<Material> mats;
    List<Renderer> selectedRenderers = new List<Renderer>();
    int index = -1;
    // Unhighlight current selection if there is one
    if (curSelectedComponent != null)
    {
      selectedRenderers.AddRange(curSelectedComponent.GetComponentsInChildren<Renderer>());
      for (int i = 0; i < selectedRenderers.Count; i++)
      {
        mats = new List<Material>(selectedRenderers[i].materials);

        index = mats.FindIndex(comp => comp.name == highlightMat.name || comp.name == highlightMat.name + " (Instance)");

        if (index != -1)
          mats.RemoveAt(index);

        selectedRenderers[i].materials = mats.ToArray();
      }
    }

    curSelectedComponent = _selected;

    // If selected component was null then consider this a deselection and break out
    if (highlightMat == null || curSelectedComponent == null)
      return;

    selectedRenderers.Clear();
    selectedRenderers.AddRange(_selected.GetComponentsInChildren<Renderer>());
    // Highlight our new selection
    for (int i = 0; i < selectedRenderers.Count; i++)
    {
      mats = new List<Material>(selectedRenderers[i].materials);
      mats.Add(highlightMat);
      selectedRenderers[i].materials = mats.ToArray();
    }

    if (previewCam != null)
      previewCam.SetNewTarget(curSelectedComponent);

    // Fill and show component properties window
    UpdatePropertiesWindow(); 
  }

  void UpdatePropertiesWindow()
  {
    if (curSelectedComponent == null)
    {
      Debug.LogError("Trying to set up property windows for a component but there isn't one currently selected! NO BEUNO");
      return;
    }

    switch(curSelectedComponent.componenttype)
    {
      case Component.ComponentType.BUTTON:
        if (editBtnWindow == null)
        {
          Debug.LogError("ArcadeStick: EditBtnWindow not set, we can't display button component properties.");
          return;
        }

        editBtnWindow.ShowWindow();
        break;
      case Component.ComponentType.BODY:
      case Component.ComponentType.CORD:
      case Component.ComponentType.DECAL:
      case Component.ComponentType.JOYSTICK:
      case Component.ComponentType.PANEL:
      case Component.ComponentType.TRIM:
        break;
    }
  }
}
