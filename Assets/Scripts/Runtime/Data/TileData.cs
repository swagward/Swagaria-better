using UnityEngine;

namespace NCG.Swagaria.Runtime.Data
{
    public struct TileData
    {
        public bool isSolid;
        public bool emitsLight;
        public int lightLevel;
        public readonly byte type;
        public Vector2 UVOffset; //for textures


        private TileData(bool isSolid, bool emitsLight, int lightLevel, int posX, int posY, int chunkX, int chunkY, TileType type, Vector2 uvOffset)
        {
            this.isSolid = isSolid;
            this.emitsLight = emitsLight;
            this.lightLevel = lightLevel;
            this.type = (byte)type;
            UVOffset = uvOffset;
        }

        public static TileData Air()
            => new (false, false, 15, 0, 0, 0, 0, TileType.Air, Vector2.zero);

        public static TileData Grass(int x, int y, int chunkX, int chunkY)
            => new (true, false, 0, x, y, chunkX, chunkY, TileType.Grass, new Vector2(0.125f, 0));

        public static TileData Dirt(int x, int y, int chunkX, int chunkY)
            => new (true, false, 0, x, y, chunkX, chunkY, TileType.Dirt, new Vector2(0.25f, 0));

        public static TileData DirtWall(int x, int y, int chunkX, int chunkY)
            => new (false, false, 0, x, y, chunkX, chunkY, TileType.DirtWall, new Vector2(0.25f, 0.125f));

        public static TileData Stone(int x, int y, int chunkX, int chunkY)
            => new (true, false, 0, x, y, chunkX, chunkY, TileType.Stone, new Vector2(0.375f, 0));

        public static TileData StoneWall(int x, int y, int chunkX, int chunkY)
            => new (false, false, 0, x, y, chunkX, chunkY, TileType.StoneWall, new Vector2(0.375f, 0.125f));

        public static TileData Wood(int x, int y, int chunkX, int chunkY)
            => new (true, false, 0, x, y, chunkX, chunkY, TileType.Wood, new Vector2(0.5f, 0));

        public static TileData Glowstone(int x, int y, int chunkX, int chunkY)
            => new (true, true, 15, x, y, chunkX, chunkY, TileType.Glowstone, new Vector2(0.625f, 0));
    }

    public enum TileType : byte
    {
        Air,
        Grass,
        Dirt,
        DirtWall,
        Stone,
        StoneWall,
        Wood,
        Glowstone
    }
}