using UnityEngine;
using System.Collections;

public class BoatEngine : MonoBehaviour
{
    //스크루
    public Transform screwTransform;
    //파워 증가량
    public const float powerFactor = 0.2f;
    
    //최대파워
    public float maxPower;

    public float currentPower;

    private Rigidbody boatRB;

    //키(방향타) 기본위치
    Quaternion defaultDir;
    //왼쪽 오른쪽 최대 회전각
    Quaternion leftDir;
    Quaternion rightDir;
    void Start()
    {
        boatRB = GetComponent<Rigidbody>();
    }


    void Update()
    {
        UserInput();
    }

    void FixedUpdate()
    {
        UpdateWaterJet();
    }

    void UserInput()
    {

        //전진
        if (Input.GetKey(KeyCode.W))
        {
            if (currentPower < maxPower)
            {
                currentPower += Mathf.Lerp(0f, maxPower, Time.deltaTime / 5);
                currentPower = Mathf.Clamp(currentPower,0f, maxPower);
            }
        }
        else
        {
            currentPower -= Mathf.Lerp(0f, maxPower, Time.deltaTime / 5);
            currentPower = Mathf.Clamp(currentPower, 0f, maxPower);
        }
        //y축을 중심으로한 회전행렬
        //[cos(Theta)   0   sin(Theta)][x]      [x']
        //[    0        1        0    ][y]  =   [y']
        //[-sin(Theta)  0   cos(Theta)][z]      [z']

        //x' = x*cos(Theta) + z*sin(Theta)
        //y' = y
        //z' = -x*sin(Theta) + z*cos(Theta)

        //좌회전
        if (Input.GetKey(KeyCode.A))
        {
            //좌측으로 30도 회전한 벡터 구하기
            Vector3 forwardvector = transform.forward;

            float x = forwardvector.x * Mathf.Cos(CalculateForces.GetRadian(30)) + forwardvector.z * Mathf.Sin(CalculateForces.GetRadian(30));
            float y = forwardvector.y;
            float z = -forwardvector.x * Mathf.Sin(CalculateForces.GetRadian(30)) + forwardvector.z * Mathf.Cos(CalculateForces.GetRadian(30));

            leftDir = Quaternion.LookRotation(new Vector3(x, y, z), transform.up);

            //A키 눌린동안 방향타 좌측으로 회전
            screwTransform.rotation = Quaternion.RotateTowards(screwTransform.rotation, leftDir, 10.0f * Time.deltaTime);
            //Debug.Log(waterJetTransform.rotation);
        }
        //우회전
        else if (Input.GetKey(KeyCode.D))
        {
            //우측으로 30도 회전한 벡터 구하기
            Vector3 forwardvector = transform.forward;

            float x = forwardvector.x * Mathf.Cos(CalculateForces.GetRadian(-30)) + forwardvector.z * Mathf.Sin(CalculateForces.GetRadian(-30));
            float y = forwardvector.y;
            float z = -forwardvector.x * Mathf.Sin(CalculateForces.GetRadian(-30)) + forwardvector.z * Mathf.Cos(CalculateForces.GetRadian(-30));

            rightDir = Quaternion.LookRotation(new Vector3(x, y, z), transform.up);

            //D키 눌린동안 방향타 좌측으로 회전
            screwTransform.rotation = Quaternion.RotateTowards(screwTransform.rotation, rightDir, 10.0f * Time.deltaTime);
        }
        else
        {
            //방향타 원위치
            defaultDir = Quaternion.LookRotation(transform.forward, transform.up);
            screwTransform.rotation = Quaternion.RotateTowards(screwTransform.rotation, defaultDir, 30.0f * Time.deltaTime);
        }
        Debug.DrawRay(screwTransform.position, -screwTransform.forward * 300);
        //Debug.Log("Current Quarternion" + screwTransform.rotation + "default Quarternion" + defaultDir);
    }

    void UpdateWaterJet()
    {
        //Debug.Log(boatController.CurrentSpeed);
        //스크루 출력
        Vector3 forceToBoat = transform.forward * currentPower;
        //키에 작용하는 힘
        Vector3 forceToRudder = -(transform.forward + (-screwTransform.forward)).normalized * currentPower * 0.05f;

        Debug.DrawRay(screwTransform.GetChild(0).position, forceToRudder, Color.black);
        
        //배가 물 위에 있을경우 전진x
        float waveYPos = WaterController.current.GetWaveYPos(screwTransform.position);
        if (screwTransform.position.y < waveYPos)
        {
            boatRB.AddForceAtPosition(forceToBoat, screwTransform.position);
            boatRB.AddForceAtPosition(forceToRudder, screwTransform.GetChild(0).position);
        }
    }
}