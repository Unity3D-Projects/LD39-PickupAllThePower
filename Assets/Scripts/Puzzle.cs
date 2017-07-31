using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    public int Level;
    public Position Position;
    public Vector3 WorldPosition;
    public int PickupCount;
    public Position? UpDoor, DownDoor, LeftDoor, RightDoor;

    public Puzzle(int level, int r, int c, float x, float y, float z, int pickupCount)
        : this(level, new Position(r, c), new Vector3(x, y, z), pickupCount)
    {
    }

    public Puzzle(int level, Position position, Vector3 worldPosition, int pickupCount)
    {
        Level = level;
        Position = position;
        WorldPosition = worldPosition;
        PickupCount = pickupCount;
    }

    public override string ToString()
    {
        return string.Format("Level: {0}, Position: {1}, WorldPosition: {2}", Level, Position, WorldPosition);
    }
}