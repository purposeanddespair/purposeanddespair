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

    private Vector3Int centeringOffset => new Vector3Int(-width / 2, -height / 2, 0);
    private Sprite terminalSprite, serverSprite;
    private IReadOnlyDictionary<Directions, Sprite> wireSprites;
    private Tilemap tilemap;
    private Camera netGameCamera;
    private NetGame.NetGame netGame;

    private UnityEngine.Tilemaps.Tile currentRotatingTile = null;
    private float currentRotatingTileValue;
    private TilePos currentRotatingTilePos;
    private Vector3Int currentRotatingTileMapPos => new Vector3Int(
        currentRotatingTilePos.X,
        currentRotatingTilePos.Y,
        0) + centeringOffset;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        netGameCamera = GetComponent<Camera>();
        createSpriteMap();

        GenerateNewPuzzle();
    }

    private void setTileZRotation(Vector3Int tilePos, Vector3 euler)
    {
        var tile = tilemap.GetTile<UnityEngine.Tilemaps.Tile>(tilePos);
        if (tile == null)
            return;
        tile.flags = TileFlags.LockTransform;
        tile.transform = Matrix4x4.Rotate(Quaternion.Euler(euler));
        tilemap.RefreshTile(tilePos);
    }

    void Update()
    {
        if (currentRotatingTile != null)
        {
            currentRotatingTileValue = iTween.FloatUpdate(currentRotatingTileValue, -90.0f, 10.0f);
            Vector3 euler = currentRotatingTileValue * Vector3.forward;
            setTileZRotation(currentRotatingTileMapPos, euler);
            //setTileZRotation(currentRotatingTileMapPos + new Vector3Int(0, 0, 1), euler);

            if (Mathf.Abs(currentRotatingTileValue - (-90.0f)) < 5.0f)
            {
                currentRotatingTile = null;
                netGame.RotateAt(currentRotatingTilePos);
                updateTilemap();
            }
        }
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
        if (currentRotatingTile != null)
            return;

        currentRotatingTilePos = new TilePos(
            (int)(normalizedPos.x * width),
            (int)(normalizedPos.y * height)
        );
        currentRotatingTile = tilemap.GetTile<UnityEngine.Tilemaps.Tile>(currentRotatingTileMapPos);
        currentRotatingTileValue = 0.0f;
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
