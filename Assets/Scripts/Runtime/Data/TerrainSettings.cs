using NCG.Swagaria.Runtime.Generation;
using UnityEngine;

namespace NCG.Swagaria.Runtime.Data
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Swagaria/World Settings")]
    public class TerrainSettings : ScriptableObject
    {
        public int seed;

        //surface
        public float terrainFreq;
        public int terrainOctaves;
        public float surfaceThreshold;
        public int terrainHeightMultiplier;
        public int terrainHeightAddition;
        public int dirtLayerHeight;

        //deep caves
        public float caveFrequency;
        public int caveOctaves;
        public float caveThreshold;
        public Chunk[,] Chunks;

        //[Header("Ores")]
        //public OreClass Ores
    }
}