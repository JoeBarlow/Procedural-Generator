using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A class that acts as an individual marching cube
/// </summary>
public class Voxel
{
    private const int VOXEL_VERTEX_COUNT = 8;
    private const int AIR_CONST = 0; //Voxel is entirely in the air
    private const int GROUND_CONST = 255; //Voxel is entirely submerged in the ground

    private Vector3 position; //From corner of voxel
    private Vector3[] vertexPositions;
    private float[] vertexDensities;
    private int caseIndex; //The 8-bit number that represents a voxel's state

    private Vector3[] interpVertices; //Vertices that have been interpolated for better precision
    private Vector3[] triangleData;

    /// <summary>
    /// Position will be the corner of the voxel
    /// </summary>
    /// <param name="position"></param>
    public Voxel(Vector3 position, int unitVoxelSize)
    {
        this.position = position;
        vertexDensities = new float[VOXEL_VERTEX_COUNT];
            
        //Constructs a voxel given the desired unit size
        vertexPositions = new Vector3[VOXEL_VERTEX_COUNT];
        {
            #region Assigning Vertices
            vertexPositions[0] = position;
            vertexPositions[1] = position + (new Vector3(0, 0, 1) * unitVoxelSize);
            vertexPositions[2] = position + (new Vector3(1, 0, 1) * unitVoxelSize);
            vertexPositions[3] = position + (new Vector3(1, 0, 0) * unitVoxelSize);
            vertexPositions[4] = position + (new Vector3(0, 1, 0) * unitVoxelSize);
            vertexPositions[5] = position + (new Vector3(0, 1, 1) * unitVoxelSize);
            vertexPositions[6] = position + (new Vector3(1, 1, 1) * unitVoxelSize);
            vertexPositions[7] = position + (new Vector3(1, 1, 0) * unitVoxelSize);
            #endregion
        }
    }

    /// <summary>
    /// Takes the case index and calculates interpolated vertices and triangle data. 
    /// <para>Returns false if voxel does not need to be drawn.</para> 
    /// </summary>
    /// <param name="isoLevel"></param>
    /// <param name="usingMidPoint"></param>
    /// <returns></returns>
    public bool ProcessCaseIndex(float isoLevel, bool usingMidPoint)
    {
        //Exits function if voxel is entirely in air or ground
        if (caseIndex == AIR_CONST || caseIndex == GROUND_CONST) 
        {
            return false; 
        }

        int[] edgeList = GetEdgeList(); //An array of edges that are intersected by the isosurface (Each voxel has 12 edges)
        int[] triList = GetTriangleList(); //An array of edges that represent the order in which triangles should be drawn

        interpVertices = new Vector3[edgeList.Length];
        triangleData = new Vector3[triList.Length];

        //A loop for calculating vertex positions on an edge
        for (int i = 0; i < edgeList.Length; i++)
        {
            Vector3 posA, posB; //Get both vertices that make up an edge
            posA = vertexPositions[LookupTables.edgeVertexIndices[edgeList[i], 0]]; //Uses a lookup table to find vertex indices, then uses 'vertexPositions' to retrieve world positions
            posB = vertexPositions[LookupTables.edgeVertexIndices[edgeList[i], 1]];

            if (usingMidPoint)
            {
                Vector3 midpoint = (posA + posB) * 0.5f;
                interpVertices[i] = midpoint;
            }
            else //Interpolate between the two vertices using their density
            {
                float v1 = vertexDensities[LookupTables.edgeVertexIndices[edgeList[i], 0]];
                float v2 = vertexDensities[LookupTables.edgeVertexIndices[edgeList[i], 1]];
                float t = (isoLevel - v1) / (v2 - v1);
                interpVertices[i] = posA + t * (posB - posA);
            }

            //A loop for creating triangle data
            for (int j = 0; j < triList.Length; j++)
            {
                //Compares the original triList with the original edgeList - updating triangle data with new vertex when they match
                if (triList[j] == edgeList[i])
                {
                    triangleData[j] = interpVertices[i];
                }
            }
        }

        return true;
    }

    private int[] GetEdgeList()
    {
        int intersectedEdges = LookupTables.edgeTable[caseIndex]; //A 12-bit number that represents which edges are intersected by the isosurface
        List<int> edgeList = new List<int>();

        for (int i = 0; i < 12; i++)
        {
            int currentEdgeCheck = intersectedEdges & (1 << i); //Bitwise AND to check if current edge is equal to 1
            if (currentEdgeCheck >= 1)
            {
                edgeList.Add(i);
            }
        }

        return edgeList.ToArray();
    }

    private int[] GetTriangleList()
    {
        List<int> triList = new List<int>();

        //Adds all edges relevant to specific case to a list
        for (int i = 0; LookupTables.triTable[caseIndex, i] != -1; i+=3) //Iterate until edge value == -1 (Refer to LookupTables.cs for information)
        {
            triList.Add(LookupTables.triTable[caseIndex, i]);
            triList.Add(LookupTables.triTable[caseIndex, i + 1]);
            triList.Add(LookupTables.triTable[caseIndex, i + 2]);
        }

        return triList.ToArray();
    }


    public void GetCaseIndex(float isoLevel, float floorOffset, Vector3 randomFloat, Vector2[] octaveSettings)
    {
        caseIndex = 0;

        //Calculates Perlin Noise for each vertex in voxel
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            Vector3 currentVertex = vertexPositions[i] + randomFloat;
            float noise = 0;

            //Adds layers of noise depending on the number of octaves
            for (int j = 0; j < octaveSettings.Length; j++)
            {
                noise += SCR_MarchingCubes.Perlin3D(currentVertex * octaveSettings[j].x) * octaveSettings[j].y;                  
            }

            vertexDensities[i] = -(vertexPositions[i].y - floorOffset) + noise;
        }

        //Adds 1 bit to the case index for each vertex above the below the isosurface threshold
        for (int i = 0; i < vertexDensities.Length; i++)
        {
            if (vertexDensities[i] < isoLevel)
            {
                caseIndex |= (1 << i);
            }
        }
    }

    public Vector3[] InterpolatedVertices
    {
        get { return interpVertices; }
    }

    public Vector3[] TriangleData
    {
        get { return triangleData; }
    }
}

