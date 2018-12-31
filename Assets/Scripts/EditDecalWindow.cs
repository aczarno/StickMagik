using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;

public class EditDecalWindow : MonoBehaviour {

  public RawImage texturePreview = null;
  public InputField rotInput = null;
  public InputField posXInput = null;
  public InputField posYInput = null;
  public InputField scaleXInput = null;
  public InputField scaleYInput = null;

  Renderer curSelectedComponent = null;

  void Start()
  {
    if (texturePreview == null)
      Debug.LogError("Texture image need to defined in the editor.");
  }

  void OnEnable()
  {
  }

  void OnDisable()
  {
  }

  void onComponentSelected(GameObject _component)
  {
    curSelectedComponent = _component.GetComponent<Renderer>();
    texturePreview.texture = curSelectedComponent.material.mainTexture;
  }

  [DllImport("user32.dll")]
  private static extern void OpenFileDialog();

  public void openNewTexture()
  {
    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    {
      byte[] rawData;
      Texture2D fill = new Texture2D(1, 1);

      rawData = File.ReadAllBytes(ofd.FileName);
      fill.LoadImage(rawData);
      texturePreview.texture = fill;
      if (curSelectedComponent != null)
      {
        Renderer ren = curSelectedComponent.GetComponent<Renderer>();
        ren.material.mainTexture = fill;
      }
    }
  }

  /// <summary>
  /// Try to update the positionX dynamically, making sure the user isn't inputing garbage.
  /// </summary>
  public void tryUpdatePosX(string _value)
  {
    float newPosX = 0;
    if (!float.TryParse(_value, out newPosX))
      return;

    Vector2 curOffset = curSelectedComponent.material.GetTextureOffset("_MainTex");
    updatePos(newPosX, curOffset.y);
  }

  /// <summary>
  /// Slider position X handler. Update the offset position X of the material.
  /// </summary>
  /// <param name="_value">New X position offset</param>
  public void slidePosX(float _value)
  {
    Vector2 curOffset = curSelectedComponent.material.GetTextureOffset("_MainTex");
    updatePos(_value, curOffset.y);
  }

  /// <summary>
  /// Slider position Y handler. Update the offset position Y of the material.
  /// </summary>
  /// <param name="_value">New Y position offset</param>
  public void slidePosY(float _value)
  {
    Vector2 curOffset = curSelectedComponent.material.GetTextureOffset("_MainTex");
    updatePos(curOffset.x, _value);
  }

  /// <summary>
  /// Try to update the positionY dynamically, making sure the user isn't inputing garbage.
  /// </summary>
  public void tryUpdatePosY(string _value)
  {
    float newPosY = 0;
    if (!float.TryParse(_value, out newPosY))
      return;

    Vector2 curOffset = curSelectedComponent.material.GetTextureOffset("_MainTex");
    updatePos(curOffset.x, newPosY);
  }

  /// <summary>
  /// Update the offset position of the material.
  /// </summary>
  /// <param name="_x">New offset X</param>
  /// <param name="_y">New offset Y</param>
  void updatePos(float _x, float _y)
  {
    posXInput.text = _x.ToString();
    posYInput.text = _y.ToString();

    texturePreview.material.SetTextureOffset("_MainTex", new Vector2(_x, _y));
    curSelectedComponent.material.SetTextureOffset("_MainTex", new Vector2(_x, _y));
  }

  /// <summary>
  /// Rotation input handler. Try to update the rotation dynamically, making sure the user isn't inputing garbage.
  /// </summary>
  /// <param name="_value">Input field value, should be a number 0-360.</param>
  public void tryUpdateRot(string _value)
  {
    float newRot = 0;
    if (!float.TryParse(_value, out newRot))
      return;

    updateRot(newRot);
  }

  /// <summary>
  /// Rotation slider handler. Update the rotation of the material.
  /// </summary>
  /// <param name="_value">A degree 0-360.</param>
  public void updateRot(float _value)
  {
    rotInput.text = _value.ToString();
    curSelectedComponent.material.SetFloat("_Rotation", Mathf.Deg2Rad * _value);
  }

  /// <summary>
  /// Scale X input handler. Try to update the scaleX dynamically, making sure the user isn't inputing garbage.
  /// </summary>
  public void tryUpdateScaleX(string _value)
  {
    float newScaleX = 0;
    if (!float.TryParse(_value, out newScaleX))
      return;

    Vector2 curScale = curSelectedComponent.material.GetTextureScale("_MainTex");
    updateScale(newScaleX, curScale.y);
  }

  /// <summary>
  /// Scale Y input handler. Try to update the scaleY dynamically, making sure the user isn't inputing garbage.
  /// </summary>
  public void tryUpdateScaleY(string _value)
  {
    float newScaleY = 0;
    if (!float.TryParse(_value, out newScaleY))
      return;

    Vector2 curScale = curSelectedComponent.material.GetTextureScale("_MainTex");
    updateScale(curScale.x, newScaleY);
  }

  /// <summary>
  /// Slider scale X handler. Update X scale of the material.
  /// </summary>
  /// <param name="_value">New X scale</param>
  public void slideScaleX(float _value)
  {
    Vector2 curScale = curSelectedComponent.material.GetTextureScale("_MainTex");
    updateScale(_value, curScale.y);
  }

  /// <summary>
  /// Slider scale Y handler. Update Y scale of the material.
  /// </summary>
  /// <param name="_value">New Y scale</param>
  public void slideScaleY(float _value)
  {
    Vector2 curScale = curSelectedComponent.material.GetTextureScale("_MainTex");
    updateScale(curScale.x, _value);
  }

  /// <summary>
  /// Update material scale.
  /// </summary>
  /// <param name="_x">New scale X</param>
  /// <param name="_y">New scale Y</param>
  void updateScale(float _x, float _y)
  {
    scaleXInput.text = _x.ToString();
    scaleYInput.text = _y.ToString();

    curSelectedComponent.material.SetTextureScale("_MainTex", new Vector2(_x, _y));
  }
}
