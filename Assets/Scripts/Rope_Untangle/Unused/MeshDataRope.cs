using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDataRope
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;
    
    int triangleIndex;
    bool useFlatShading;

    public MeshDataRope(int numVertsPerCircle, int numVertsAlongRope, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;
        
        vertices = new Vector3[numVertsPerCircle * numVertsAlongRope];
        uvs = new Vector2[vertices.Length];
        
        int numMainTriangles = (numVertsPerCircle * (numVertsAlongRope - 1)) * 2;
        triangles = new int[numMainTriangles * 3];
        triangleIndex = 0;
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        vertices[vertexIndex] = vertexPosition;
        uvs[vertexIndex] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; 1 < vertexNormals.Length; i++)
            vertexNormals[i].Normalize();
        
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {

        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void ProcessMesh()
    {
        if (useFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3 [triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }
        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if (useFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = bakedNormals;
        }
        return mesh; 
    }

    internal void ResetMesh(int numVertsPerCircle, int numVertsAlongRope, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;
        
        vertices = new Vector3[numVertsPerCircle * numVertsAlongRope];
        uvs = new Vector2[vertices.Length];
        
        int numMainTriangles = (numVertsPerCircle * (numVertsAlongRope - 1)) * 2;
        triangles = new int[numMainTriangles * 3];
        triangleIndex = 0;
    }
}
