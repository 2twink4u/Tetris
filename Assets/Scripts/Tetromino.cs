using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I, J, L, O, S, T, Z
}

public enum BonusType
{
    None,
    LineClear,
    ColumnClear,
    Bomb,
    SlowMotion
}

[System.Serializable]
public class TetrominoData
{
    public Tile tile;
    public Tetromino tetromino;

    public BonusType bonusType;
    public bool isBonus;

    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }

    public TetrominoData Clone()
    {
        return new TetrominoData()
        {
            tile = this.tile,
            tetromino = this.tetromino,
            bonusType = this.bonusType,
            isBonus = this.isBonus,
        };
    }
}