using System;
using NCG.Swagaria.Runtime.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using TileData = NCG.Swagaria.Runtime.Data.TileData;

namespace NCG.Swagaria.Runtime.Generation
{
    public class Chunk
    {
        //gets the neighbouring chunks 
        //_drawColliders for testing ONLY
        private readonly bool _drawColliders;
        private readonly CreateMesh _meshCreator;
        private readonly TerrainGeneration _terrainGen;

        private readonly MeshFilter _meshFilter;

        //chunk creation
        public readonly GameObject ChunkObj;
        private TileData[,] _tiles;

        public Chunk(int rootX, int rootY, int chunkSize, GameObject prefab, Transform parentObj, TerrainGeneration terrainGen, bool drawColliders)
        {
            _drawColliders = drawColliders;

            RootX = rootX;
            RootY = rootY;

            ChunkObj = Object.Instantiate(prefab, new Vector3(rootX, rootY, 0), Quaternion.identity, parentObj);
            ChunkObj.name = $"Chunk Position: {RootX} | {RootY}";
            _terrainGen = terrainGen;

            _meshFilter = ChunkObj.GetComponent<MeshFilter>();
            _meshCreator = new CreateMesh();

            CreateTileData(chunkSize);
            DrawChunk();
        }

        //collisions only
        public Chunk NeighborLeft { get; set; }
        public Chunk NeighborRight { get; set; }
        public Chunk NeighborTop { get; set; }
        public Chunk NeighborBottom { get; set; }

        private void CreateTileData(int chunkSize)
        {
            _tiles = new TileData[chunkSize, chunkSize];

            for (var x = 0; x < _tiles.GetLength(0); x++)
            for (var y = 0; y < _tiles.GetLength(1); y++)
                _tiles[x, y] = TerrainConfig.GetWorldTile(x, y, RootX, RootY);
        }

        private void DrawChunk()
        {
            var newMesh = _meshCreator.CalculateMesh(_tiles);
            _meshFilter.mesh = newMesh;

            if (_drawColliders)
            {
                foreach (var collider in ChunkObj.GetComponents<BoxCollider2D>())
                    Object.Destroy(collider);

                for (var x = 0; x < _tiles.GetLength(0); x++)
                for (var y = 0; y < _tiles.GetLength(1); y++)
                    if (_tiles[x, y].isSolid && IsTileTouchingAirWithNeighbors(x, y))
                        AddTileCollider(x, y);
            }
        }

        public void UpdateTile(int posX, int posY, TileData newTile)
        {
            if (!IsInBounds(posX, posY))
                throw new Exception("Tried to update tile outside of chunk bounds.");

            // Update tile
            _tiles[posX, posY] = newTile;

            // Update nearby tiles' physics
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > 1) continue;
                UpdateTileCollider(posX + x, posY + y);
            }

            DrawChunk();
        }

        #region Colliders

        private void UpdateTileCollider(int posX, int posY)
        {
            if (!IsInBounds(posX, posY))
                return;

            var tile = _tiles[posX, posY];
            if (!tile.isSolid)
                return;

            // Remove existing collider at this position
            foreach (var collider in ChunkObj.GetComponents<BoxCollider2D>())
                if (collider.offset == new Vector2(posX + 0.5f, posY + 0.5f))
                    Object.Destroy(collider);

            // Add a collider if the tile is touching air
            if (IsTileTouchingAirWithNeighbors(posX, posY))
                AddTileCollider(posX, posY);
        }

        private void AddTileCollider(int x, int y)
        {
            var collider = ChunkObj.AddComponent<BoxCollider2D>();
            collider.offset = new Vector2(x + 0.5f, y + 0.5f);
            collider.size = Vector2.one;
        }

        private bool IsTileTouchingAirWithNeighbors(int x, int y)
        {
            if (x == 0)
            {
                if (NeighborLeft != null && !NeighborLeft.GetTile(_tiles.GetLength(0) - 1, y).isSolid)
                    return true;
            }
            else if (!GetTileFromChunk(x - 1, y).isSolid)
            {
                return true;
            }


            if (x == _tiles.GetLength(0) - 1)
            {
                if (NeighborRight != null && !NeighborRight.GetTile(0, y).isSolid)
                    return true;
            }
            else if (!GetTileFromChunk(x + 1, y).isSolid)
            {
                return true;
            }


            if (y == 0)
            {
                if (NeighborBottom != null && !NeighborBottom.GetTile(x, _tiles.GetLength(1) - 1).isSolid)
                    return true;
            }
            else if (!GetTileFromChunk(x, y - 1).isSolid)
            {
                return true;
            }

            if (y == _tiles.GetLength(1) - 1)
            {
                if (NeighborTop != null && !NeighborTop.GetTile(x, 0).isSolid)
                    return true;
            }
            else if (!GetTileFromChunk(x, y + 1).isSolid)
            {
                return true;
            }

            return false;
        }

        #endregion Colliders

        #region Helpers

        private TileData GetTileFromChunk(int x, int y)
        {
            if (x < 0)
                return NeighborLeft?.GetTile(NeighborLeft._tiles.GetLength(0) - 1, y) ?? TileData.Air();
            if (x >= _tiles.GetLength(0))
                return NeighborRight?.GetTile(0, y) ?? TileData.Air();
            if (y < 0)
                return NeighborBottom?.GetTile(x, NeighborBottom._tiles.GetLength(1) - 1) ?? TileData.Air();
            if (y >= _tiles.GetLength(1))
                return NeighborTop?.GetTile(x, 0) ?? TileData.Air();
            return IsInBounds(x, y) ? _tiles[x, y] : TileData.Air();
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1);
        }

        public TileData GetTile(int localX, int localY)
        {
            return IsInBounds(localX, localY) ? _tiles[localX, localY] : TileData.Air();
        }

        public int RootX { get; }

        public int RootY { get; }

        public int ChunkSize => _tiles.GetLength(0);

        #endregion Helpers
    }
}