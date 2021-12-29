using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSquare
{
    public Transform plane;
    public MeshFilter squareMeshFilter;

    private int size;
    private int resolution;
    private int vertexNum_In_A_Row;
    
    public Vector3[] vertices;

    public WaterSquare(GameObject waterSquareObj, int size, int resolution)
    {
        this.plane = waterSquareObj.transform;
        this.squareMeshFilter = waterSquareObj.GetComponent<MeshFilter>();

        this.size = size;
        this.resolution = resolution;

        //해당 정사각형의 변당 버텍스 수
        vertexNum_In_A_Row = (int)(size / resolution) + 1;

        GenerateMesh();

        this.vertices = squareMeshFilter.mesh.vertices;
    }
    public void GenerateMesh()
    {
        //List<Vector3[]> vertices = new List<Vector3[]>(vertexNum_In_A_Row);
        List<int> triangles = new List<int>();
        List<Vector2> uvPos = new List<Vector2>();
        //xz평면에서 한줄씩 순회하며 버텍스의 좌표를 계산하고 삼각형에 추가한다

        Vector3[] vertices = new Vector3[vertexNum_In_A_Row * vertexNum_In_A_Row];
        for (int index = 0; index < vertexNum_In_A_Row * vertexNum_In_A_Row; index++)
        {
            int x = index / vertexNum_In_A_Row;
            int z = index % vertexNum_In_A_Row;

            Vector3 currentPoint = new Vector3();
            //UV좌표
            Vector2 currentUV = new Vector2();
            //plane의 Position을 중심으로 해서 버텍스의 좌표 계산.
            currentPoint.x = x * resolution - size / 2;
            currentPoint.y = plane.position.y;
            currentPoint.z = z * resolution - size / 2;

            vertices[x * vertexNum_In_A_Row + z] = currentPoint;

            //uv좌표 매핑
            currentUV.x = z / (float)(vertexNum_In_A_Row - 1);
            currentUV.y = x / (float)(vertexNum_In_A_Row - 1);
            uvPos.Add(currentUV);

            //(0,0)일때는 삼각형이 생성될 수 없음. 최소한 (1,1)은 되어야됨. 
            if (x <= 0 || z <= 0)
                continue;

            //위쪽 삼각형
            triangles.Add((z - 1) + (x - 1) * vertexNum_In_A_Row);
            triangles.Add(z + (x - 1) * vertexNum_In_A_Row);
            triangles.Add(z + x * vertexNum_In_A_Row);

            //아래쪽 삼각형
            triangles.Add((z - 1) + (x - 1) * vertexNum_In_A_Row);
            triangles.Add(z + x * vertexNum_In_A_Row);
            triangles.Add((z - 1) + x * vertexNum_In_A_Row);
        }

        //vertex 좌표와 triangle 실제로 Mesh에 가져다 붙이기
        Mesh seaMesh = new Mesh();
        seaMesh.vertices = vertices;
        seaMesh.triangles = triangles.ToArray();
        seaMesh.uv = uvPos.ToArray();
        seaMesh.RecalculateBounds();
        seaMesh.RecalculateNormals();

        squareMeshFilter.mesh.Clear();
        squareMeshFilter.mesh = seaMesh;
        squareMeshFilter.mesh.name = "Water Mesh";
    }
}
