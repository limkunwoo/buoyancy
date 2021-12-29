using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class BoatPhysics : MonoBehaviour
{
    
    //모트를 물 위와 아래로 나누기 위함.
    public GameObject underWaterObj;
    public GameObject aboveWaterObj;

    private Mesh underWaterMesh;
    private Mesh aboveWaterMesh;
    //보트 매쉬 수정 클래스
    private ModifyBoatMesh modifyBoatMesh;
    
    private Rigidbody boatRB;
    // 밀도 (kg/M^3)
    private float waterDestiny = 1000f;
    private float airDestiny = 1.225f;

    private float boatMass;
    private float boatArea;
    void Awake()
    {
        boatRB = gameObject.GetComponent<Rigidbody>();
    }
    void Start()
    {
        modifyBoatMesh = new ModifyBoatMesh(gameObject, underWaterObj, aboveWaterObj);
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = aboveWaterObj.GetComponent<MeshFilter>().mesh;

        boatMass = boatRB.mass;
        GameObject[] ballanceBalls = GameObject.FindGameObjectsWithTag("ballance");
        for (int i = 0; i < ballanceBalls.Length; i++)
        {
            boatMass += ballanceBalls[i].GetComponent<Rigidbody>().mass;
        }
        boatArea = modifyBoatMesh.boatArea;
    }
    
    void Update()
    {
        modifyBoatMesh.GenerateUnderWaterMesh();
        modifyBoatMesh.ApplyMesh(underWaterMesh, "UnderWater Mesh", modifyBoatMesh.underWaterTriangleData);
        //modifyBoatMesh.DisplayMesh(aboveWaterMesh, "Above Water Mesh", modifyBoatMesh.aboveWaterTriangleData);
    }

    private void FixedUpdate()
    {
        if(modifyBoatMesh.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForce();
        }
        if(modifyBoatMesh.aboveWaterTriangleData.Count > 0)
        {
            //AddAirResistance();
        }
    }

    void AddUnderWaterForce()
    {
        //물의 저항을 위한 항력계수
        float WaterdragCoefficient = CalculateForces.CalculateDragCoefficient(boatRB.velocity.magnitude, modifyBoatMesh.CalculateAroundLength("Under"), "Water");
        List<TriangleData> underWaterTriangles = modifyBoatMesh.underWaterTriangleData;

        List<SlammingForceData> slammingForceData = modifyBoatMesh.slammingForceData;
        CalculateTriangleVelocities(slammingForceData);

        List<int> indexofOriginalTriangle = modifyBoatMesh.indexOfOriginalTriangle;

        //압력 계산해서 각 삼각형에 가해준다.
        
        for(int i =0; i < underWaterTriangles.Count; i++)
        {
            TriangleData triangle = underWaterTriangles[i];

            Vector3 forceUnderwater = Vector3.zero;

            //부력 + 표면마찰저항 + 모양항력 + slamming
            forceUnderwater += CalculateForces.GetBuoyancyForce(waterDestiny, triangle);
            //Debug.Log("buoyancy" + forceUnderwater);
                         //Debug.DrawRay(triangle.center,  forceUnderwater.normalized, Color.blue);
            forceUnderwater += CalculateForces.ViscousResistanceForce(waterDestiny, triangle, WaterdragCoefficient);
            //Debug.Log("Viscous" + forceUnderwater);
                        //Debug.DrawRay(triangle.center, CalculateForces.ViscousResistanceForce(waterDestiny, triangle, WaterdragCoefficient), Color.blue);
            forceUnderwater += CalculateForces.PressureDragForce(triangle);
            //Debug.Log("Pressure" + forceUnderwater);
            //Debug.DrawRay(triangle.center, CalculateForces.PressureDragForce(triangle),Color.black);
            int originalTriangleData = indexofOriginalTriangle[i];
            SlammingForceData slammingData = slammingForceData[originalTriangleData];
            
            forceUnderwater += CalculateForces.SlammingForce(slammingData, triangle, boatArea, boatMass);
            Debug.DrawRay(triangle.center, CalculateForces.SlammingForce(slammingData, triangle, boatArea, boatMass), Color.blue);
            //Debug.Log("Slamming" + forceUnderwater);
            boatRB.AddForceAtPosition(forceUnderwater, triangle.center);
                        //Debug.DrawRay(triangle.center, triangle.normal * 3f, Color.white);
                        //Debug.DrawRay(triangle.center, forceUnderwater.normalized, Color.blue);

        }
    }

    void AddAirResistance()
    {
        //공기저항 항력계수
        float AirdragCoefficient = CalculateForces.CalculateDragCoefficient(boatRB.velocity.magnitude, modifyBoatMesh.CalculateAroundLength("Above"), "Air");
                     //Debug.Log(AirdragCoefficient);
        List<TriangleData> aboveWaterTriangleData = modifyBoatMesh.aboveWaterTriangleData;

        for(int i = 0; i <aboveWaterTriangleData.Count; i++)
        {
            TriangleData triangle = aboveWaterTriangleData[i];

            Vector3 airResistance = CalculateForces.GetAirResistanceForce(airDestiny, triangle, AirdragCoefficient);

            boatRB.AddForceAtPosition(airResistance, triangle.center);
                       Debug.DrawRay(triangle.center, airResistance, Color.white);
                      //Debug.Log(airResistance);
        }
    }

    //속도 계산
    private void CalculateTriangleVelocities(List<SlammingForceData> slammingForceData)
    {
        for(int i = 0; i < slammingForceData.Count; i++)
        {
            slammingForceData[i].previousVelocity = slammingForceData[i].triangleVelocity;
            slammingForceData[i].triangleVelocity = CalculateForces.GetTriangleVelocity(slammingForceData[i].triangleCenter);
        }
    }
}

