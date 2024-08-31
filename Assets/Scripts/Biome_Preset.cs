using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome Preset", menuName = "ScriptableObjects/Biome Preset")]
public class Biome_Preset : ScriptableObject
{
    public float floorOffset = 0;
    [Header("Frequency / Amplitude")]
    public Vector2[] octaveSettings;
}
