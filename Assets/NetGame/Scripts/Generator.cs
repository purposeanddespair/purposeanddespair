using System;
using System.Linq;
using System.Collections.Generic;

namespace NetGame
{
    public class Generator
    {
        public enum ServerPosition
        {
            Centered,
            Random
        }

        public int Width { get; set; } = 5;
        public int Height { get; set; } = 5;
        public int Seed { get; set; } = new Random().Next();
        public ServerPosition ServerPos { get; set; } = ServerPosition.Centered;
        public int ServerCount { get; set; } = 1;
        public int PowerCount { get; set; } = 1;

        public NetGame Generate()
        {
            validateParameters();
            Random random = new Random(Seed);
            Tile[] tiles = Enumerable.Repeat(new Tile(TileType.Terminal, 0), Width * Height).ToArray();

            // Generate servers
            var servers = generateServers(random);
            foreach (var server in servers)
                tiles[server.pos.ToIndex(Width)] = new Tile(TileType.Server, server.powerId);

            // Generate wires
            int remainingTiles = tiles.Length - servers.Count();
            var branchHeads = new List<TilePos>(servers.Select(server => server.pos));
            while (branchHeads.Any() && remainingTiles > 0)
            {
                // Find next step
                int headIndex = random.Next(branchHeads.Count);
                var curPos = branchHeads[headIndex];
                var nextSteps = findNextSteps(curPos, tiles).ToArray();
                if (nextSteps.Length <= 1)
                    branchHeads.RemoveAt(headIndex);
                if (nextSteps.Length == 0)
                    continue;

                // Add new step
                var nextStep = nextSteps[random.Next(nextSteps.Length)];
                var nextPos = curPos + nextStep;
                int powerId = tiles[curPos.ToIndex(Width)].PowerId;
                tiles[curPos.ToIndex(Width)] = tiles[curPos.ToIndex(Width)].WithNewConnection(nextStep);
                tiles[nextPos.ToIndex(Width)] = new Tile(TileType.Wire, powerId, nextStep.Mirror().Expand());
                remainingTiles--;
                branchHeads.Add(nextPos);
            }
            if (tiles.Any(tile => tile.Type == TileType.Terminal))
                throw new Exception("Assertion failed: grid was not fully filled");

            // Convert dead-ends into terminals
            var terminalIndices = tiles
                .Select((tile, index) => (tile, index))
                .Where(t => t.tile.Type == TileType.Wire)
                .Where(t => t.tile.Connections.Components().Count() == 1)
                .Select(t => t.index);
            foreach (var i in terminalIndices)
                tiles[i] = tiles[i].WithType(TileType.Terminal);

            return new NetGame(tiles, Width);
        }

        private IEnumerable<(TilePos pos, int powerId)> generateServers(Random random)
        {
            if (ServerPos == ServerPosition.Centered)
                return new[] { (new TilePos(Width / 2, Height / 2), 0) };

            if (ServerPos != ServerPosition.Random)
                throw new NotImplementedException();
            var servers = new HashSet<TilePos>();
            for (int i = 0; i < ServerCount; i++)
            {
                TilePos newPos;
                do
                {
                    newPos = TilePos.FromIndex(random.Next(Width * Height), Width);
                } while (!servers.Add(newPos));
            }
            return servers
                .Select((pos, i) => (
                    pos,
                    i < PowerCount ? i : random.Next(PowerCount)
                ));
        }

        private IEnumerable<Direction> findNextSteps(TilePos tilePos, Tile[] tiles)
        {
            if (tiles[tilePos.ToIndex(Width)].Connections.Count() >= 3)
                return Enumerable.Empty<Direction>();
            return Directions.All.Components()
                .Select(dir => (dir, nextPos: tilePos + dir))
                .Where(t => t.nextPos.IsValid(Width, Height))
                .Where(t => tiles[t.nextPos.ToIndex(Width)].Type == TileType.Terminal)
                .Select(t => t.dir);
        }

        private void validateParameters()
        {
            if (Width <= 0)
                throw new ArgumentOutOfRangeException("Width");
            if (Height <= 0)
                throw new ArgumentOutOfRangeException("Height");
            if (ServerCount > 1 && ServerPos == ServerPosition.Centered)
                throw new ArgumentException("There cannot be more than one centered server");
            if (ServerCount <= 0 || ServerCount > Width * Height)
                throw new ArgumentOutOfRangeException("ServerCount");
            if (PowerCount < 1)
                throw new ArgumentOutOfRangeException("PowerCount");
            if (PowerCount > ServerCount)
                throw new ArgumentException("There needs to be at least as many servers as power types");
        }
    }
}
