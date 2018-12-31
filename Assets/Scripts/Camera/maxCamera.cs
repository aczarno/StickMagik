//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{
  public Transform target;
  public Vector3 targetOffset;
  public float distance = 5.0f;
  public float maxDistance = 20;
  public float minDistance = .6f;
  public float xSpeed = 200.0f;
  public float ySpeed = 200.0f;
  public int yMinLimit = -80;
  public int yMaxLimit = 80;
  public int zoomRate = 40;
  public float panSpeed = 10f;
  public float zoomDampening = 5.0f;

  private float xDeg = 0.0f;
  private float yDeg = 0.0f;
  private float currentDistance;
  private float desiredDistance;
  private Quaternion currentRotation;
  private Quaternion desiredRotation;
  private Quaternion rotation;
  private Vector3 position;

  private Vector2 oldMousePosition = new Vector2(0, 0);
  private float pitch = 0f;
  private float yaw = 0f;

  void Start() { Init(); }
  void OnEnable() { Init(); }

  public void Init()
  {
    //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
    if (!target)
    {
      GameObject go = new GameObject("Cam Target");
      go.transform.position = transform.position + (transform.forward * distance);
      target = go.transform;
    }

    distance = Vector3.Distance(transform.position, target.position);
    currentDistance = distance;
    desiredDistance = distance;

    //be sure to grab the current rotations as starting points.
    position = transform.position;
    rotation = transform.rotation;
    currentRotation = transform.rotation;
    desiredRotation = transform.rotation;

    xDeg = Vector3.Angle(Vector3.right, transform.right);
    yDeg = Vector3.Angle(Vector3.up, transform.up);

    pitch = transform.eulerAngles.y;
    yaw = transform.eulerAngles.x;
  }

  /*
   * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
   */
  void LateUpdate()
  {
    //GUILayout.TextField();

    KeyPanCamera();
    MousePanCamera();

    CalculatePrincipalAxes();

    if (Input.GetMouseButtonDown(2))
    {
      oldMousePosition.x += Input.GetAxis("Mouse X");
      oldMousePosition.y += Input.GetAxis("Mouse Y");
    }
    /*
    // Zoom: ctrl + alt + Middle button
    if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
    {
      desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
    }
    // Orbit: left mouse + left alt are selected
    else if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt))
    {
      xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
      yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

      RotateCamera();
      
      ////////OrbitAngle

      //Clamp the vertical axis for the orbit
      yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
      // set camera rotation 
      desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
      currentRotation = transform.rotation;

      rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
      transform.rotation = rotation;
    }
    // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
    else if (Input.GetMouseButton(2))
    {
      transform.position += transform.forward * xSpeed * Input.GetAxis("Vertical");
      transform.position += transform.right * ySpeed * Input.GetAxis("Horizontal");
      //grab the rotation of the camera so we can move in a psuedo local XY space
      target.rotation = transform.rotation;
      target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
      target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
    }

    ////////Orbit Position

    // affect the desired Zoom distance if we roll the scrollwheel
    desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
    //clamp the zoom min/max
    desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    // For smoothing of the zoom, lerp distance
    currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

    // calculate position based on the new currentDistance 
    position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
    transform.position = position;
    */
  }

  private static float ClampAngle(float angle, float min, float max)
  {
    if (angle < -360)
      angle += 360;
    if (angle > 360)
      angle -= 360;
    return Mathf.Clamp(angle, min, max);
  }

  private void RotateCamera()
  {
    transform.eulerAngles = new Vector3(yDeg, xDeg, 0.0f);
  }

  private void KeyPanCamera()
  {
    // Forward and backward
    if (Input.GetKey(KeyCode.W))
      transform.position += transform.forward*Time.deltaTime*panSpeed;
    else if (Input.GetKey(KeyCode.S))
      transform.position -= transform.forward * Time.deltaTime * panSpeed;

    // Strafe right and left
    if (Input.GetKey(KeyCode.D))
      transform.position += transform.right * Time.deltaTime * panSpeed;
    else if (Input.GetKey(KeyCode.A))
      transform.position -= transform.right * Time.deltaTime * panSpeed;

    // Up and down
    if (Input.GetKey(KeyCode.E))
      transform.position += transform.up * Time.deltaTime * panSpeed;
    else if (Input.GetKey(KeyCode.Q))
      transform.position -= transform.up * Time.deltaTime * panSpeed;
  }

  private void MousePanCamera()
  {
    if(Input.GetMouseButton(2))
    {
      transform.position -= new Vector3(Input.GetAxis("Mouse X") - oldMousePosition.x, Input.GetAxis("Mouse Y") - oldMousePosition.y, 0f) * Time.deltaTime * panSpeed;

    }
  }

  // Yaw, Pitch
  private void CalculatePrincipalAxes()
  {
    if (Input.GetMouseButton(1))
    {
      pitch -= ySpeed * Input.GetAxis("Mouse Y");
      yaw += xSpeed * Input.GetAxis("Mouse X");

      transform.eulerAngles = new Vector3(pitch, yaw, transform.eulerAngles.z);
    }
  }
}