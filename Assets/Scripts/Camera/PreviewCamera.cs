using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewCamera : MonoBehaviour
{
  public GameObject target = null;
  public float speed = 10f;
  public Shader shade = null;

  Vector3 tarPosition = Vector3.zero;

  // Start is called before the first frame update
  void Start()
  {
    transform.LookAt(target.transform);
  }

  private void OnEnable()
  {
    GetComponent<Camera>().SetReplacementShader(shade, null);
  }

  private void OnDisable()
  {
    GetComponent<Camera>().ResetReplacementShader();
  }
  // Update is called once per frame
  void Update()
  {
    //if (shade != null)
      //GetComponent<Camera>().RenderWithShader(shade, "Preview");

    if (target != null)
    {
      
      //transform.RotateAround(target.transform.position, Vector3.up, speed * Time.deltaTime);
      //transform.Translate(Vector3.right * Time.deltaTime);
    }
  }

  public void SetNewTarget(Component _tar)
  {
    if (_tar == null)
      return;
    // Retrun old target to default layer
    ChangeLayer(target, LayerMask.NameToLayer("Default"));

    target = _tar.gameObject;
    ChangeLayer(target, LayerMask.NameToLayer("Preview"));

    tarPosition = FindAveragePosition(target.transform);
    // Offset cam a bit so we get a nice full view
    transform.position = tarPosition + Vector3.forward*0.03f;
    transform.LookAt(tarPosition);
  }

  void ChangeLayer(GameObject _obj, int _layer)
  {
    if (_obj == null)
      return;

    for(int i=0; i<_obj.transform.childCount; i++)
    {
      _obj.transform.GetChild(i).gameObject.layer = _layer;
    }

    _obj.layer = _layer;
  }

  Vector3 FindAveragePosition(Transform _head)
  {
    Vector3 avg = _head.position;
    int count = _head.childCount + 1;

    // Ignore our first point if it was zero
    if(avg == Vector3.zero)
    {
      count -= 1;
    }

    for(int i=0; i<_head.childCount; i++)
    {
      avg += _head.GetChild(i).position;
    }

    avg.x = avg.x / count;
    avg.y = avg.y / count;
    avg.z = avg.z / count;


    return avg;
  }
}
