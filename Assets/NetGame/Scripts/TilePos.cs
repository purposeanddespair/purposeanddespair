using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    public struct TilePos
    {
        public int X { get; }
        public int Y { get; }

        public TilePos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static TilePos FromIndex(int index, int width)
        {
            return new TilePos(index % width, index / width);
        }

        public int ToIndex(int width)
        {
            return Y * width + X;
        }

        public bool IsValid(int width, int height)
        {
            return X >= 0 && X < width && Y >= 0 && Y < height;
        }

        public TilePos Move(Direction dir, int amount = 1)
        {
            return new TilePos(
                X + amount * dir.XDelta(),
                Y + amount * dir.YDelta()
            );
        }

        public static TilePos operator + (TilePos pos, Direction dir)
        {
            return pos.Move(dir, 1);
        }

        public static TilePos operator - (TilePos pos, Direction dir)
        {
            return pos.Move(dir, -1);
        }

        public static bool operator == (TilePos a, TilePos b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator != (TilePos a, TilePos b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        /// <summary>
        /// Finds the von-Neumann-Neighborhood
        /// </summary>
        /// <returns>Set of surrounding tile positions</returns>
        public IEnumerable<TilePos> Surrounding()
        {
            foreach (var dir in Directions.All.Components())
                yield return this + dir;
        }
    }
}