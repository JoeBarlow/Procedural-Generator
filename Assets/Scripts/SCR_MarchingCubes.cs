using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SCR_MarchingCubes : MonoBehaviour
{
    private const bool INVALID = false; //A voxel does not need to be drawn
    private const int MAX_CHUNK_VERT_COUNT = 60000; //Maximum number of vertices allowed in a unity mesh

    //Mesh Data
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 randomFloat;
    //------------------------------------------------

    //Terrain Generator Settings
    [HideInInspector] public int unitVoxelSize = 1;
    [HideInInspector] public float isoLevel;
    [HideInInspector] public int seed;
    [HideInInspector] public Vector2[] octaveSettings;
    [HideInInspector] public Vector3Int chunkSize;
    [HideInInspector] public bool showBorder = false;
    [HideInInspector] public bool midPoint = true;
    [HideInInspector] public float floorOffset;
    //------------------------------------------------

    //Biome Preset Scriptable Object
    [Header("Scriptable Object")]
    public Biome_Preset biomePreset;
    //------------------------------------------------
    private void Start()
    {
        //Sets random float used to offset voxel vertex positions
        randomFloat = new Vector3();
        SeedRandom();

        //Sets up mesh
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        GenerateChunk();
    }

    private void Update()
    {
        //Used to re-generate chunks whilst in playmode
        if (Input.GetKeyDown(KeyCode.G))
        {
            SeedRandom();
            GenerateChunk();
        }
    }

    private void GenerateChunk()
    {
        int iterationCounter = 0; //Tracks how many vertices are created

        Dictionary<Vector3, int> vertDictionary = new Dictionary<Vector3, int>();
        List<int> triangles = new List<int>();
        int vertIndex = 0;

        //Loops through chunk size and computes mesh data
        for (int x = 0; x < chunkSize.x; x += unitVoxelSize)
        {
            for (int y = 0; y < chunkSize.y; y += unitVoxelSize)
            {
                for (int z = 0; z < chunkSize.z; z += unitVoxelSize)
                {

                    Voxel currentVoxel = new Voxel(new Vector3(x, y, z), unitVoxelSize);
                    currentVoxel.GetCaseIndex(isoLevel, floorOffset, randomFloat, octaveSettings); //Applies Perlin Noise to voxel
                    if (currentVoxel.ProcessCaseIndex(isoLevel, midPoint) == INVALID) //Uses vertex density to create triangle data
                    { 
                        continue; //Continues to next voxel if no drawing is needed
                    } 

                    //Adds required vertices to a dictionary
                    for (int i = 0; i < currentVoxel.InterpolatedVertices.Length; i++)
                    {
                        //Ensures no duplicates are added
                        if (!vertDictionary.ContainsKey(currentVoxel.InterpolatedVertices[i]))
                        {
                            vertDictionary.Add(currentVoxel.InterpolatedVertices[i], vertIndex);
                            vertIndex++;
                        }
                    }

                    //Maps triangle data to dictionary vertices
                    for (int i = 0; i < currentVoxel.TriangleData.Length; i++)
                    {
                        int value = vertDictionary[currentVoxel.TriangleData[i]];
                        triangles.Add(value);
                    }

                    //Throws an error when Unity's max mesh vertex count is reached
                    iterationCounter++;
                    if (iterationCounter >= MAX_CHUNK_VERT_COUNT)
                    {
                        throw new System.ArgumentException("Too many vertices for mesh! (Try lowering the chunk size or unit voxel size)");
                    }
                }
            }
        }

        //Loads chunk data into mesh
        mesh.Clear();
        mesh.vertices = vertDictionary.Keys.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Randomly offsets the xyz of the perlin noise
    /// </summary>
    private void SeedRandom()
    {
        Random.InitState(seed);
        randomFloat.x = Random.Range(-100f, 100f);
        randomFloat.y = Random.Range(-100f, 100f);
        randomFloat.z = Random.Range(-100f, 100f);
    }

    //Code from Carlpilot - https://www.youtube.com/watch?v=Aga0TBJkchM
    public static float Perlin3D(Vector3 worldSpace)
    {
        float x = worldSpace.x;
        float y = worldSpace.y;
        float z = worldSpace.z;

        float AB = Perlin2D(x, y);
        float BC = Perlin2D(y, z);
        float AC = Perlin2D(x, z);

        float BA = Perlin2D(y, x);
        float CB = Perlin2D(z, y);
        float CA = Perlin2D(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;
    }
    /// <summary>
    /// Returns values in range [-1, 1]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static float Perlin2D(float x, float y)
    {
        return Mathf.PerlinNoise(x, y) * 2 - 1;
    }

    private void OnDrawGizmos()
    {
        if (!showBorder || unitVoxelSize == 0) { return; }

        Gizmos.color = Color.green;
        float t = unitVoxelSize * 0.5f;

        Vector3 borderSize = new Vector3(chunkSize.x, chunkSize.y, chunkSize.z);
        Vector3 cornerPos = new Vector3(chunkSize.x * 0.5f, chunkSize.y * 0.5f, chunkSize.z * 0.5f);
        Gizmos.DrawWireCube(cornerPos, borderSize);
    }
}
