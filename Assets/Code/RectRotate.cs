using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectRotate : MonoBehaviour
{
    public float rotateSpeed;
    private void FixedUpdate()
    {
        GetComponent<RectTransform>().Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}
