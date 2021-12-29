using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//SlammingForce계산을 위한 구조체
//j번째 삼각형을 Tj 라고 할때
//SlammingForce = Clamp(Acc(j)/Acc_max, 0 , 1)^p * cos(theta(j)) * 2 * 잠긴 면적 / 총 보트의 넓이 * 보트의 질량 * Tj의 속도
//Acc(j) =  Tj의 잠긴 면적(t) - Tj의 잠긴 면적 (t - dt)/Tj의 면적 * dt
public class SlammingForceData
{
    //원래의 삼각형 면적
    public float originalArea;
    //이전 프레임과 현재프레임의 침수된 삼각형의 면적
    public float submergedArea;
    public float previousSubmergedArea;
    
    //삼각형의 속도
    public Vector3 triangleCenter;
    public Vector3 triangleVelocity;
    public Vector3 previousVelocity;

    public bool isdividedTwo = false;
}
