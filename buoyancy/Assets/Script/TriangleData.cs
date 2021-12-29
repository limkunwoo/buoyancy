using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TriangleData
{
    //글로벌 좌표계
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public Vector3 center;
    public Vector3 normal;

    public Vector3 velocity;

    public float distanceToSurface;
    public float area;

    public float cosTheta;
    public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;

        this.center = CalculateForces.getTriangleCenter(p1,p2,p3);
        
        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        this.distanceToSurface = Mathf.Abs(WaterController.current.DistanceToWater(this.center));
        //삼각형의 넓이 = 외적한 벡터의 길이 / 2
        this.area = CalculateForces.GetTriangleArea(p1, p2, p3);

        //폴리곤의 속도 Vi = 선박의 속도(V) + {각속도 벡터 Cross (선박 무게중심-> 삼각형 무게중심)}
        Vector3 massCenterToTriangleCenter = center - DebugPhysics.current.centerOfMass;
        this.velocity = CalculateForces.GetTriangleVelocity(center);

        //진행각
        this.cosTheta = Vector3.Dot(this.velocity.normalized, normal);
    }
}
