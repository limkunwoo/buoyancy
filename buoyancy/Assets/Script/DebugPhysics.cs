using UnityEngine;
using System.Collections;

public class DebugPhysics : MonoBehaviour
{
    public static DebugPhysics current;
    public GameObject boatObj;
    public Rigidbody boatRB;

    
    public Vector3 centerOfMass;
    public Vector3 boatVelocity;
    public Vector3 boatAngularVelocity;

    //모양항력 계수
    [Header("Force 2 - Pressure Drag Force")]
    public float velocityReference;

    [Header("Pressure Drag")]
    public float C_PD1 = 10f;
    public float C_PD2 = 10f;
    public float f_P = 0.5f;

    [Header("Suction Drag")]
    public float C_SD1 = 10f;
    public float C_SD2 = 10f;
    public float f_S = 0.5f;

    //Slamming Force
    [Header("Force 3 - Slamming Force")]
    //Slamming용
    public float p = 2f;
    public float acc_max = 100f;
    

    private void Awake()
    {
        boatRB = boatObj.GetComponent<Rigidbody>();
    }
    void Start()
    {
        
        current = this;
    }
    void Update()
    {
        centerOfMass = boatObj.transform.TransformPoint(boatRB.centerOfMass);
        boatVelocity = boatRB.velocity;
        boatAngularVelocity = boatRB.angularVelocity;
    }
}