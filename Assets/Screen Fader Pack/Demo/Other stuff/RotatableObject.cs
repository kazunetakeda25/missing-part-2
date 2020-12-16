using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class RotatableObject : MonoBehaviour
{
    public float angleX = 0;
    public float angleY = 0;
    public float angleZ = 0;

    void Update()
    {
        if (angleX > 0 | angleY > 0 | angleZ > 0)
        {
            this.transform.RotateAroundLocal(Vector3.up, angleY * Time.deltaTime);
            this.transform.RotateAroundLocal(Vector3.forward, angleX * Time.deltaTime);
            this.transform.RotateAroundLocal(Vector3.right, angleZ * Time.deltaTime);
        }
    }
}
