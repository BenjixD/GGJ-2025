using UnityEngine;

[CreateAssetMenu(fileName = "LevelBoundsSO", menuName = "Scriptable Objects/LevelBoundsSO")]
public class LevelBoundsSO : ScriptableObject
{
    public float minX, maxX;
    public float minY, maxY;
    public float minZ, maxZ;
}
