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
  //public float panSpeed = 0.3f;
  public float zoomDampening = 5.0f;

  public float mouseSpeed = 100.0f;
  public float translateSpeed = 100.0f;
  public float scrollSpeed = 10f;
  public float panSpeed = 2f;

  private float xDeg = 0.0f;
  private float yDeg = 0.0f;
  private float currentDistance;
  private float desiredDistance;
  private Quaternion currentRotation;
  private Quaternion desiredRotation;
  private Quaternion rotation;
  private Vector3 position;

  private Vector3 lastMousePos;

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
  }

  void Update()
  {
    // If the debug menu is running, we don't want to conflict with its inputs.
    //if (DebugManager.instance.displayRuntimeUI)
    //return;

    float inputRotateAxisX = 0.0f;
    float inputRotateAxisY = 0.0f;
    // Update cam rotation on right click
    if (Input.GetMouseButton(1))
    {
      inputRotateAxisX = Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
      inputRotateAxisY = Input.GetAxis("Mouse Y") * mouseSpeed * Time.deltaTime;
    }

    /*float inputChangeSpeed = Input.GetAxis(kSpeedAxis);
    if (inputChangeSpeed != 0.0f)
    {
        m_MoveSpeed += inputChangeSpeed * m_MoveSpeedIncrement;
        if (m_MoveSpeed < m_MoveSpeedIncrement) m_MoveSpeed = m_MoveSpeedIncrement;
    }*/

    // W, S keys
    float inputVertical = Input.GetAxis("Vertical");
    // A, D keys
    float inputHorizontal = Input.GetAxis("Horizontal");
    // Q, E keys
    float inputStrafe = Input.GetAxis("Strafe");
    // Mouse scroll wheel
    float inputScroll = Input.GetAxis("Mouse ScrollWheel");

    if (inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f || inputHorizontal != 0.0f || inputStrafe != 0.0f || inputScroll != 0f)
    {
      float rotationX = transform.localEulerAngles.x;
      float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

      // Weird clamping code due to weird Euler angle mapping...
      float newRotationX = (rotationX - inputRotateAxisY);
      if (rotationX <= 90.0f && newRotationX >= 0.0f)
        newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
      if (rotationX >= 270.0f)
        newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

      transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

      float moveSpeed = Time.deltaTime * translateSpeed;
      // Speed boost
      /*if (Input.GetMouseButton(1))
          moveSpeed *= Input.GetKey(KeyCode.LeftShift) ? m_Turbo : 1.0f;
      else
          moveSpeed *= Input.GetAxis("Fire1") > 0.0f ? m_Turbo : 1.0f;*/
      transform.position += transform.forward * moveSpeed * inputVertical;
      transform.position += transform.forward * scrollSpeed * Time.deltaTime * inputScroll;
      transform.position += transform.right * moveSpeed * inputHorizontal;
      transform.position += Vector3.up * moveSpeed * inputStrafe;
    }

    // Pan cam on middle mouse btn down
    if (Input.GetMouseButtonDown(2))
    {
      lastMousePos = Input.mousePosition;
    }

    if (Input.GetMouseButton(2))
    {
      Vector3 delta = Input.mousePosition - lastMousePos;
      transform.Translate(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0f);
      lastMousePos = Input.mousePosition;
    }
  }

  /*
   * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
   */
  /*void LateUpdate()
  {
      // If Control and Alt + Middle button? ZOOM!
      if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
      {
          desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
      }
      // If middle mouse and left alt are selected? ORBIT
      else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt))
      {
          xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
          yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

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
  }*/

  private static float ClampAngle(float angle, float min, float max)
  {
    if (angle < -360)
      angle += 360;
    if (angle > 360)
      angle -= 360;
    return Mathf.Clamp(angle, min, max);
  }
}