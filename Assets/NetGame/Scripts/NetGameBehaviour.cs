using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;
using NetGame;

public class NetGameBehaviour : MonoBehaviour
{
    private static readonly IReadOnlyDictionary<TileStatus, Color> colorByTileStatus = new Dictionary<TileStatus, Color>()
    {
        { TileStatus.Unpowered, Color.white },
        { TileStatus.Cyclic, Color.red }
    };
    private static readonly Color[] colorByPowerId = new Color[]
    {
        Color.blue, Color.yellow, Color.green
    };

    public SpriteAtlas spriteAtlas;
    public int width = 5, height = 5;
    public int serverCount = 2;
    public Generator.ServerPosition serverPosition = Generator.ServerPosition.Random;
    public int powerCount = 2;

    private Sprite terminalSprite, serverSprite;
    private IReadOnlyDictionary<Directions, Sprite> wireSprites;
    private Tilemap tilemap;
    private Camera netGameCamera;
    private NetGame.NetGame netGame;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        netGameCamera = GetComponent<Camera>();
        createSpriteMap();
    }

    void Update()
    {
    }

    public void GenerateNewPuzzle()
    {
        var generator = new Generator();
        generator.Width = width;
        generator.Height = height;
        generator.ServerCount = serverCount;
        generator.ServerPos = serverPosition;
        generator.PowerCount = powerCount;
        netGame = generator.Generate();
        updateTilemap();
    }

    public void ResetPuzzle()
    {
        netGame.RandomizeRotations();
        updateTilemap();
    }

    public void OnClickAt(Vector2 normalizedPos)
    {
        TilePos tilePos = new TilePos(
            (int)(normalizedPos.x * width),
            (int)(normalizedPos.y * height)
        );
        netGame.RotateAt(tilePos);
        updateTilemap();
    }

    private void updateTilemap()
    {
        netGameCamera.orthographicSize = Mathf.Max(width, height) * serverSprite.bounds.size.x / 2.0f;
        tilemap.tileAnchor = new Vector3(
            width % 2 == 0 ? 0.5f : 0.0f,
            height % 2 == 0 ? 0.5f : 0.0f,
            0.0f);

        var tilePositions = new List<Vector3Int>(width * height);
        var tiles = new List<UnityEngine.Tilemaps.Tile>(width * height);
        tilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                var gameTile = netGame.GetTileAt(new TilePos(x, y));
                if (wireSprites.ContainsKey(gameTile.Connections))
                {
                    var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                    tile.sprite = wireSprites[gameTile.Connections];
                    tile.color = gameTile.Status == TileStatus.Powered
                        ? colorByPowerId[gameTile.PowerId]
                        : colorByTileStatus[gameTile.Status];
                    tiles.Add(tile);
                    tilePositions.Add(new Vector3Int(x, y, 0));
                }
                if (gameTile.Type != TileType.Wire)
                {
                    var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                    tile.sprite = gameTile.Type == TileType.Server
                        ? serverSprite
                        : terminalSprite;
                    tiles.Add(tile);
                    tilePositions.Add(new Vector3Int(x, y, 1));
                }
            }
        }

        Vector3Int centeringOffset = new Vector3Int(-width / 2, -height / 2, 0);
        tilemap.SetTiles(
            tilePositions.Select(pos => pos + centeringOffset).ToArray(),
            tiles.ToArray()
        );
    }

    private static readonly string spritePrefix = "netgame-tiles-";
    private static readonly IReadOnlyDictionary<Directions, string> spriteNames = new Dictionary<Directions, string>()
    {
        { Directions.Up , "wireU" },
        { Directions.Down , "wireD" },
        { Directions.Left , "wireL" },
        { Directions.Right , "wireR" },
        { Directions.Down | Directions.Up, "wireVer" },
        { Directions.Left | Directions.Right, "wireHor" },
        { Directions.Up | Directions.Right, "wireUR" },
        { Directions.Up | Directions.Left, "wireUL" },
        { Directions.Down | Directions.Right, "wireDR" },
        { Directions.Down | Directions.Left, "wireDL" },
        { Directions.All.Remove(Direction.Up), "wireHorD" },
        { Directions.All.Remove(Direction.Down), "wireHorU" },
        { Directions.All.Remove(Direction.Left), "wireVerR" },
        { Directions.All.Remove(Direction.Right), "wireVerL" },
    };
    private void createSpriteMap()
    {
        serverSprite = findSprite("server");
        terminalSprite = findSprite("terminal");
        wireSprites = spriteNames
            .Select(p => (dirs: p.Key, sprite: findSprite(p.Value)))
            .ToDictionary(p => p.dirs, p => p.sprite);
    }
    private Sprite findSprite(string name)
    {
        Sprite sprite = spriteAtlas.GetSprite(spritePrefix + name);
        if (sprite == null)
            throw new Exception("Could not find sprite " + name);
        return sprite;
    }
}
