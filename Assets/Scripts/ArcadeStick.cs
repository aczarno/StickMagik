using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcadeStick : MonoBehaviour
{
  List<Component> components = new List<Component>();
  public ListBox componentsList;
  public Material highlightMat = null;

  Component curSelectedComponent = null;

  // Start is called before the first frame update
  void Start()
  {
    if (highlightMat == null)
      Debug.LogError("Please set highlight material, so we can highlight components currently selected.");

    parseComponents();
  }

  // Update is called once per frame
  void Update()
  {

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

    Material[] mats;
    List<Renderer> selectedRenderers = new List<Renderer>();
    // Unhighlight current selection if there is one
    if (curSelectedComponent != null)
    {
      selectedRenderers.AddRange(curSelectedComponent.GetComponentsInChildren<Renderer>());
      for (int i = 0; i < selectedRenderers.Count; i++)
      {
        mats = new Material[selectedRenderers[i].materials.Length-1];

        // Cut off the last material
        for(int j=0; j<mats.Length; j++)
        {
          mats[j] = selectedRenderers[i].materials[j];
        }
        
        selectedRenderers[i].materials = mats;
      }
      
    }

    // Find our selected component
    curSelectedComponent = components.Find(comp => comp.name == aListBox.valueString);

    // Something went horibly wrong
    if (curSelectedComponent == null)
    {
      Debug.LogError("Cannot find component: " + aListBox.valueString + " in our list of components.");
      return;
    }

    if (highlightMat == null)
      return;
    selectedRenderers.Clear();
    selectedRenderers.AddRange(curSelectedComponent.GetComponentsInChildren<Renderer>());
    for (int i = 0; i < selectedRenderers.Count; i++)
    {
      mats = new Material[selectedRenderers[i].materials.Length+1];
      selectedRenderers[i].materials.CopyTo(mats, 0);
      //selectedRenderers[i]?.GetMaterials(mats);
      mats[mats.Length-1] = highlightMat;
      selectedRenderers[i].materials = mats;
    }

   
        
  }
}
