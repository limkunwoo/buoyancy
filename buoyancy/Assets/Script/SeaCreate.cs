using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class SeaCreate : MonoBehaviour
{
    public GameObject boatObj;
    public GameObject waterPrefab;

    public static int squareWidth = 800;
    //해상도. 가운데 판만 높음.
    public static int innerSquareResolution = 5;
    public static int outerSquareResolution = 5;

    Vector3 boatPosition;
    Vector3 oceanPosition;

    List<WaterSquare> waterSquares = new List<WaterSquare>();
    void Start()
    {
        CreateSea();
        oceanPosition = transform.position;
    }

    void Update()
    {
        boatPosition = boatObj.transform.position;
        transform.position = oceanPosition;
    }

    public void CreateSea()
    {
        //가운데 판먼저 생성
        AddSeaPlane(0f, 0f, 0f, squareWidth, innerSquareResolution);
        //바깥족 판들 생성
        for(int i = 0; i < 9; i++)
        {
            int x = i / 3 - 1;
            int z = i % 3  -1;

            if (x == 0 && z == 0)
                continue;

            AddSeaPlane(x * squareWidth, 0f, z * squareWidth, squareWidth, outerSquareResolution);
        }
    }
    void AddSeaPlane(float xMove, float yPosition, float zMove, int squareWidth, int resolution)
    {
        GameObject waterPlane = Instantiate(waterPrefab, transform.position, transform.rotation) as GameObject;
        waterPlane.SetActive(true);

        //가운데 판을 기준으로 정렬
        Vector3 centerPosition = transform.position;
        centerPosition.x += xMove;
        centerPosition.y += yPosition;
        centerPosition.z += zMove;

        waterPlane.transform.position = centerPosition;
        waterPlane.transform.parent = this.transform;

        //square 리스트에 추가
        WaterSquare newWaterSquare = new WaterSquare(waterPlane, squareWidth, resolution);
        waterSquares.Add(newWaterSquare);
    }
}
