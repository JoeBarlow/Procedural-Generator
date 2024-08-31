using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

[CustomEditor(typeof(SCR_MarchingCubes))]
public class Editor_TerrainGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        SCR_MarchingCubes generator = (SCR_MarchingCubes)target;
        Transform waterPlane = GameObject.FindGameObjectWithTag("Water").transform;

        generator.unitVoxelSize = EditorGUILayout.IntField(new GUIContent("Unit Voxel Size", "Increasing this will improve load times but lower quality"), generator.unitVoxelSize);
        {
            GUI.enabled = false;
            generator.isoLevel = EditorGUILayout.FloatField(new GUIContent("Isosurface Level", "Minimum value of the isosurface"), generator.isoLevel);
            GUI.enabled = true;
        }
        generator.seed = EditorGUILayout.IntField("Seed", generator.seed);
        generator.floorOffset = EditorGUILayout.FloatField(new GUIContent("Floot Offset", "Added weight in noise values on the y-axis"), generator.floorOffset);

        EditorGUILayout.LabelField("\nFrequency / Amplitude\n", EditorStyles.boldLabel);
        SerializedProperty octaves = serializedObject.FindProperty("octaveSettings");
        EditorGUILayout.PropertyField(octaves);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.LabelField("\nChunk Settings\n", EditorStyles.boldLabel);
        generator.chunkSize = EditorGUILayout.Vector3IntField("Chunk Size", generator.chunkSize);
        generator.showBorder = EditorGUILayout.Toggle("Show Border", generator.showBorder);
        generator.midPoint = EditorGUILayout.Toggle(new GUIContent("Use Midpoint", "Uses the midpoint to build triangles in marching cubes. Default uses interpolation."), generator.midPoint);


        if (GUILayout.Button("Reset To Default Values"))
        {
            Vector2[] defaultValues =
            {
                new Vector2(0.02f, 64f),
                new Vector2(0.04f, 32f),
                new Vector2(0.08f, 16f),
                new Vector2(0.16f, 8f),
                new Vector2(0.32f, 4f),
                new Vector2(0.64f, 2f)
            };

            generator.octaveSettings = defaultValues;
            generator.seed = 14;
            generator.unitVoxelSize = 2;
            generator.floorOffset = 16;
            generator.chunkSize = new Vector3Int(150, 64, 150);
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Load Biome Preset"))
        {
            generator.octaveSettings = generator.biomePreset.octaveSettings;
            generator.floorOffset = generator.biomePreset.floorOffset;
        }

        #region Clamps
        {
            generator.unitVoxelSize = Mathf.Clamp(generator.unitVoxelSize, 1, 20);
            generator.floorOffset = Mathf.Clamp(generator.floorOffset, -100, 100);
        }
        #endregion

        #region Resize Water Plane
        {
            waterPlane.transform.position = new Vector3(generator.chunkSize.x * 0.5f, 0f, generator.chunkSize.z * 0.5f);
            waterPlane.transform.localScale = new Vector3(generator.chunkSize.x * 0.1f, 1f, generator.chunkSize.z * 0.1f);
        }
        #endregion
    }
}
