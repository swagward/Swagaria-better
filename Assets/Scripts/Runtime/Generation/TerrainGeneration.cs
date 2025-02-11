using System.Diagnostics;
using System.Runtime.InteropServices;
using NCG.Swagaria.Runtime.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NCG.Swagaria.Runtime.Generation
{
    public class TerrainGeneration : MonoBehaviour
    {
        //debugging
        [SerializeField] public bool checkLoadTime;
        [SerializeField] public bool drawColliders;

        //chunk culling
        [SerializeField] public bool cullChunks;
        [SerializeField] public Transform player;

        //world and chunk size
        public Vector2Int worldSize;
        [Tooltip("Keep below 50 for no mesh tearing")] [Range(1, 100)]
        public int chunkSize;
        private GameObject _chunk;
        
        //lighting
        public ComputeShader lightingShader;
        public GameObject lightingOverlay;
        private RenderTexture _lightingTexture;

        [StructLayout(LayoutKind.Sequential)]
        private struct ComputeTiles
        {
            public int tileType;
            public int emitsLight;
            public int lightLevel;  
        }
        private ComputeTiles[] _tiles;
        private ComputeTiles[] _nextTiles;
        
        private ComputeBuffer _tileBuffer;
        private ComputeBuffer _nextTileBuffer;

        private void Start()
        {
            _chunk = Resources.Load<GameObject>("ChunkPref");
            TerrainConfig.Initialise();
            InitChunks();

            if (checkLoadTime)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                GenerateTerrain();
                stopwatch.Stop();

                var ts = stopwatch.Elapsed;
                var elapsedTime = $"{ts.Minutes}m, {ts.Seconds}s, {ts.Milliseconds}ms";
                Debug.Log($"Load time: {elapsedTime}");
            }
            else
                GenerateTerrain();

            LinkChunkNeighbors();

            InitLighting();
            UpdateLighting();
        }

        private void InitLighting()
        {
            _lightingTexture = new RenderTexture(worldSize.x, worldSize.y, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };
            _lightingTexture.Create();
            lightingOverlay.GetComponent<MeshRenderer>().material.mainTexture = _lightingTexture;

            _tiles = new ComputeTiles[worldSize.x * worldSize.y];
            _nextTiles = new ComputeTiles[worldSize.x * worldSize.y]; // NEW BUFFER

            for (var x = 0; x < worldSize.x; x++)
            {
                for (var y = 0; y < worldSize.y; y++)
                {
                    var index = y * worldSize.x + x;
                    var chunk = GetChunk(x / chunkSize, y / chunkSize);
                    if (chunk != null)
                    {
                        var tile = chunk.GetTile(x % chunkSize, y % chunkSize);

                        // Glowstone should emit max light
                        if (tile.type == (byte)TileType.Glowstone)
                        {
                            _tiles[index].lightLevel = 15;
                            _tiles[index].emitsLight = 1;
                        }
                        // Air should emit some light, around 8
                        else if (tile.type == (byte)TileType.Air)
                        {
                            _tiles[index].lightLevel = 8;
                            _tiles[index].emitsLight = 1;
                        }
                        else
                        {
                            _tiles[index].lightLevel = 0;
                            _tiles[index].emitsLight = 0;
                        }

                        _tiles[index].tileType = tile.type != (byte)TileType.Air ? 1 : 0;
                    }
                    else
                    {
                        // Default sky light behavior (make sure it emits light)
                        _tiles[index].tileType = 0; // Air
                        _tiles[index].lightLevel = 8; // Sky brightness
                        _tiles[index].emitsLight = 1;
                    }

                    _nextTiles[index] = _tiles[index]; // Initialize second buffer
                }
            }

            _tileBuffer = new ComputeBuffer(worldSize.x * worldSize.y, Marshal.SizeOf(typeof(ComputeTiles)), ComputeBufferType.Default);
            _tileBuffer.SetData(_tiles);

            _nextTileBuffer = new ComputeBuffer(worldSize.x * worldSize.y, Marshal.SizeOf(typeof(ComputeTiles)), ComputeBufferType.Default);
            _nextTileBuffer.SetData(_nextTiles);

            var kernelIndex = lightingShader.FindKernel("CSMain");
            lightingShader.SetBuffer(kernelIndex, "Tiles", _tileBuffer);
            lightingShader.SetBuffer(kernelIndex, "NextTiles", _nextTileBuffer); // NEW BUFFER
            lightingShader.SetTexture(kernelIndex, "LightMap", _lightingTexture);
            lightingShader.SetInts("worldSize", worldSize.x, worldSize.y);
        }

        private const int LightIterations = 15; // Run compute shader multiple times

        private void UpdateLighting()
        {
            var kernelHandle = lightingShader.FindKernel("CSMain");
            var threadGroupsX = Mathf.CeilToInt((float)worldSize.x / 8);
            var threadGroupsY = Mathf.CeilToInt((float)worldSize.y / 8);

            // Upload initial tile data to GPU buffers
            _tileBuffer.SetData(_tiles);
            _nextTileBuffer.SetData(_nextTiles);

            for (var i = 0; i < LightIterations; i++)
            {
                lightingShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
                SwapBuffers();
            }

            // Update texture to reflect new lighting data
            lightingOverlay.GetComponent<MeshRenderer>().material.mainTexture = _lightingTexture;
        }

        void SwapBuffers()
        {
            (_tileBuffer, _nextTileBuffer) = (_nextTileBuffer, _tileBuffer);

            lightingShader.SetBuffer(lightingShader.FindKernel("CSMain"), "Tiles", _tileBuffer);
            lightingShader.SetBuffer(lightingShader.FindKernel("CSMain"), "NextTiles", _nextTileBuffer);
        }

        private void Update()
        {
            if (!cullChunks) return;

            var chunkPositionX = Mathf.RoundToInt(player.position.x / chunkSize);
            var chunkPositionY = Mathf.RoundToInt(player.position.y / chunkSize);

            var startX = Mathf.Max(0, chunkPositionX - 2);
            var startY = Mathf.Max(0, chunkPositionY - 2);
            var endX = Mathf.Min(TerrainConfig.Settings.Chunks.GetLength(0) - 1, chunkPositionX + 1);
            var endY = Mathf.Min(TerrainConfig.Settings.Chunks.GetLength(1) - 1, chunkPositionY + 1);

            // Unload all chunks
            for (var x = 0; x < TerrainConfig.Settings.Chunks.GetLength(0); x++)
            for (var y = 0; y < TerrainConfig.Settings.Chunks.GetLength(1); y++)
                TerrainConfig.Settings.Chunks[x, y].ChunkObj.SetActive(false);

            // Activate only the nearby chunks
            for (var x = startX; x <= endX; x++)
            for (var y = startY; y <= endY; y++)
                TerrainConfig.Settings.Chunks[x, y].ChunkObj.SetActive(true);
        }

        private void InitChunks()
        {
            TerrainConfig.Settings.Chunks = new Chunk[worldSize.x / chunkSize, worldSize.y / chunkSize];
        }

        private void GenerateTerrain()
        {
            for (var x = 0; x < worldSize.x; x += chunkSize)
            for (var y = 0; y < worldSize.y; y += chunkSize)
                CreateChunk(x, y);
        }

        private void LinkChunkNeighbors()
        {
            for (var x = 0; x < TerrainConfig.Settings.Chunks.GetLength(0); x++)
            for (var y = 0; y < TerrainConfig.Settings.Chunks.GetLength(1); y++)
            {
                var currentChunk = TerrainConfig.Settings.Chunks[x, y];

                // Link neighbors (null if out of bounds)
                currentChunk.NeighborLeft = GetChunk(x - 1, y);
                currentChunk.NeighborRight = GetChunk(x + 1, y);
                currentChunk.NeighborTop = GetChunk(x, y + 1);
                currentChunk.NeighborBottom = GetChunk(x, y - 1);
            }
        }

        private void CreateChunk(int x, int y)
        {
            var chunkX = x / chunkSize;
            var chunkY = y / chunkSize;
            TerrainConfig.Settings.Chunks[chunkX, chunkY] =
                new Chunk(x, y, chunkSize, _chunk, transform, this, drawColliders);
        }

        public Chunk GetChunk(int chunkX, int chunkY)
        {
            if (chunkX < 0 || chunkX >= TerrainConfig.Settings.Chunks.GetLength(0) ||
                chunkY < 0 || chunkY >= TerrainConfig.Settings.Chunks.GetLength(1))
                return null;

            return TerrainConfig.Settings.Chunks[chunkX, chunkY];
        }
        
        private void OnDestroy()
        {
            _tileBuffer?.Release();
            _nextTileBuffer?.Release();
        }
    }
}