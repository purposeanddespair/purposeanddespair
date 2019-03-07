using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    public enum TileType
    {
        Wire,
        Terminal,
        Server
    }

    public enum TileStatus
    {
        Unpowered,
        Powered,
        Cyclic
    }

    public struct Tile
    {
        public Tile(TileType type, Directions connections = Directions.None, TileStatus status = TileStatus.Unpowered)
        {
            Type = type;
            Connections = connections;
            Status = type == TileType.Server ? TileStatus.Powered : status;
        }

        public TileType Type { get; }
        public Directions Connections { get; }
        public TileStatus Status { get; }

        public Tile Rotate()
        {
            return new Tile(Type, Connections.Rotate(), Status);
        }

        public Tile WithType(TileType type)
        {
            return new Tile(type, Connections, Status);
        }

        public Tile WithStatus(TileStatus status)
        {
            return new Tile(Type, Connections, status);
        }

        public Tile WithNewConnection(Direction direction)
        {
            return new Tile(Type, Connections.Add(direction), Status);
        }

        public bool ConnectsTo(Direction direction)
        {
            return Connections.HasDirection(direction);
        }

        public IEnumerable<TilePos> ConnectedTiles(TilePos myPos)
        {
            return Connections
                .Components()
                .Select(dir => myPos.Move(dir));
        }

        public IEnumerable<TilePos> ConnectedTilesFrom(TilePos myPos, Direction from)
        {
            return Connections
                .Components().Except(from)
                .Select(dir => myPos.Move(dir));
        }
    }
}