using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CalculateForces
{
    private const float VISCOUS_OF_WATER = 0.018f;
    //항력 공식 = 0.5* 밀도 * 유체의 속도^2 * 면적 * 항력계수
    // F = 0.5 * p * V^2 * S * C
    private const float VISCOUS_OF_Air = 0.00018f;

    //각도 -> 라디안
    public static float GetRadian(float angle)
    {
        return Mathf.PI / 180 * angle;
    }

    //항력계수 C =  0.075/(log10(Rn) -2)^2
    public static float CalculateDragCoefficient(float hullSpeed, float Legnth, string fluidName)
    {
        float viscous_fluid = 0.0f;
        if (fluidName == "Water")
            viscous_fluid = VISCOUS_OF_WATER;
        else if (fluidName == "Air")
            viscous_fluid = VISCOUS_OF_Air;
        //Debug.Log("Speed" + hullSpeed);
        //Debug.Log("WaterLength" + WaterLegnth);
        //레이놀드 수(Rn) = VL/v (선박의 속도 * 유체가 표면을 따라 지나갈 거리 / 유체의 점성)
        float Rn = hullSpeed * Legnth / viscous_fluid;
        //Debug.Log("Rn : "+ Rn);
        //항력계수 C =  0.075/(log10(Rn) -2)^2
        float coefficient = 0.075f / Mathf.Pow(Mathf.Log10(Rn) - 2f, 2f);
        return coefficient;
    }

    //마찰항력 계산 메서드
    public static Vector3 ViscousResistanceForce(float fluidDestiny, TriangleData triangle, float coefficient)
    {
        Vector3 triangleVelocity = triangle.velocity;
        Vector3 triangleNormal = triangle.normal;

        //V의 n으로의 정사영에 수직인 벡터(w2)
        //V - projection n V  =V -( V dot n / ||n||^2 * n)
        Vector3 w2 = triangleVelocity - Vector3.Dot(triangleVelocity, triangleNormal) * triangle.normal;
        //항력은 항상 운동방향과  반대로 작용함
        //w2방향의 반대방향이고 크기가 triangleVelocity인 벡터가 유체의 속도임
        Vector3 fluidSpeed = -w2.normalized * triangleVelocity.magnitude;

        //항력 공식 = 0.5* 밀도 * 유체의 속도^2 * 면적 * 항력계수
        Vector3 viscousResistanceForce = 0.5f * fluidDestiny * fluidSpeed.magnitude * fluidSpeed * triangle.area * coefficient;
        return CheckForceIsValid(viscousResistanceForce, "Water");
    }
    public static Vector3 GetAirResistanceForce(float fluidDestiny, TriangleData triangle, float coefficient)
    {
        Vector3 triangleVelocity = triangle.velocity;
        Vector3 triangleNormal = triangle.normal;

        //V의 n으로의 정사영에 수직인 벡터(w2)
        //V - projection n V  =V -( V dot n / ||n||^2 * n)
        Vector3 w2 = triangleVelocity - Vector3.Dot(triangleVelocity, triangleNormal) * triangle.normal;
        //항력은 항상 운동방향과  반대로 작용함
        //w2방향의 반대방향이고 크기가 triangleVelocity인 벡터가 유체의 속도임
        Vector3 fluidSpeed = -w2.normalized * triangleVelocity.magnitude;

        //항력 공식 = 0.5* 밀도 * 유체의 속도^2 * 면적 * 항력계수
        Vector3 viscousResistanceForce = 0.5f * fluidDestiny * fluidSpeed.magnitude * fluidSpeed * triangle.area * coefficient;
        return CheckForceIsValid(viscousResistanceForce, "Air");
    }

//모양 항력
public static Vector3 PressureDragForce(TriangleData triangleData)
{
    float velocity = triangleData.velocity.magnitude;
    //Debug.Log("velocity:" + velocity + " magnitude : " + triangleData.velocity.magnitude);
    float velocityReference = velocity;

    if (velocityReference == 0f)
        velocityReference = 1f;

    //Debug.Log(velocityReference);
    velocity = velocity/velocityReference;

    Vector3 pressureDragForce = Vector3.zero;

    if (triangleData.cosTheta > 0f)
    {
        float C_PD1 = DebugPhysics.current.C_PD1;
        float C_PD2 = DebugPhysics.current.C_PD2;
        float f_P = DebugPhysics.current.f_P;

        pressureDragForce = -(C_PD1 * velocity + C_PD2 * (velocity * velocity)) * triangleData.area * Mathf.Pow(triangleData.cosTheta, f_P) * triangleData.normal;
        //Debug.Log(pressureDragForce);
    }
    else
    {
        float C_SD1 = DebugPhysics.current.C_SD1;
        float C_SD2 = DebugPhysics.current.C_SD2;
        float f_S = DebugPhysics.current.f_S;
        //Debug.Log("coeff" + velocity);
        //Debug.Log(Mathf.Pow(-triangleData.cosTheta, f_S));
        pressureDragForce = (C_SD1 * velocity + C_SD2 * (velocity * velocity)) * triangleData.area * Mathf.Pow(-triangleData.cosTheta, f_S) * triangleData.normal;
        //Debug.Log(pressureDragForce);
    }
    return CheckForceIsValid(pressureDragForce ,"pressure Force");
}
    
    
    
    
    
    //부력 계산 메서드
    public static Vector3 GetBuoyancyForce(float destiny, TriangleData triangle)
    {
        //물속에서 면에 가해지는 압력 = 유체의 밀도 * 중력상수 * 면의 넓이 * 수면까지의 거리(높이)
        Vector3 buoyancyForce = destiny * Physics.gravity.y * triangle.area * triangle.distanceToSurface * triangle.normal;

        //양 옆 방향의 힘은 서로 상쇄됨.
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        return CheckForceIsValid(buoyancyForce, "buoyancy");
    }


    //값의 이상 판정
    private static Vector3 CheckForceIsValid(Vector3 force, string forceName)
    {
        if (!float.IsNaN(force.x + force.y + force.z))
        {
            return force;
        }
        else
        {
            Debug.Log(forceName += " force is NaN");

            return Vector3.zero;
        }
    }
    //삼각형의 진행속도 = 선박속도 + 외적(각속도,무게중심->삼각형 벡터)
    public static Vector3 GetTriangleVelocity(Vector3 triangleCenter)
    {
        Vector3 massOfCenterToTriangle = triangleCenter - DebugPhysics.current.centerOfMass;
        return DebugPhysics.current.boatVelocity + Vector3.Cross(DebugPhysics.current.boatAngularVelocity, massOfCenterToTriangle);
    }

    public static Vector3 SlammingForce(SlammingForceData slammingData, TriangleData triangleData, float boatArea, float boatMass)
    {
        //물 위로 올라가고 있을때는 작용하지 않음.
        if(triangleData.cosTheta < 0f || slammingData.originalArea <= 0f)
        {
            return Vector3.zero;
        }

        //삼각형의 수몰면젹의 변위량
        Vector3 dV = slammingData.submergedArea * slammingData.triangleVelocity;
        Vector3 dV_previous = slammingData.previousSubmergedArea * slammingData.triangleVelocity;
        //가속도
        Vector3 accVec = (dV - dV_previous)/(slammingData.originalArea * Time.fixedDeltaTime);
        float acc = accVec.magnitude; //속력
        //Debug.Log("acc : " + acc);

        //가속도가 최대에 가까울때만 slammingForce를 적용하기 위한 제곱수
        float p = DebugPhysics.current.p; 
        float acc_max = DebugPhysics.current.acc_max;

        //acc/
        Vector3 slammingForce = Mathf.Pow(Mathf.Clamp01(acc / acc_max), p) * triangleData.cosTheta * 2f * slammingData.submergedArea / boatArea * boatMass * triangleData.velocity;

        if (slammingData.isdividedTwo)
            slammingForce = slammingForce / 2;

        slammingForce *= -1f;
        return CheckForceIsValid(slammingForce, "Slamming");
    }

    public static float GetTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Cross(p2 - p1, p3 - p1).magnitude / 2;
    }

    public static Vector3 getTriangleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1 + p2 + p3) / 3;
    }
}