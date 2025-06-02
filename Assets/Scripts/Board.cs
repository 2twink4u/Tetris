using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public Tile bonusTile;
    public float slowMotionFactor = 0.5f;
    public float slowMotionDuration = 5f;

    [Header("Score System")]
    public Text scoreText;
    public GameObject gameOverPanel;

    private int score = 0;
    public bool isGameOver { get; private set; } = false;
    private bool isSlowMotion = false;
    private float slowMotionEndTime = 0f;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        UpdateScore(0);
        SpawnPiece();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isSlowMotion && Time.time > slowMotionEndTime)
        {
            Time.timeScale = 1f;
            isSlowMotion = false;
        }

        // Тестовый рестарт по клавише R
    if (Input.GetKeyDown(KeyCode.R))
    {
        Debug.Log("Restart triggered by keyboard");
        RestartGame();
    }
    
    
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random].Clone();
        data.Initialize();

        if (Random.value < 0.15f)
        {
            data.isBonus = true;
            data.bonusType = (BonusType)Random.Range(1, 5);
            data.tile = bonusTile;
        }

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }


    }

    private void ActivateBonus(BonusType bonusType, Vector3Int position)
    {
        int bonusPoints = 0;

        switch (bonusType)
        {
            case BonusType.LineClear:
                ClearRandomLine();
                bonusPoints = 200;
                break;

            case BonusType.ColumnClear:
                ClearRandomColumn();
                bonusPoints = 200;
                break;

            case BonusType.Bomb:
                ExplodeAround(position);
                bonusPoints = 150;
                break;

            case BonusType.SlowMotion:
                StartSlowMotion();
                bonusPoints = 100;
                break;
        }

        UpdateScore(bonusPoints);
        Debug.Log($"Activated bonus: {bonusType} - adding {bonusPoints} points");
        UpdateScore(bonusPoints);
    }

    private void ClearRandomLine()
    {
        int row = Random.Range(Bounds.yMin, Bounds.yMax);
        LineClear(row);
    }

    private void ClearRandomColumn()
    {
        RectInt bounds = Bounds;
        int col = Random.Range(bounds.xMin, bounds.xMax);

        for (int row = bounds.yMin; row < bounds.yMax; row++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }
    }

    private void ExplodeAround(Vector3Int position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int explosionPos = position + new Vector3Int(x, y, 0);
                tilemap.SetTile(explosionPos, null);
            }
        }
    }

    public void Lock(Piece piece)
    {
        Set(piece);

        foreach (Vector3Int cell in piece.cells)
        {
            Vector3Int position = cell + piece.position;
            // Исправьте условие проверки Game Over
            if (position.y >= Bounds.yMax - 1)
            {
                Debug.Log("Game Over triggered by piece position!");
                GameOver();
                return;
            }
        }

        ClearLines();

        if (piece.data.isBonus)
        {
            ActivateBonus(piece.data.bonusType, piece.position);
        }

        if (!isGameOver)
        {
            SpawnPiece();
        }
    }

    private void StartSlowMotion()
    {
        Time.timeScale = slowMotionFactor;
        slowMotionEndTime = Time.time + slowMotionDuration;
        isSlowMotion = true;
    }

    public void GameOver()
    {
        isGameOver = true;
        tilemap.ClearAllTiles();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (activePiece != null)
        {
            activePiece.enabled = false;
        }
    }

    public void RestartGame()
    {
        // Сброс состояния
        isGameOver = false;
        score = 0;
        UpdateScore(0);
        tilemap.ClearAllTiles();

        // Сброс времени
        Time.timeScale = 1f;
        isSlowMotion = false;

        // Скрытие панели (критически важно!)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Перезапуск фигуры
        if (activePiece == null)
        {
            activePiece = new GameObject("ActivePiece").AddComponent<Piece>();
            activePiece.transform.SetParent(transform);
        }

        activePiece.gameObject.SetActive(true);
        activePiece.enabled = true;

        SpawnPiece();
    }

    public void UpdateScore(int points)
    {
        score += points;
        Debug.Log($"Updating score: {score} (+{points})");

        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }
        else
        {
            Debug.LogError("scoreText is null!");
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }

        if (linesCleared > 0)
        {
            int points = 0;
            switch (linesCleared)
            {
                case 1: points = 10; break;
                case 2: points = 30; break;
                case 3: points = 50; break;
                case 4: points = 80; break;
            }
            Debug.Log($"Cleared {linesCleared} lines! Adding {points} points");
            UpdateScore(points);
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}