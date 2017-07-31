using UnityEngine;

public class Cell
{
    public CellType Type;
    public Position Position;
    public Vector3 WorldPosition;

    public Cell()
    {
    }

    public Cell(CellType type, Position position, Vector3 worldPosition)
    {
        Type = type;
        Position = position;
        WorldPosition = worldPosition;
    }
}
