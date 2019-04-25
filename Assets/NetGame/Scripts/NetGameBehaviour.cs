using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;
using NetGame;
using UnityEngine.Analytics;

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

    public event Action GameWasCompleted = ()=>
        {
            Debug.Log("Game was completed");
            AnalyticsEvent.Custom("NetgameCompleted", new Dictionary<string, object>
            {
                { "name", "Netgame completed"}
            });
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
    private TilemapRenderer tilemapRenderer;
    private Camera netGameCamera;
    private NetGame.NetGame netGame;
    private TMPro.TextMeshProUGUI uiText;

    private string textTemplate;

    private class RotatingTile
    {
        public TilePos pos;
        public float value = 0.0f; // current rotation value
        public float target = 0.0f; // target rotation value
        public int count = 0; // count of rotations to be applied

        public RotatingTile(TilePos rotateTilePos)
        {
            pos = rotateTilePos;
        }
    }
    private List<RotatingTile> rotatingTiles = new List<RotatingTile>();

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        netGameCamera = GetComponent<Camera>();
        uiText = transform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        textTemplate = uiText.text;
        setText("Needs part", "", -1);
        tilemapRenderer.enabled = false;
        createSpriteMap();
        GenerateNewPuzzle();
        ResetPuzzle();
    }

    private void setText(string errorMessage, string successMessage, int progressBar)
    {
        string progressBarString = "";
        if (progressBar >= 0)
            progressBarString = "[" + new string('x', progressBar) + new string(' ', 10 - progressBar) + "]";
        uiText.text = textTemplate
            .Replace("$ERROR_MSG", errorMessage)
            .Replace("$SUCCESS_MSG", successMessage)
            .Replace("$PROGRESS_BAR", progressBarString);
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
        var shouldUpdate = false;

        var finishedTiles = new List<RotatingTile>();
        foreach (var rt in rotatingTiles)
        {
            rt.value = iTween.FloatUpdate(rt.value, rt.target, 15.0f);
            setTileZRotation(
                centeringOffset + new Vector3Int(rt.pos.X, rt.pos.Y, 0),
                Vector3.forward * rt.value
            );

            if (Mathf.Abs(rt.value - rt.target) < 5.0f)
            {
                finishedTiles.Add(rt);
                for (int i = 0; i < rt.count; i++)
                    netGame.RotateAt(rt.pos);
                shouldUpdate = true;
            }
        }
        rotatingTiles = rotatingTiles.Except(finishedTiles).ToList();

        if (shouldUpdate)
        {
            updateTilemap();
            if (netGame.Completed)
                StartCoroutine(finishingAnimation());
        }
    }

    private IEnumerator finishingAnimation()
    {
        tilemapRenderer.enabled = false;
        uiText.enabled = true;
        setText("Compiling...", "", 0);
        yield return new WaitForSeconds(0.1f);
        for (int i = 1; i <= 10; i++)
        {
            setText("Compiling...", "", i);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.5f);
        uiText.text = "Finished upgrade!";
        GameWasCompleted();
    }

    public void OnPickedUpPart()
    {
        setText("User interaction required", "", -1);
    }

    public void StartGame()
    {
        uiText.enabled = false;
        tilemapRenderer.enabled = true;
    }

    public void GenerateNewPuzzle()
    {
        Analytics.CustomEvent("NetgameCreated", new Dictionary<string, object>
        {
            { "name", "Netgame created"}
        });

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
        if (netGame.Completed)
            return;

        TilePos rotateTilePos = new TilePos(
            (int)(normalizedPos.x * width),
            (int)(normalizedPos.y * height)
        );
        var previousTileSet = rotatingTiles.Where(rt => rt.pos == rotateTilePos);

        RotatingTile rotateTile;
        if (previousTileSet.Any())
            rotateTile = previousTileSet.Single();
        else
            rotatingTiles.Add(rotateTile = new RotatingTile(rotateTilePos));

        rotateTile.count++;
        rotateTile.target -= 90.0f;
        if (rotateTile.target <= -360.0f)
            rotateTile.target += 360.0f;
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
