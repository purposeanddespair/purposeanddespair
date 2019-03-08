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
        public Tile(TileType type, int powerId, Directions connections = Directions.None, TileStatus status = TileStatus.Unpowered)
        {
            Type = type;
            Connections = connections;
            Status = type == TileType.Server ? TileStatus.Powered : status;
            PowerId = powerId;
        }

        public TileType Type { get; }
        public Directions Connections { get; }
        public TileStatus Status { get; }
        public int PowerId { get; }

        public Tile Rotate()
        {
            return new Tile(Type, PowerId, Connections.Rotate(), Status);
        }

        public Tile WithType(TileType type)
        {
            return new Tile(type, PowerId, Connections, Status);
        }

        public Tile WithStatus(TileStatus status)
        {
            return new Tile(Type, PowerId, Connections, status);
        }

        public Tile WithStatusAndPowerId(TileStatus status, int powerId)
        {
            return new Tile(Type, powerId, Connections, status);
        }

        public Tile WithNewConnection(Direction direction)
        {
            return new Tile(Type, PowerId, Connections.Add(direction), Status);
        }

        public IEnumerable<TilePos> PointsTo(TilePos myPos)
        {
            return Connections
                .Components()
                .Select(dir => myPos.Move(dir));
        }
    }
}