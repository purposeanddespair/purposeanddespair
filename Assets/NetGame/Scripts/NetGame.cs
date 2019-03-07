using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    /// <summary>
    /// A generated (playable) instance of a netgame
    /// </summary>
    public class NetGame
    {
        public Tile[] Tiles { get; private set; }
        public int Width { get; }

        public int Height => Tiles.Length / Width;

        public bool Completed => Tiles.All(tile => tile.Status == TileStatus.Powered);

        public NetGame(Tile[] tiles, int width)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (tiles.Length % width != 0)
                throw new ArgumentException("Invalid tiles array", "tiles");
            Tiles = tiles.ToArray();
            Width = width;
        }

        public Tile GetTileAt(TilePos tilePos)
        {
            return Tiles[tilePos.ToIndex(Width)];
        }

        private void setTileAt(TilePos tilePos, Tile tile)
        {
            Tiles[tilePos.ToIndex(Width)] = tile;
        }

        public void RandomizeRotations()
        {
            var random = new Random();
            Tiles = Tiles.Select(tile => {
                int rotateCount = random.Next(4);
                for (int i = 0; i < rotateCount; i++)
                    tile = tile.Rotate();
                return tile;
            }).ToArray();
            updatePower();
        }

        public void RotateAt(TilePos tilePos)
        {
            setTileAt(tilePos, GetTileAt(tilePos).Rotate());
            updatePower();
        }

        private void updatePower()
        {
            Tiles = Tiles.Select(tile => tile.WithStatus(TileStatus.Unpowered)).ToArray();

            var queue = new Queue<TilePos>(Tiles
                .Select((tile, index) => (tile, index))
                .Where(t => t.tile.Type == TileType.Server)
                .Select(t => TilePos.FromIndex(t.index, Width))
            );
            while(queue.Any())
            {
                var currentPos = queue.Dequeue();
                var currentTile = GetTileAt(currentPos);
                var neighbors = currentTile.ConnectedTiles(currentPos);

                var cyclicNeighbors = neighbors.Where(
                    neighborPos => GetTileAt(neighborPos).Status == TileStatus.Powered
                );
                foreach (TilePos cyclicNeighborPos in cyclicNeighbors)
                    markCyclic(currentPos, cyclicNeighborPos);

                foreach (TilePos neighborPos in neighbors.Except(cyclicNeighbors))
                    queue.Append(neighborPos);

                setTileAt(currentPos, currentTile.WithStatus(TileStatus.Powered));
            }
        }

        private void markCyclic(TilePos currentPos, TilePos cyclicNeighborPos)
        {
            // todo: find the alternative path from current to cyclicNeighbor
            setTileAt(currentPos, GetTileAt(currentPos).WithStatus(TileStatus.Cyclic));
        }
    }
}
