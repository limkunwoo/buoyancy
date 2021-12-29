using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ModifyBoatMesh
{   
    private Transform boatTrans;
    private Rigidbody boatRB;
     //보트 매쉬정보
    Vector3[] boatVertices;
    int[] boatTriangles;
     //보트 버틱스 글로벌 좌표
    public Vector3[] boatVerticesGlobal;
    //버텍스-수면 높이
    float[] verticeToWaterDistance;

    //물 위 아래 매쉬와 triangle데이터(글로벌 좌표로 관리)
    public List<TriangleData> underWaterTriangleData = new List<TriangleData>();
    public List<TriangleData> aboveWaterTriangleData = new List<TriangleData>();

    private Mesh underWaterMesh;
    private Mesh aboveWaterMesh;

    //slamming force위한 리스트
    public List<SlammingForceData> slammingForceData = new List<SlammingForceData>();
    public List<int> indexOfOriginalTriangle = new List<int>();

    public float boatArea;
    
    public ModifyBoatMesh(GameObject boatObj, GameObject underWaterObj, GameObject aboveWaterObj)
    {
        boatTrans = boatObj.transform;
        boatRB = boatObj.GetComponent<Rigidbody>();

        boatVertices = boatObj.GetComponent<MeshFilter>().mesh.vertices;
        boatTriangles = boatObj.GetComponent<MeshFilter>().mesh.triangles;

        boatVerticesGlobal = new Vector3[boatVertices.Length];
        verticeToWaterDistance = new float[boatVertices.Length]; ;

        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = aboveWaterObj.GetComponent<MeshFilter>().mesh;

        //slammingForceData 초기화
        for(int i = 0; i < boatTriangles.Length/3; i++)
        {
            slammingForceData.Add(new SlammingForceData());
        }
        //slamingForceData.originalArea 초기화
        CalculateOriginalTriangleArea();
    }
    //slamingForceData.originalArea 초기화 코드
    private void CalculateOriginalTriangleArea()
    {
        int triangleNum = 0;
        for(int i = 0; i< boatTriangles.Length; i = i+3)
        {
            Vector3 p1 = boatVertices[boatTriangles[i]];
            Vector3 p2 = boatVertices[boatTriangles[i + 1]];
            Vector3 p3 = boatVertices[boatTriangles[i + 2]];

            float triangleArea = CalculateForces.GetTriangleArea(p1, p2, p3);

            slammingForceData[triangleNum].originalArea = triangleArea;
            boatArea += triangleArea;

            triangleNum += 1;
        }
    }

    private class VertexData
    {
        public float distanceToWater;
        public int index;
        public Vector3 globalVertexPos;
    }

    public void GenerateUnderWaterMesh()
    {
        //이전프레임 삼각형 초기화
        underWaterTriangleData.Clear();
        aboveWaterTriangleData.Clear();
        indexOfOriginalTriangle.Clear();

        //submerged Area 최신화
        for (int j = 0; j < slammingForceData.Count; j++)
        {
            slammingForceData[j].previousSubmergedArea = slammingForceData[j].submergedArea;
        }

        //보트 버텍스을 글로벌 좌표로
        for (int i = 0; i < boatVertices.Length; i++)
        {
            boatVerticesGlobal[i] = boatTrans.TransformPoint(boatVertices[i]);
            verticeToWaterDistance[i] = WaterController.current.DistanceToWater(boatVerticesGlobal[i]);
        }

        AddTriangles();
    }

    public void AddTriangles()
    {
        int from = 0;
        int to = boatTriangles.Length / 3;
        int taskCount = 4;

        //멀티 태스크
        Func<object, List<ICollection>> ForTask = (objectRange) =>
            {
                int[] range = (int[])objectRange;

                List<VertexData> vertexDatasInTriangle = new List<VertexData>();

                //삼각형 하나를 이루는 버텍스 3개 준비
                vertexDatasInTriangle.Add(new VertexData());
                vertexDatasInTriangle.Add(new VertexData());
                vertexDatasInTriangle.Add(new VertexData());

                List<TriangleData> underWaterTriangle = new List<TriangleData>();
                List<TriangleData> aboveWaterTriangle = new List<TriangleData>();
                List<int> indexofOriginalTri = new List<int>();

                List<ICollection> totalTriangles = new List<ICollection>();

                for (int i = range[0]; i < range[1]; i++)
                {
                    //vertexData 초기화
                    //Debug.Log("Length = " + boatTriangles.Length);

                    for (int j = 0; j < 3; j++)
                    {
                        vertexDatasInTriangle[j].distanceToWater = verticeToWaterDistance[boatTriangles[i * 3 + j]];
                        vertexDatasInTriangle[j].index = j;
                        vertexDatasInTriangle[j].globalVertexPos = boatVerticesGlobal[boatTriangles[i * 3 + j]];
                    }

                    //Debug.Log("What is I : " + i);

                    //삼각형 세개의 점이 모두 물 위에 있는 경우
                    if (vertexDatasInTriangle[0].distanceToWater > 0f && vertexDatasInTriangle[1].distanceToWater > 0f && vertexDatasInTriangle[2].distanceToWater > 0f)
                    {
                        Vector3 p1 = vertexDatasInTriangle[0].globalVertexPos;
                        Vector3 p2 = vertexDatasInTriangle[1].globalVertexPos;
                        Vector3 p3 = vertexDatasInTriangle[2].globalVertexPos;

                        //aboveWaterTriangle.Add(new TriangleData(p1, p2, p3, boatRB));
                        //i = 삼각형 인덱스
                        slammingForceData[i].submergedArea = 0f;
                        slammingForceData[i].triangleCenter = CalculateForces.getTriangleCenter(p1, p2, p3);
                    }
                    //모두 물 아래에 있는 경우
                    else if (vertexDatasInTriangle[0].distanceToWater < 0f && vertexDatasInTriangle[1].distanceToWater < 0f && vertexDatasInTriangle[2].distanceToWater < 0f)
                    {
                        Vector3 p1 = vertexDatasInTriangle[0].globalVertexPos;
                        Vector3 p2 = vertexDatasInTriangle[1].globalVertexPos;
                        Vector3 p3 = vertexDatasInTriangle[2].globalVertexPos;

                        underWaterTriangle.Add(new TriangleData(p1, p2, p3));

                        slammingForceData[i].submergedArea = slammingForceData[i].originalArea;
                        slammingForceData[i].triangleCenter = CalculateForces.getTriangleCenter(p1, p2, p3);
                        indexofOriginalTri.Add(i);
                        
                    }
                    else
                    {
                        //버텍스를 높이순으로 정렬
                        vertexDatasInTriangle.Sort((x, y) => x.distanceToWater.CompareTo(y.distanceToWater));
                        vertexDatasInTriangle.Reverse();

                        //점 하나만 물 위에 있는 경우
                        if (vertexDatasInTriangle[0].distanceToWater > 0f && vertexDatasInTriangle[1].distanceToWater < 0f && vertexDatasInTriangle[2].distanceToWater < 0f)
                        {
                            AddTrianglesOneAvoveWater(vertexDatasInTriangle, underWaterTriangle, aboveWaterTriangle, indexofOriginalTri, i);
                        }
                        //점 두개가 물 위에 있는 경우
                        else if (vertexDatasInTriangle[0].distanceToWater > 0f && vertexDatasInTriangle[1].distanceToWater > 0f && vertexDatasInTriangle[2].distanceToWater < 0f)
                        {
                            AddTrianglesTwoAboveWater(vertexDatasInTriangle, underWaterTriangle, aboveWaterTriangle, indexofOriginalTri, i);
                        }
                    }
                }
                totalTriangles.Add(underWaterTriangle);
                //totalTriangles.Add(aboveWaterTriangle);
                totalTriangles.Add(indexofOriginalTri);
                //Debug.Log("underwater:" + underWaterTriangle.Count + "\nindexOf:" + indexofOriginalTri.Count);

                return totalTriangles;
            };

        //병렬처리 시작
        Task<List<ICollection>>[] tasks = new Task<List<ICollection>>[taskCount];

        int currentFrom = from;
        int currentTo = to / tasks.Length;

        for(int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Task<List<ICollection>>(ForTask, new int[] { currentFrom, currentTo });
            currentFrom = currentTo;

            if (i == tasks.Length - 2)
                currentTo = to;
            else 
                currentTo = currentTo + to/tasks.Length;
        }

        foreach(Task<List<ICollection>> task in tasks)
            task.Start();

        foreach (Task<List<ICollection>> task in tasks)
        {
            task.Wait();
            List<TriangleData> underWaterTri = task.Result[0] as List<TriangleData>;
            if (underWaterTri == null)
                Debug.Log("null");
            else
            {
                //Debug.Log("Not null");
                underWaterTriangleData.AddRange(underWaterTri.ToArray());
            }
            List<int> indexOfOriginTri = task.Result[1] as List<int>;
            if (indexOfOriginTri != null)
            {
                //Debug.Log("inputed index of original triangle");
                indexOfOriginalTriangle.AddRange(indexOfOriginTri.ToArray());
            }
                
            //aboveWaterTriangleData.AddRange(task.Result[1].ToArray());
        }

        //Debug.Log("underwaterTriangles:" + underWaterTriangleData.Count + "\nindex : " + indexOfOriginalTriangle.Count);
    }
    //한 점만 물 위에 있을때의 처리
    private void AddTrianglesOneAvoveWater(List<VertexData> vertexData, List<TriangleData> underWaterTriangle, List<TriangleData>aboveWaterTriangle,List<int> indexOfOriginalTri ,int triangleCounter)
    {
        //물 밖의 점
        Vector3 high = vertexData[0].globalVertexPos;
        //물 밑의 두 점
        Vector3 left = Vector3.zero;
        Vector3 right = Vector3.zero;

        //각각 점의 물까지의 높이
        float h_High = vertexData[0].distanceToWater;
        //일단 0
        float h_Left = 0f;
        float h_Right = 0f;

        //왼쪽 오른쪽 점 찾기
        int left_Index = vertexData[0].index - 1;
        if (left_Index < 0)
            left_Index = 2;

        if (vertexData[1].index == left_Index)
        {
            left = vertexData[1].globalVertexPos;
            right = vertexData[2].globalVertexPos;

            h_Left = vertexData[1].distanceToWater;
            h_Right = vertexData[2].distanceToWater;
        }
        else
        {
            left = vertexData[2].globalVertexPos;
            right = vertexData[1].globalVertexPos;

            h_Left = vertexData[2].distanceToWater;
            h_Right = vertexData[1].distanceToWater;
        }

        //직각삼각형 변의 비율?? 비례식 이용해서 물과의 접점을 구한다.
        //왼쪽점 -> 물 위의 점 벡터
        Vector3 leftToHigh = high - left;

        //비례식이용 - 왼쪽 점 -> 물과의 접점 벡터를 구한뒤 왼쪽 점 죄표를 더해 물과의 접점 좌표를 알아낸다.
        float length = -h_Left / (h_High - h_Left);
        Vector3 leftToWaterSurfaceVector = length * leftToHigh;
        Vector3 leftWaterSurface = left + leftToWaterSurfaceVector;

        //오른쪽 점에 대해서도 수행
        Vector3 rightToHigh = high - right;
        Vector3 rightToWaterSurfaceVector = -h_Right / (h_High - h_Right) * rightToHigh;
        Vector3 rightWaterSurface = right + rightToWaterSurfaceVector;

        //물 아래 삼각형 추가 (순서주의)
        underWaterTriangle.Add(new TriangleData(left, leftWaterSurface, rightWaterSurface));
        underWaterTriangle.Add(new TriangleData(left, rightWaterSurface, right));

        //물 위 삼각형 추가
        //aboveWaterTriangle.Add(new TriangleData(leftWaterSurface, high, rightWaterSurface, boatRB));

        //잠긴 삼각형의 넓이
        float submergedArea = CalculateForces.GetTriangleArea(left, leftWaterSurface, rightWaterSurface) + CalculateForces.GetTriangleArea(left, rightWaterSurface, right);
        slammingForceData[triangleCounter].submergedArea = submergedArea;
        slammingForceData[triangleCounter].triangleCenter = (left + rightWaterSurface) / 2;
        slammingForceData[triangleCounter].isdividedTwo = true;

        //Debug.Log("subMergedArea: " + slammingForceData[triangleCounter].submergedArea);

        indexOfOriginalTri.Add(triangleCounter);
        indexOfOriginalTri.Add(triangleCounter);
    }
    //물위 점이 2개인 경우.
    private void AddTrianglesTwoAboveWater(List<VertexData> vertexData, List<TriangleData> underWaterTriangle, List<TriangleData> aboveWaterTriangle, List<int> indexOfOriginalTri, int triangleCounter)
    {
        Vector3 low = vertexData[2].globalVertexPos;
        Vector3 left = Vector3.zero;
        Vector3 right = Vector3.zero;

        float h_Low = vertexData[2].distanceToWater;
        float h_Left = 0f;
        float h_Right = 0f;


        int leftIndex = vertexData[2].index + 1;
        if (leftIndex > 2)
            leftIndex = 0;
        if (leftIndex == vertexData[1].index)
        {
            left = vertexData[1].globalVertexPos;
            h_Left = vertexData[1].distanceToWater;

            right = vertexData[0].globalVertexPos;
            h_Right = vertexData[0].distanceToWater;
        }
        else
        {
            left = vertexData[0].globalVertexPos;
            h_Left = vertexData[0].distanceToWater;

            right = vertexData[1].globalVertexPos;
            h_Right = vertexData[1].distanceToWater;
        }
        Vector3 lowToLeftVector = left - low;
        Vector3 lowToLeftSurfaceVector = -h_Low / (-h_Low + h_Left) * lowToLeftVector;
        Vector3 leftWaterSurface = low + lowToLeftSurfaceVector;

        Vector3 lowToRightVector = right - low;
        Vector3 lowToRIghtSurfaceVector = -h_Low / (-h_Low + h_Right) * lowToRightVector;
        Vector3 rightWaterSurface = low + lowToRIghtSurfaceVector;

        underWaterTriangle.Add(new TriangleData(low, leftWaterSurface, rightWaterSurface));

        //aboveWaterTriangle.Add(new TriangleData(leftWaterSurface, left, right, boatRB));
        //aboveWaterTriangle.Add(new TriangleData(rightWaterSurface, leftWaterSurface, right, boatRB));

        slammingForceData[triangleCounter].submergedArea = CalculateForces.GetTriangleArea(low, leftWaterSurface, rightWaterSurface);
        slammingForceData[triangleCounter].triangleCenter = CalculateForces.getTriangleCenter(low, leftWaterSurface, rightWaterSurface);

        indexOfOriginalTri.Add(triangleCounter);
    }
    public void ApplyMesh(Mesh mesh, string name, List<TriangleData> trianglesData)
    {
        //Mesh를 실제로 적용
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < trianglesData.Count; i++)
        {
            //글로벌 -> 로컬
            Vector3 p1 = boatTrans.InverseTransformPoint(trianglesData[i].p1);
            Vector3 p2 = boatTrans.InverseTransformPoint(trianglesData[i].p2);
            Vector3 p3 = boatTrans.InverseTransformPoint(trianglesData[i].p3);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }

        mesh.Clear();
        mesh.name = name;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }


    //유체가 표면을 타고 흐를 길이
    public float CalculateAroundLength(string name)
    {
        float longRad = 0f;
        float shortRad = 0f;
        //타원의 둘레(근사치) = 2PI * root((장반경 제곱 + 단반경 제곱) / 2)
        if (name == "Under")
        {
            longRad = underWaterMesh.bounds.size.z;
            shortRad = underWaterMesh.bounds.size.x;
        }
        else if(name == "Above")
        {
            Debug.Log("Enter");
            longRad = aboveWaterMesh.bounds.size.z;
            shortRad = aboveWaterMesh.bounds.size.x;
        }

        float inRoot = (longRad * longRad + shortRad * shortRad) / 2;
        float length = 1 * Mathf.PI * Mathf.Sqrt(inRoot);
        //Debug.Log("z:" + underWaterMesh.bounds.size.z + "x:" + underWaterMesh.bounds.size.x + "y:" + underWaterMesh.bounds.size.y + "length:" + length);
        return length;
    }
}
