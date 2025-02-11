using System;
using NCG.Swagaria.Runtime.Data;
using NCG.Swagaria.Runtime.Generation;
using UnityEngine;

namespace NCG.Swagaria.Runtime.Player
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private TerrainGeneration terrainGen;

        [Header("Move Settings")] [SerializeField]
        private float moveSpeed = 10f;

        private Camera _camera;
        private TileData _toPlace;
        private Vector2 _worldBounds;

        private void Start()
        {
            _worldBounds = terrainGen.worldSize;
            _camera = Camera.main;
        }

        private void Update()
        {
            var mousePos = Input.mousePosition;
            var worldPos = _camera.ScreenToWorldPoint(mousePos);

            HandleMovement();
            //HandleTileHover(mousePos, worldPos);
            ModifyTile(worldPos);
        }

        private void ModifyTile(Vector3 worldPos)
        {
            var worldX = Mathf.FloorToInt(worldPos.x);
            var worldY = Mathf.FloorToInt(worldPos.y);
            if (worldX < 0 || worldX >= _worldBounds.x || worldY < 0 || worldY >= _worldBounds.y)
                throw new Exception("Mouse is outside the world bounds");

            var chunkX = worldX / terrainGen.chunkSize;
            var chunkY = worldY / terrainGen.chunkSize;
            var chunk = terrainGen.GetChunk(chunkX, chunkY);
            if (chunk is null)
                throw new Exception($"No chunk found for tile: {worldX} / {worldY}");

            var localX = worldX % terrainGen.chunkSize;
            var localY = worldY % terrainGen.chunkSize;

            if (Input.GetKeyDown(KeyCode.Alpha0))
                _toPlace = TileData.Air();
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                _toPlace = TileData.Dirt(worldX, worldY, chunkX, chunkY);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                _toPlace = TileData.Grass(worldX, worldY, chunkX, chunkY);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                _toPlace = TileData.Stone(worldX, worldY, chunkX, chunkY);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                _toPlace = TileData.Wood(worldX, worldY, chunkX, chunkY);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                _toPlace = TileData.Glowstone(worldX, worldY, chunkX, chunkY);

            if (Input.GetMouseButton(0))
            {
                chunk.UpdateTile(localX, localY, _toPlace);
            }
        }


        private void HandleMovement()
        {
            var hInput = Input.GetAxis("Horizontal");
            var vInput = Input.GetAxis("Vertical");

            var moveVector = new Vector3(hInput, vInput, 0) * (moveSpeed * Time.deltaTime);
            var newPosition = transform.position + moveVector;

            newPosition.x = Mathf.Clamp(newPosition.x, 0 + _camera.orthographicSize * _camera.aspect,
                _worldBounds.x - _camera.orthographicSize * _camera.aspect);
            newPosition.y = Mathf.Clamp(newPosition.y, 0 + _camera.orthographicSize,
                _worldBounds.y - _camera.orthographicSize);

            transform.position = newPosition;
        }

        private void HandleTileHover(Vector3 mousePos, Vector3 worldPos)
        {
            var worldX = Mathf.FloorToInt(worldPos.x);
            var worldY = Mathf.FloorToInt(worldPos.y);
            if (worldX < 0 || worldX >= terrainGen.worldSize.x || worldY < 0 || worldY >= terrainGen.worldSize.y)
                throw new Exception("Mouse is outside the world bounds.");

            var chunkX = worldX / terrainGen.chunkSize;
            var chunkY = worldY / terrainGen.chunkSize;
            var chunk = terrainGen.GetChunk(chunkX, chunkY);
            if (chunk is null)
                throw new Exception("No chunk found at these coordinates.");

            var localTileX = worldX % terrainGen.chunkSize;
            var localTileY = worldY % terrainGen.chunkSize;
            var tileData = chunk.GetTile(localTileX, localTileY);
            Debug.Log($"Tile at ({worldX}, {worldY}): {tileData.type.ToString()}");
        }
    }
}