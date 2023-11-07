using UnityEngine;

namespace Rope_Untangle.Unused
{
    public class MeshDataRope
    {
        Vector3[] _vertices;
        int[] _triangles;
        Vector2[] _uvs;
        Vector3[] _bakedNormals;
    
        int _triangleIndex;
        bool _useFlatShading;

        public MeshDataRope(int numVertsPerCircle, int numVertsAlongRope, bool useFlatShading)
        {
            this._useFlatShading = useFlatShading;
        
            _vertices = new Vector3[numVertsPerCircle * numVertsAlongRope];
            _uvs = new Vector2[_vertices.Length];
        
            int numMainTriangles = (numVertsPerCircle * (numVertsAlongRope - 1)) * 2;
            _triangles = new int[numMainTriangles * 3];
            _triangleIndex = 0;
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            _vertices[vertexIndex] = vertexPosition;
            _uvs[vertexIndex] = uv;
        }

        public void AddTriangle(int a, int b, int c)
        {
            _triangles[_triangleIndex] = a;
            _triangles[_triangleIndex + 1] = b;
            _triangles[_triangleIndex + 2] = c;
            _triangleIndex += 3;
        }

        Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[_vertices.Length];
            int triangleCount = _triangles.Length / 3;

            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = _triangles[normalTriangleIndex];
                int vertexIndexB = _triangles[normalTriangleIndex + 1];
                int vertexIndexC = _triangles[normalTriangleIndex + 2];

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

            Vector3 pointA = _vertices[indexA];
            Vector3 pointB = _vertices[indexB];
            Vector3 pointC = _vertices[indexC];
            Vector3 sideAb = pointB - pointA;
            Vector3 sideAc = pointC - pointA;
            return Vector3.Cross(sideAb, sideAc).normalized;
        }

        public void ProcessMesh()
        {
            if (_useFlatShading)
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
            _bakedNormals = CalculateNormals();
        }

        void FlatShading()
        {
            Vector3[] flatShadedVertices = new Vector3 [_triangles.Length];
            Vector2[] flatShadedUvs = new Vector2[_triangles.Length];
            for (int i = 0; i < _triangles.Length; i++)
            {
                flatShadedVertices[i] = _vertices[_triangles[i]];
                flatShadedUvs[i] = _uvs[_triangles[i]];
                _triangles[i] = i;
            }
            _vertices = flatShadedVertices;
            _uvs = flatShadedUvs;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = _vertices;
        
            mesh.triangles = _triangles;
            mesh.uv = _uvs;

            if (_useFlatShading)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = _bakedNormals;
            }
            return mesh; 
        }

        internal void ResetMesh(int numVertsPerCircle, int numVertsAlongRope, bool useFlatShading)
        {
            this._useFlatShading = useFlatShading;
        
            _vertices = new Vector3[numVertsPerCircle * numVertsAlongRope];
            _uvs = new Vector2[_vertices.Length];
        
            int numMainTriangles = (numVertsPerCircle * (numVertsAlongRope - 1)) * 2;
            _triangles = new int[numMainTriangles * 3];
            _triangleIndex = 0;
        }
    }
}
