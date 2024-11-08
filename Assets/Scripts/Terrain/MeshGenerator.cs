using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMult, AnimationCurve _heightCurve, int LOD)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int meshSimplificationIncrement = (LOD == 0) ? 1 : LOD * 2; //how much to "jump" between vertices, so we dont draw all of them necessarily

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2*meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int vertsPerLine = (meshSize - 1) / meshSimplificationIncrement + 1; //verts per line if we simplify the mesh

        MeshData meshData = new MeshData(vertsPerLine);
        //int vertIndex = 0;

        int[,] vertIndexMap = new int[borderedSize, borderedSize];
        int meshVertIndex =  0;
        int borderVertIndex = -1;

        for(int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for(int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if(isBorderVertex)
                {
                    vertIndexMap[x, y] = borderVertIndex;
                    borderVertIndex--;
                }
                else
                {
                    vertIndexMap[x, y] = meshVertIndex;
                    meshVertIndex++;
                }
            }
        }

        for(int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for(int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertIndex = vertIndexMap[x, y];

                Vector2 percent = new Vector2((x - meshSimplificationIncrement)/ (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMult;
                Vector3 vertPos = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);
                
                meshData.addVert(vertPos, percent, vertIndex);

                if(x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertIndexMap[x, y];
                    int b = vertIndexMap[x + meshSimplificationIncrement, y];
                    int c = vertIndexMap[x, y + meshSimplificationIncrement];
                    int d = vertIndexMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTri(a, d, c);
                    meshData.AddTri(d, a, b);
                }

                vertIndex++;
            }
        }

        return meshData;
    }

    /*public static MeshData GenerateWaterMesh(float[,] heightMap)//TODO
    {
        int width = heightMap.GetLength(0) - 1;
        int height = heightMap.GetLength(1) - 1;
        MeshData meshData = new MeshData(width, height);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
            }
        }

        return meshData;
    }*/
}

//Class for holding meshdata
public class MeshData
{
    Vector3[] verts;
    int[] tris;
    Vector2[] uvs;
    Vector3[] borderVerts;
    int[] borderTris;
    int triIndex;
    int borderTriIndex;

    //Constructor
    public MeshData(int vertPerLine)
    {
        verts = new Vector3[vertPerLine * vertPerLine];
        uvs = new Vector2[vertPerLine * vertPerLine];
        tris = new int[(vertPerLine-1)*(vertPerLine-1)*6];

        borderVerts = new Vector3[vertPerLine * 4 + 4];
        borderTris = new int[24 * vertPerLine];
    }

    public void addVert(Vector3 vertPos, Vector2 uv, int vertIndex)
    {
        if(vertIndex < 0)
        {
            borderVerts[-vertIndex - 1] = vertPos;
        }
        else
        {
            verts[vertIndex] = vertPos;
            uvs[vertIndex] = uv;
        }
    }

    public void AddTri(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0) //belong to border
        {
            borderTris[borderTriIndex] = a;
            borderTris[borderTriIndex + 1] = b;
            borderTris[borderTriIndex + 2] = c;
            borderTriIndex += 3;
        }
        else
        {
            tris[triIndex] = a;
            tris[triIndex + 1] = b;
            tris[triIndex + 2] = c;
            triIndex += 3;
        }
    }

    Vector3[] CalcNormals()
    {
        Vector3[] normals = new Vector3[verts.Length];
        int triCount = tris.Length / 3;
        for(int i = 0; i < triCount; i++)
        {
            int normalTriIndex = i * 3;
            int vertIndexA = tris[normalTriIndex];
            int vertIndexB = tris[normalTriIndex + 1];
            int vertIndexC = tris[normalTriIndex + 2];

            Vector3 triNormal = SurfaceNormalFromIndices(vertIndexA, vertIndexB, vertIndexC);
            normals[vertIndexA] += triNormal;
            normals[vertIndexB] += triNormal;
            normals[vertIndexC] += triNormal;
        }

        int borderTriCount = borderTris.Length / 3;
        for(int i = 0; i < borderTriCount; i++)
        {
            int normalTriIndex = i * 3;
            int vertIndexA = borderTris[normalTriIndex];
            int vertIndexB = borderTris[normalTriIndex + 1];
            int vertIndexC = borderTris[normalTriIndex + 2];

            Vector3 triNormal = SurfaceNormalFromIndices(vertIndexA, vertIndexB, vertIndexC);
            if(vertIndexA >= 0)
            {
                normals[vertIndexA] += triNormal;
            }
            if(vertIndexB >= 0)
            {
                normals[vertIndexB] += triNormal;
            }
            if(vertIndexC >= 0)
            {
                normals[vertIndexC] += triNormal;
            }    
        }

        for(int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        return normals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVerts[-indexA - 1] : verts[indexA];
        Vector3 pointB = (indexB < 0) ? borderVerts[-indexB - 1] : verts[indexB];
        Vector3 pointC = (indexC < 0) ? borderVerts[-indexC - 1] : verts[indexC];

        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;
        return Vector3.Cross(AB, AC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = CalcNormals();
        return mesh;
    }
}
