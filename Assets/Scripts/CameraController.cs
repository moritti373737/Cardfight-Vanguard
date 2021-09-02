using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 diff;
    GameObject target;

    void Start()
    {
        target = SelectManager.Instance.SelectBox;
        diff = target.transform.position - transform.position;
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.transform.position - diff, Time.deltaTime * 15.0f);
    }
}