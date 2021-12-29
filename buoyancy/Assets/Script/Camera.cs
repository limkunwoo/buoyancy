using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform boatTrans;
    //반지름
    private float r;

    //극좌표를 위한 각도
    //0~Pi
    private float theta;
    //0~2Pi
    private float omega;

    //카메라 회전 속도
    private float velocity = 0.5f;
    void Start()
    {
        r = 70f;

        theta = CalculateForces.GetRadian(40);
        omega = CalculateForces.GetRadian(270);
        transform.LookAt(boatTrans);
    }

    
    void LateUpdate()
    {
        //마우스 휠로 반지름 조정
        r -= Input.GetAxis("Mouse ScrollWheel") * 10;
        r = Mathf.Clamp(r, 10f, 300);

        //오른쪽 마우스 드래그로 각도 조정
        if (Input.GetMouseButton(1))
        {
            omega += Input.GetAxis("Mouse X") * velocity * Time.smoothDeltaTime * Mathf.PI;
            
            theta += Input.GetAxis("Mouse Y") * velocity * Time.smoothDeltaTime * Mathf.PI;
            theta = Mathf.Clamp(theta, CalculateForces.GetRadian(0.5f), CalculateForces.GetRadian(70));
            //Debug.Log(theta);
        }
        //극좌표->직교좌표계
        float x = r * Mathf.Sin(theta) * Mathf.Cos(omega) + boatTrans.position.x;
        float y = r * Mathf.Cos(theta);
        float z = r * Mathf.Sin(theta) * Mathf.Sin(omega) + boatTrans.position.z;

        //Vector3 dir = boatTrans.position - transform.position;
        transform.position = new Vector3(x, y, z);
        transform.LookAt(boatTrans);
    }
}
