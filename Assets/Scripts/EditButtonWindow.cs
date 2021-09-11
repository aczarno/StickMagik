using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class EditButtonWindow : MonoBehaviour
{
  public ArcadeStick arcadeStick;

  public InputField posX;
  public Slider sliderX;
  public InputField posY;
  public Slider sliderY;
  public InputField rot;
  public Slider sliderRot;
  public RawImage cylinderColorPreview;
  public RawImage cylinderTexturePreview;
  public RawImage plungerColorPreview;
  public RawImage plungerTexturePreview;

  ButtonCP curButton = null;

  public void Start()
  {
    if (arcadeStick == null)
      arcadeStick = FindObjectOfType<ArcadeStick>();

    if (arcadeStick == null)
      Debug.LogError("EditButtonWindow: Can't find the arcade stick things will not work right at all.");
  }
  public void ShowWindow()
  {
    enabled = true;

    if (arcadeStick == null)
      return;

    // Just be extra safe
    curButton = null;

    ButtonCP newBtnProp = arcadeStick.GetCurrentSelectedComponenet() as ButtonCP;

    posX.text = newBtnProp.transform.position.x.ToString();
    sliderX.value = newBtnProp.transform.position.x;
    posY.text = newBtnProp.transform.position.y.ToString();
    sliderY.value = newBtnProp.transform.position.y;
    rot.text = newBtnProp.transform.rotation.eulerAngles.y.ToString();

    cylinderColorPreview.color = newBtnProp.cylinder.sharedMaterial.color;
    cylinderTexturePreview.texture = newBtnProp.cylinder.sharedMaterial.mainTexture;
    plungerColorPreview.color = newBtnProp.plunger.sharedMaterial.color;
    plungerTexturePreview.texture = newBtnProp.plunger.sharedMaterial.mainTexture;

    curButton = newBtnProp;
  }

  public void HideWindow()
  {
    enabled = false;

    curButton = null;

    posX.text = "";
    sliderX.value = 0f;
    posY.text = "";
    sliderY.value = 0f;
    rot.text = "";

    cylinderColorPreview.color = Color.white;
    cylinderTexturePreview.texture = null;
    plungerColorPreview.color = Color.white;
    plungerTexturePreview.texture = null;
  }

  public void OnChooseCylinderColor()
  {
    if (curButton == null)
      return;

    ColorPicker.Create(curButton.cylinder.sharedMaterial.color, "Edit Cylinder color", SetCylinderColor, SetCylinderColor);
  }

  void SetCylinderColor(Color _color)
  {
    if (cylinderColorPreview != null)
      cylinderColorPreview.color = _color;
    if (curButton != null)
      curButton.cylinder.sharedMaterial.color = _color;
  }

  public void OnChoosePlungerColor()
  {
    if (curButton == null)
      return;
    
    ColorPicker.Create(curButton.plunger.sharedMaterial.color, "Edit Plunger color", SetPlungerColor, SetPlungerColor);
  }

  void SetPlungerColor(Color _color)
  {
    if (plungerColorPreview != null)
      plungerColorPreview.color = _color;
    if (curButton != null)
      curButton.plunger.sharedMaterial.color = _color;
  }

  // TODO: Create a "ghost" button preview when moving slider around
  public void SetPositionOffset()
  {
    if (curButton == null)
      return;

    float newX = 0f;
    float newZ = 0f;

    // Set it to default if the parse fails
    newX = float.TryParse(posX.text, out newX) ? newX : curButton.transform.position.x;
    newZ = float.TryParse(posY.text, out newZ) ? newZ : curButton.transform.position.z;

    curButton.transform.position = new Vector3(newX, curButton.transform.position.y, newZ);
  }

  public void PosXSlideChange()
  {
    if (sliderX == null || curButton == null)
      return;
    posX.text = sliderX.value.ToString();
    SetPositionOffset();
  }

  public void PosYSlideChange()
  {
    if (sliderY == null || curButton == null)
      return;
    posY.text = sliderY.value.ToString();
    SetPositionOffset();
  }

  public void SetRotation()
  {
    if (curButton == null)
      return;

    float rotY = 0f;

    rotY = float.TryParse(rot.text, out rotY) ? rotY : curButton.transform.localRotation.eulerAngles.x;

    curButton.transform.localRotation = Quaternion.Euler(new Vector3(curButton.transform.localRotation.eulerAngles.x, rotY, curButton.transform.localRotation.eulerAngles.z));
  }

  public void RotSliderChange()
  {
    if (sliderRot == null || curButton == null)
      return;

    rot.text = sliderRot.value.ToString();
    SetRotation();
  }

  //[DllImport("user32.dll")]
  //private static extern void OpenFileDialog();
  
  public void onNewTexture(string _part)
  {
    if (curButton == null)
      return;
    RawImage texPreview = null;

    FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));
    FileBrowser.SetDefaultFilter(".jpg");

    /*OpenFileDialog ofd = new OpenFileDialog();
    if (ofd.ShowDialog() == DialogResult.OK)
    {
      byte[] rawData;
      Texture2D fill = new Texture2D(1, 1);

      rawData = File.ReadAllBytes(ofd.FileName);
      fill.LoadImage(rawData);
      texPreview.texture = fill;

      switch (_part)
      {
        case "Cylinder":
          cylinderTexturePreview.texture = fill;
          curButton.cylinder.material.mainTexture = fill;
          break;
        case "Plunger":
          plungerTexturePreview.texture = fill;
          curButton.plunger.material.mainTexture = fill;
          break;
        default:
          return;
      }
    }*/
    StartCoroutine(ShowLoadDialogCoroutine(_part));
  }

  IEnumerator ShowLoadDialogCoroutine(string _part)
  {
    // Show a load file dialog and wait for a response from user
    // Load file/folder: both, Allow multiple selection: true
    // Initial path: default (Documents), Initial filename: empty
    // Title: "Load File", Submit button text: "Load"
    yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

    // Dialog is closed
    // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
    Debug.Log(FileBrowser.Success);

    if (FileBrowser.Success)
    {
      // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
      for (int i = 0; i < FileBrowser.Result.Length; i++)
        Debug.Log(FileBrowser.Result[i]);

      Texture2D fill = new Texture2D(1, 1);

      // Read the bytes of the first file via FileBrowserHelpers
      // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
      byte[] rawData = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
      fill.LoadImage(rawData);

      switch (_part)
      {
        case "Cylinder":
          cylinderTexturePreview.texture = fill;
          curButton.cylinder.material.mainTexture = fill;
          break;
        case "Plunger":
          plungerTexturePreview.texture = fill;
          curButton.plunger.material.mainTexture = fill;
          break;
      }

      // Or, copy the first file to persistentDataPath
      //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
      //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
    }
  }
}
