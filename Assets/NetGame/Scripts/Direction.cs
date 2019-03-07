using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    /// <summary>
    /// A single direction (ordered for rotation)
    /// </summary>
    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    /// <summary>
    /// Zero or more directions
    /// </summary>
    [Flags]
    public enum Directions
    {
        Up = (1 << (int)Direction.Up),
        Right = (1 << (int)Direction.Right),
        Down = (1 << (int)Direction.Down),
        Left = (1 << (int)Direction.Left),

        None = 0,
        All = Up | Right | Down | Left
    }

    public static class DirectionUtils
    {
        public static int XDelta(this Direction direction)
        {
            return
                (direction == Direction.Left ? -1 : 0) +
                (direction == Direction.Right ? 1 : 0);
        }

        public static int YDelta(this Direction direction)
        {
            return
                (direction == Direction.Up ? 1 : 0) +
                (direction == Direction.Down ? -1 : 0);
        }

        public static Direction Mirror(this Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
        }

        public static Direction Rotate(this Direction direction)
        {
            return (Direction)(((int)direction + 1) % 4);
        }

        public static Directions Rotate(this Directions directions)
        {
            return directions
                .Components()
                .Select(dir => dir.Rotate())
                .Aggregate(Directions.None, (dirs, dir) => dirs.Add(dir));
        }

        public static Directions Expand(this Direction direction)
        {
            return (Directions)(1 << (int)direction);
        }

        public static Directions Add(this Directions directions, Direction direction)
        {
            return directions | (Directions)(1 << (int)direction);
        }

        public static Directions Remove(this Directions directions, Direction direction)
        {
            return directions & (Directions)~(1 << (int)direction);
        }

        public static Directions Remove(this Directions directions, Directions toBeRemoved)
        {
            return directions & (Directions)~(int)toBeRemoved;
        }

        public static bool HasDirection(this Directions directions, Direction direction)
        {
            return directions.HasFlag((Directions)(1 << (int)direction));
        }

        public static IEnumerable<Direction> Components(this Directions directions)
        {
            return Enumerable.Range(0, 4)
                .Where(flagBit => directions.HasDirection((Direction)flagBit))
                .Select(dir => (Direction)dir);
        }

        public static int Count(this Directions directions)
        {
            return Enumerable.Range(0, 4)
                .Sum(flagBit => directions.HasDirection((Direction)flagBit) ? 1 : 0);
        }

        public static bool Any(this Directions directions)
        {
            return directions != Directions.None;
        }

        public static bool All(this Directions directions)
        {
            return directions == Directions.All;
        }

        public static Direction Single(this Directions directions)
        {
            var components = directions.Components().ToArray();
            if (components.Length != 1)
                throw new Exception("No or multiple directions given");
            return components[0];
        }
    }
}