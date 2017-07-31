using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor
{
    private MapController _map;

    void Awake()
    {
        _map = (MapController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var mapFileName = "map.txt";

        GUILayout.BeginHorizontal();
        GUILayout.Label("Map file");
        mapFileName = GUILayout.TextField(mapFileName);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Load map"))
        {
            for (int i = _map.transform.childCount - 1; i >= 0; i--)
            {
                var child = _map.transform.GetChild(i);
                GameObject.DestroyImmediate(child.gameObject);
            }

            int rows, cols;
            Position startPos;
            var cells = MapUtils.LoadMap(mapFileName, out rows, out cols, out startPos);

            var floorLayer = _map.CreateLayer("Floor");
            var wallsLayer = _map.CreateLayer("Walls");
            var pickupsLayer = _map.CreateLayer("Pickups");
            var doorsLayer = _map.CreateLayer("Doors");

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = cells[r, c];

                    _map.CreateFloor(cell, floorLayer);

                    switch (cell.Type)
                    {
                        case CellType.Empty:
                        {
                            // nothing more than a floor
                            break;
                        }

                        case CellType.Block:
                        {
                            _map.CreateWall(cell, wallsLayer);
                            break;
                        }

                        case CellType.Pickup:
                        {
                            _map.CreatePickup(cell, pickupsLayer);
                            break;
                        }

                        case CellType.Door:
                        {
                            var doorType = (cell.Position.r % 5 == 0) ? DoorType.Vertical : DoorType.Horizontal;
                            _map.CreateDoor(cell, DoorState.Closed, doorType, doorsLayer);
                            break;
                        }

                        case CellType.Finish:
                        {
                            var doorType = (cell.Position.r % 5 == 0) ? DoorType.Vertical : DoorType.Horizontal;
                            _map.CreateDoor(cell, DoorState.Closed, doorType, doorsLayer);
                            break;
                        }
                    }
                }
            }
        }
    }
}



