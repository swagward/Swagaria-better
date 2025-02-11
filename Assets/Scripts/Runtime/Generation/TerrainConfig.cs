using UnityEngine;
using NCG.Swagaria.Runtime.Data;

namespace NCG.Swagaria.Runtime.Generation
{
    public static class TerrainConfig
    {
        public static TerrainSettings Settings;

        public static void Initialise(bool regenSeed = false)
        {
            Settings = Resources.Load<TerrainSettings>("World Settings");

            if (Settings.seed == 0 || regenSeed)
                Settings.seed = GetSeed();
        }

        private static int GetSeed()
        {
            return Random.Range(-32676, 32676);
        }

        public static TileData GetWorldTile(int x, int y, int rootX, int rootY)
        {
            var height = GetHeight(x, rootX);
            if (y + rootY > height)
                return TileData.Air();

            var surfaceCaves = GetSurfaceCaves(x, y, rootX, rootY);
            var deepCaves = GetDeepCaves(x, y, rootX, rootY);
            if (deepCaves > Settings.caveThreshold)
                return TileData.StoneWall(x, y, rootX, rootY);
            if (surfaceCaves > Settings.surfaceThreshold)
                return TileData.DirtWall(x, y, rootX, rootY);
            
            if(x == rootX && y == rootY)
                return TileData.Glowstone(x, y, rootX, rootY);

            if (y < height - rootY - Settings.dirtLayerHeight - Random.Range(-3, 3))
                return TileData.Stone(x, y, rootX, rootY);
            if (y < height - rootY - 1)
                return TileData.Dirt(x, y, rootX, rootY);
            if (y <= height - rootY)
                return TileData.Grass(x, y, rootX, rootY);

            return TileData.Air();
        }

        private static float GetHeight(int x, int rootX)
        {
            var noiseHeight = 0f;
            var freq = Settings.terrainFreq;
            for (var i = 0; i < Settings.terrainOctaves; i++)
            {
                noiseHeight += Mathf.PerlinNoise((x + rootX + Settings.seed) * freq, Settings.seed * freq) * 2 - 1;
                freq *= 1.5f;
            }

            noiseHeight /= Settings.terrainOctaves;
            noiseHeight = noiseHeight * Settings.terrainHeightMultiplier + Settings.terrainHeightAddition;

            return noiseHeight;
        }

        private static float GetSurfaceCaves(int x, int y, int rootX, int rootY)
        {
            var surfaceCaves = 0f;
            var surfaceFreq = Settings.caveFrequency / 1.25f;
            if (y > GetHeight(x, rootX) - rootY - Settings.dirtLayerHeight)
                for (var i = 0; i < Settings.terrainOctaves; i++)
                {
                    surfaceCaves += Mathf.PerlinNoise(1 - (x + Settings.seed + rootX) * surfaceFreq,
                        1 - (y + Settings.seed + rootY) * surfaceFreq);
                    surfaceFreq *= 1.5f;
                }

            return surfaceCaves /= Settings.terrainOctaves;
        }

        private static float GetDeepCaves(int x, int y, int rootX, int rootY)
        {
            var noiseValue = 0f;
            var caveFreq = Settings.caveFrequency;

            if (y < GetHeight(x, rootX) - rootY - Settings.dirtLayerHeight)
            {
                noiseValue += Mathf.PerlinNoise((x + Settings.seed + rootX) * caveFreq,
                    (y + Settings.seed + rootY) * caveFreq);

                for (var i = 0; i < Settings.caveOctaves; i++)
                {
                    noiseValue += Mathf.PerlinNoise((x + Settings.seed + rootX) * caveFreq,
                        (y + Settings.seed + rootY) * caveFreq);
                    caveFreq *= 1.5f;
                }
            }

            return noiseValue /= Settings.caveOctaves;
        }
    }
}