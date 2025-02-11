using System.Collections.Generic;
using UnityEngine;
using NCG.Swagaria.Runtime.Data;

namespace NCG.Swagaria.Runtime.Generation
{
    public class CreateMesh
    {
        private const float TileSize = 0.125f; //increments of images in the atlas
        private readonly List<int> _triangles = new();

        private readonly List<Vector2> _uvs = new();

        //mesh data
        private readonly List<Vector3> _vertices = new();

        public Mesh CalculateMesh(TileData[,] tiles)
        {
            _vertices.Clear();
            _triangles.Clear();
            _uvs.Clear();

            var triIndex = 0;

            for (var x = 0; x < tiles.GetLength(0); x++)
            for (var y = 0; y < tiles.GetLength(1); y++)
            {
                var tile = tiles[x, y];

                if (tile.type != (byte)TileType.Air)
                {
                    DrawTile(x, y, triIndex, tile);
                    triIndex += 4;
                }
            }

            var newMesh = new Mesh
            {
                vertices = _vertices.ToArray(),
                triangles = _triangles.ToArray(),
                uv = _uvs.ToArray()
            };

            newMesh.RecalculateNormals();
            return newMesh;
        }

        private void DrawTile(int x, int y, int triIndex, TileData tile)
        {
            _vertices.Add(new Vector3(x, y, 0)); // bottom left
            _vertices.Add(new Vector3(x, y + 1, 0)); // top left
            _vertices.Add(new Vector3(x + 1, y + 1, 0)); // top right
            _vertices.Add(new Vector3(x + 1, y, 0)); // bottom right

            _triangles.Add(triIndex + 0); //triangle 1
            _triangles.Add(triIndex + 1);
            _triangles.Add(triIndex + 2);

            _triangles.Add(triIndex + 2); //triangle 2
            _triangles.Add(triIndex + 3);
            _triangles.Add(triIndex + 0);

            _uvs.Add(new Vector2(tile.UVOffset.x, tile.UVOffset.y));
            _uvs.Add(new Vector2(tile.UVOffset.x, tile.UVOffset.y + TileSize));
            _uvs.Add(new Vector2(tile.UVOffset.x + TileSize, tile.UVOffset.y + TileSize));
            _uvs.Add(new Vector2(tile.UVOffset.x + TileSize, tile.UVOffset.y));
        }
    }
}