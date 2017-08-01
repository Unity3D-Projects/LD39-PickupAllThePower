using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject PickupPrefab;
    public GameObject TurnPrefab;
    public GameObject DoorPrefab;

    public MusicController Music;
    public SoundsController Sounds;

    public Text WinText;

    public Transform Marker;

    public Cell[,] Cells;
    private Position StartPosition;
    private Position StartDirection;
    public int Rows, Cols;
    public int LevelCount = 2;

    public MapState State;
    public PlayerController Player;

    private Puzzle[] _initialPuzzles;
    private Puzzle[][,] _puzzles;
    private Puzzle _currentPuzzle;
    private Stack<GameState> _gameStates;

    private int[] _sizeByLevel;
    private float[] _cameraSizeByLevel = { 10.5f, 2.5f };
    private Position[] _pickupsInSuperPuzzle =
    {
        new Position(1, 28),
        new Position(3, 32),
        new Position(12, 22),
        new Position(17, 28),
        new Position(14, 32),
    };

    private string _mapFileName = "map.txt";

    void Awake()
    {
        Cells = MapUtils.LoadMap(_mapFileName, out Rows, out Cols, out StartPosition);
        StartDirection = new Position(0, 1);

        _gameStates = new Stack<GameState>();

        _sizeByLevel = new int[LevelCount];
        for (int l = 0; l < LevelCount; l++)
        {
            //
            // These are the sizes of each level (including borders).
            // _levelCount - 1 -> 6
            // _levelCount - 2 -> 21
            // _levelCount - 3 -> 81
            // ...
            //
            // The general formula is: (2 ^ (2 * (lc - l))) + (2 ^ (2 * (lc - l))) / 4 - 1 + 2
            // Which reduces to: 5 * (2 ^ (2 * (lc - l))) / 4 + 1
            // 
            // The explaining for the formula is: 
            // the size of actual floor cells for the levels + 
            // the amount of inner walls - 1 +
            // the two walls of the external border.
            //
            // With this, any center position of any puzzle at any level can be calculated with the formula:
            // x = topLeft.x + (_sizeByLevel[l] - 1) * c + (_sizeByLevel[l] - 1) / 2
            // y = topLeft.y - (_sizeByLevel[l] - 1) * r - (_sizeByLevel[l] - 1) / 2;
            // z = constant
            //
            // In any formula:
            // topLeft: center of the top left cell
            // lc: level count
            // l: level
            // r: row
            // c: column

            _sizeByLevel[l] = (5 * (1 << 2 * (LevelCount - l)) / 4) + 1;
        }

        _initialPuzzles = new Puzzle[4]
        {
            new Puzzle(LevelCount - 1, 0, -4, 3.0f, 18.0f, 0.0f, 1) { RightDoor = new Position(3, 5) },
            new Puzzle(LevelCount - 1, 0, -3, 8.0f, 18.0f, 0.0f, 1)  { LeftDoor = new Position(3, 5), RightDoor = new Position(2, 10) },
            new Puzzle(LevelCount - 1, 0, -2, 13.0f, 18.0f, 0.0f, 2) { LeftDoor = new Position(2, 10), RightDoor = new Position(4, 15) },
            new Puzzle(LevelCount - 1, 0, -1, 18.0f, 18.0f, 0.0f, 2) { LeftDoor = new Position(4, 15), RightDoor = new Position(1, 20) },
        };

        _puzzles = new Puzzle[LevelCount][,];
        for (int l = 0; l < LevelCount; l++)
        {
            var rows = 4 * l;
            var cols = 4 * l;

            if (l == 0)
            {
                rows = cols = 1;
            }

            _puzzles[l] = new Puzzle[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var x = 20.5f + (_sizeByLevel[l] - 1) * c + (_sizeByLevel[l] - 1) * 0.5f;
                    var y = 20.5f - (_sizeByLevel[l] - 1) * r - (_sizeByLevel[l] - 1) * 0.5f;

                    var puzzle = new Puzzle(l, r, c, x, y, 0.0f, 0);
                    if (l == LevelCount - 1)
                    {
                        var firstCellPos = GetFirstCellOfPuzzle(puzzle);

                        for (int pr = 0; pr < 6; pr++)
                        {
                            for (int pc = 0; pc < 6; pc++)
                            {
                                var cell = Cells[firstCellPos.r - 1 + pr, firstCellPos.c - 1 + pc];
                                if (cell.Type == CellType.Pickup)
                                {
                                    puzzle.PickupCount++;
                                }
                                else if (cell.Type == CellType.Door)
                                {
                                    if (pr == 0)
                                    {
                                        puzzle.UpDoor = cell.Position;
                                    }
                                    else if (pr == 5)
                                    {
                                        puzzle.DownDoor = cell.Position;
                                    }
                                    else if (pc == 0)
                                    {
                                        puzzle.LeftDoor = cell.Position;
                                    }
                                    else if (pc == 5)
                                    {
                                        puzzle.RightDoor = cell.Position;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        puzzle.PickupCount = 5;
                    }

                    _puzzles[l][r, c] = puzzle;
                }
            }
        }
    }

    void Start()
    {
        SetCurrentPuzzle(1, 0, -4);

        Player.Position = StartPosition;
        Player.Direction = StartDirection;
        Player.UpdateDirection();
        Player.Rows = Rows;
        Player.Cols = Cols;
        Player.Stop();

        SaveState();

        CreateLayer("Turns");

        State = MapState.Editing;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        switch (State)
        {
            case MapState.Editing:
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestoreState();
                    State = MapState.Editing;
                }

                EditUpdate();
                break;
            }

            case MapState.Running:
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestoreState();
                    State = MapState.Editing;
                }

                RunningUpdate();
                break;
            }

            case MapState.Stopped:
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestoreState();
                    State = MapState.Editing;
                }

                break;
            }

            case MapState.Preview:
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    var puzzle = _currentPuzzle;

                    var position = Camera.main.transform.position;
                    var newPosition = new Vector3(
                        puzzle.WorldPosition.x,
                        puzzle.WorldPosition.y,
                        -10.0f);

                    var size = Camera.main.orthographicSize;
                    var newSize = _cameraSizeByLevel[1];

                    Tweener.Instance.AddTween(
                        Camera.main,
                        0.5f,
                        0,
                        1,
                        (param, t) =>
                        {
                            var camera = (Camera)param;
                            camera.transform.position = Vector3.Lerp(position, newPosition, t);
                            camera.orthographicSize = Mathf.Lerp(size, newSize, t);
                        },
                        TweenEffectsFactory.LinearEffect,
                        (param) =>
                        {
                            var pickupsLayer = GetLayer("Pickups");
                            for (int i = 0; i < pickupsLayer.transform.childCount; i++)
                            {
                                var pickup = pickupsLayer.transform.GetChild(i);
                                var pickupController = pickup.GetComponent<PickupController>();
                                pickupController.Visible = true;
                                pickupController.PreviewInSuperPuzzle = false;
                            }

                            var markerRenderer = Marker.GetComponent<SpriteRenderer>();
                            markerRenderer.enabled = true;
                        });

                    State = MapState.Editing;
                }

                break;
            }
        }
    }

    private void EditUpdate()
    {
        if (_currentPuzzle != null && _currentPuzzle.Level == LevelCount - 1)
        {
            var mousePosition = Input.mousePosition;
            var gridPlane = new Plane(Vector3.back, Vector3.zero);
            var ray = Camera.main.ScreenPointToRay(mousePosition);

            float distance;
            if (gridPlane.Raycast(ray, out distance))
            {
                var point = ray.GetPoint(distance);
                var position = GetCellPositionFromPoint(point);
                if (InsideCurrentPuzzle(position.r, position.c))
                {
                    var cell = Cells[position.r, position.c];
                    if (cell != null)
                    {
                        Marker.GetComponent<SpriteRenderer>().enabled = true;
                        Marker.position = cell.WorldPosition;
                    }
                    else
                    {
                        Marker.GetComponent<SpriteRenderer>().enabled = false;
                        Marker.position = Vector3.zero;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (cell.Type == CellType.Empty || (cell.Type >= CellType.Turn00 && cell.Type <= CellType.Turn11))
                        {
                            switch (cell.Type)
                            {
                                case CellType.Empty:
                                {
                                    cell.Type = CellType.Turn00;
                                    break;
                                }
                                case CellType.Turn00:
                                {
                                    cell.Type = CellType.Turn01;
                                    break;
                                }
                                case CellType.Turn01:
                                {
                                    cell.Type = CellType.Turn11;
                                    break;
                                }
                                case CellType.Turn10:
                                {
                                    cell.Type = CellType.Turn00;
                                    break;
                                }
                                case CellType.Turn11:
                                {
                                    cell.Type = CellType.Turn10;
                                    break;
                                }
                            }

                            var turnGameObj = CreateTurnIfNotExists(cell);
                            var turnController = turnGameObj.GetComponent<TurnController>();
                            turnController.SetTurnDirectionByCellType(cell.Type);
                        }
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        if (cell.Type >= CellType.Turn00 && cell.Type <= CellType.Turn11)
                        {
                            cell.Type = CellType.Empty;
                            DestroyTurn(cell);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.PrevPosition = Player.Position;

                SaveState();
                State = MapState.Running;
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                // don't preview in the initial puzzles
                if (_currentPuzzle.Position.c >= 0)
                {
                    PreviewSuperPuzzle();
                }
            }
        }
    }

    private void RunningUpdate()
    {
        if (!Player.Walking)
        {
            var position = Player.Position;
            var direction = Player.Direction;

            if (!PerformActionForCurrentCell(position, direction))
            {
                Player.Stop();

                var cell = Cells[position.r, position.c];
                if (cell.Type == CellType.Door)
                {
                    var fromPosition = Player.PrevPosition;
                    var toPosition = Player.Position;

                    var fromCell = Cells[fromPosition.r, fromPosition.c];
                    var toCell = Cells[toPosition.r, toPosition.c];

                    var fromDoor = GetDoor(fromCell);

                    // in the first puzzle the player doesn't start in a door, so
                    // check for null to include this case in the general case.
                    if (fromDoor != null)
                    {
                        var toDoor = GetDoor(toCell);

                        var fromDoorController = fromDoor.GetComponent<DoorController>();
                        var fromDoorType = fromDoorController.Type;

                        var toDoorController = toDoor.GetComponent<DoorController>();
                        var toDoorType = toDoorController.Type;

                        // if is the first (not the initials) puzzle, also seal all the doors
                        if (fromDoorType != toDoorType || _currentPuzzle.Position == Position.Zero)
                        {
                            if (_currentPuzzle.UpDoor != null)
                            {
                                var doorPosition = _currentPuzzle.UpDoor.Value;
                                if (doorPosition != fromPosition && doorPosition != toPosition)
                                {
                                    ChangeDoorState(doorPosition, DoorState.Sealed);
                                }
                            }

                            if (_currentPuzzle.DownDoor != null)
                            {
                                var doorPosition = _currentPuzzle.DownDoor.Value;
                                if (doorPosition != fromPosition && doorPosition != toPosition)
                                {
                                    ChangeDoorState(doorPosition, DoorState.Sealed);
                                }
                            }


                            if (_currentPuzzle.LeftDoor != null)
                            {
                                var doorPosition = _currentPuzzle.LeftDoor.Value;
                                if (doorPosition != fromPosition && doorPosition != toPosition)
                                {
                                    ChangeDoorState(doorPosition, DoorState.Sealed);
                                }
                            }

                            if (_currentPuzzle.RightDoor != null)
                            {
                                var doorPosition = _currentPuzzle.RightDoor.Value;
                                if (doorPosition != fromPosition && doorPosition != toPosition)
                                {
                                    ChangeDoorState(doorPosition, DoorState.Sealed);
                                }
                            }
                        }
                        else
                        {
                            var doorPosition = Player.Position;
                            ChangeDoorState(doorPosition, DoorState.Sealed);
                        }
                    }
                    else
                    {
                        var doorPosition = Player.Position;
                        ChangeDoorState(doorPosition, DoorState.Sealed);
                    }

                    var newPuzzle = GetPuzzle(position, direction);
                    SetCurrentPuzzle(newPuzzle.Level, newPuzzle.Position.r, newPuzzle.Position.c);
                    ChangeCurrentDoorsStates(DoorState.Closed, true);

                    if (newPuzzle.Position == Position.Zero)
                    {
                        // if the next puzzle is the first one for real, clear the game states
                        _gameStates.Clear();

                        // hide the initial puzzles
                        HideInitialPuzzles();

                        // create a new game state
                        SaveState();

                        // and preview the super puzzle
                        PreviewSuperPuzzle();
                    }
                    else
                    {
                        State = MapState.Editing;
                    }
                }
                else if (cell.Type == CellType.Finish)
                {
                    PreviewSuperPuzzle();

                    WinText.enabled = true;

                    State = MapState.Won;
                }
                else
                {
                    State = MapState.Stopped;
                }

                return;
            }

            position = Player.Position;
            direction = Player.Direction;

            if (!PerformActionForNextCell(position, direction))
            {
                Player.Stop();
                State = MapState.Stopped;
                return;
            }
        }

        Player.UpdatePosition();
    }

    private bool PerformActionForCurrentCell(Position position, Position direction)
    {
        var cell = Cells[position.r, position.c];

        switch (cell.Type)
        {
            case CellType.Finish:
            {
                // won the game!!
                return false;
            }

            case CellType.Door:
            {
                var doorGameObj = GetDoor(cell);
                var doorController = doorGameObj.GetComponent<DoorController>();
                if (doorController.State == DoorState.Opened)
                {
                    return false;
                }

                break;
            }

            case CellType.Pickup:
            {
                cell.Type = CellType.Empty;
                _currentPuzzle.PickupCount--;

                for (int i = 0; i < _pickupsInSuperPuzzle.Length; i++)
                {
                    if (_pickupsInSuperPuzzle[i] == cell.Position)
                    {
                        var superPuzzle = _puzzles[0][0, 0];
                        superPuzzle.PickupCount--;
                        break;
                    }
                }

                if (_currentPuzzle.PickupCount == 0)
                {
                    ChangeCurrentDoorsStates(DoorState.Opened, true);

                    Sounds.Play(SoundNames.Doors);
                }
                else
                {
                    Sounds.Play(SoundNames.Pickup);
                }

                DestroyPickup(cell);
                break;
            }

            case CellType.Turn00:
            {
                if (direction == Position.Right)
                {
                    Player.TurnLeft();
                    Player.UpdateDirection();
                }
                else if (direction == Position.Down)
                {
                    Player.TurnRight();
                    Player.UpdateDirection();
                }

                Sounds.Play(SoundNames.Turn);

                break;
            }

            case CellType.Turn01:
            {
                if (direction == Position.Left)
                {
                    Player.TurnRight();
                    Player.UpdateDirection();
                }
                else if (direction == Position.Down)
                {
                    Player.TurnLeft();
                    Player.UpdateDirection();
                }

                Sounds.Play(SoundNames.Turn);

                break;
            }
            case CellType.Turn10:
            {
                if (direction == Position.Right)
                {
                    Player.TurnRight();
                    Player.UpdateDirection();
                }
                else if (direction == Position.Up)
                {
                    Player.TurnLeft();
                    Player.UpdateDirection();
                }

                Sounds.Play(SoundNames.Turn);

                break;
            }

            case CellType.Turn11:
            {
                if (direction == Position.Left)
                {
                    Player.TurnLeft();
                    Player.UpdateDirection();
                }
                else if (direction == Position.Up)
                {
                    Player.TurnRight();
                    Player.UpdateDirection();
                }

                Sounds.Play(SoundNames.Turn);

                break;
            }
        }

        return true;
    }

    private bool PerformActionForNextCell(Position position, Position direction)
    {
        var newPosition = position + direction;
        var newCell = Cells[newPosition.r, newPosition.c];

        switch (newCell.Type)
        {
            case CellType.Empty:
            {
                Player.GoForward();
                break;
            }

            case CellType.Start:
            {
                // If is other puzzle except the first, the start cell is forbidden
                if (_currentPuzzle.Position.r != 0 || _currentPuzzle.Position.c != -4)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }

            case CellType.Finish:
            {
                var superPuzzle = _puzzles[0][0, 0];
                if (superPuzzle.PickupCount > 0)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }

            case CellType.Door:
            {
                var doorGameObj = GetDoor(newCell);
                var doorController = doorGameObj.GetComponent<DoorController>();
                if (doorController.State == DoorState.Closed || doorController.State == DoorState.Sealed)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }

            case CellType.Block:
            {
                return false;
            }

            case CellType.Pickup:
            {
                Player.GoForward();
                break;
            }

            case CellType.Turn00:
            {
                if (direction != Position.Right && direction != Position.Down)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }

            case CellType.Turn01:
            {
                if (direction != Position.Left && direction != Position.Down)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }
            case CellType.Turn10:
            {
                if (direction == Position.Right && direction == Position.Up)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }

            case CellType.Turn11:
            {
                if (direction != Position.Left && direction != Position.Up)
                {
                    return false;
                }

                Player.GoForward();
                break;
            }
        }

        return true;
    }

    private void HideInitialPuzzles()
    {
        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 20; c++)
            {
                var cell = Cells[r, c];

                var floor = GetFloor(cell);
                if (floor != null)
                {
                    GameObject.Destroy(floor);
                }

                var wall = GetWall(cell);
                if (wall != null)
                {
                    GameObject.Destroy(wall);
                }

                var door = GetDoor(cell);
                if (door != null)
                {
                    GameObject.Destroy(door);
                }

                var turn = GetTurn(cell);
                if (turn != null)
                {
                    GameObject.Destroy(turn);
                }

                cell.Type = CellType.Empty;
            }
        }
    }

    private void PreviewSuperPuzzle()
    {
        var pickupsLayer = GetLayer("Pickups");
        for (int i = 0; i < pickupsLayer.transform.childCount; i++)
        {
            var pickup = pickupsLayer.transform.GetChild(i);
            var pickupController = pickup.GetComponent<PickupController>();
            pickupController.Visible = false;
        }

        var markerRenderer = Marker.GetComponent<SpriteRenderer>();
        markerRenderer.enabled = false;

        for (int i = 0; i < _pickupsInSuperPuzzle.Length; i++)
        {
            var pickupPosition = _pickupsInSuperPuzzle[i];
            var cell = Cells[pickupPosition.r, pickupPosition.c];
            var pickup = GetPickup(cell);
            if (pickup != null)
            {
                var pickupController = pickup.GetComponent<PickupController>();
                pickupController.Visible = true;
                pickupController.PreviewInSuperPuzzle = true;
            }
        }

        var puzzle = _puzzles[0][0, 0];

        var position = Camera.main.transform.position;
        var newPosition = new Vector3(
            puzzle.WorldPosition.x,
            puzzle.WorldPosition.y,
            -10.0f);

        var size = Camera.main.orthographicSize;
        var newSize = _cameraSizeByLevel[0];

        Tweener.Instance.AddTween(
            Camera.main,
            1.0f,
            0,
            1,
            (param, t) =>
            {
                var camera = (Camera)param;
                camera.transform.position = Vector3.Lerp(position, newPosition, t);
                camera.orthographicSize = Mathf.Lerp(size, newSize, t);
            }, TweenEffectsFactory.LinearEffect);

        State = MapState.Preview;
    }

    private void SaveState()
    {
        var state = new GameState
        {
            PlayerPosition = Player.Position,
            PlayerDirection = Player.Direction,
            PuzzleLevel = _currentPuzzle.Level,
            PuzzlePosition = _currentPuzzle.Position
        };

        state.Cells = new CellState[Rows, Cols];
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                var cell = Cells[r, c];

                var cellState = new CellState();
                cellState.Type = cell.Type;

                switch (cell.Type)
                {
                    case CellType.Door:
                    {
                        var doorGameObj = GetDoor(cell);
                        var doorController = doorGameObj.GetComponent<DoorController>();
                        cellState.DoorState = doorController.State;
                        cellState.DoorType = doorController.Type;

                        break;
                    }
                }

                state.Cells[r, c] = cellState;
            }
        }

        state.InitialPickupCount = new int[_initialPuzzles.Length];
        for (int i = 0; i < _initialPuzzles.Length; i++)
        {
            state.InitialPickupCount[i] = _initialPuzzles[i].PickupCount;
        }

        state.PickupCount = new int[_puzzles.Length][,];
        for (int l = 0; l < _puzzles.Length; l++)
        {
            state.PickupCount[l] = new int[_puzzles[l].GetLength(0), _puzzles[l].GetLength(1)];

            for (int i = 0; i < _puzzles[l].GetLength(0); i++)
            {
                for (int j = 0; j < _puzzles[l].GetLength(1); j++)
                {
                    state.PickupCount[l][i, j] = _puzzles[l][i, j].PickupCount;
                }
            }
        }

        _gameStates.Push(state);
    }

    private void RestoreState()
    {
        if (_gameStates.Count > 0)
        {
            // don't pop the first state of the game
            var state = _gameStates.Count > 1 ? _gameStates.Pop() : _gameStates.Peek();

            Player.Position = state.PlayerPosition;
            Player.Direction = state.PlayerDirection;
            Player.UpdateDirection();
            Player.Rows = Rows;
            Player.Cols = Cols;
            Player.Stop();

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var cell = Cells[r, c];
                    var cellState = state.Cells[r, c];
                    cell.Type = cellState.Type;

                    if (cell.Type == CellType.Pickup)
                    {
                        CreatePickupIfNotExists(cell);
                    }
                    else if (cell.Type >= CellType.Turn00 && cell.Type <= CellType.Turn11)
                    {
                        var turnGameObj = CreateTurnIfNotExists(cell);
                        var turnController = turnGameObj.GetComponent<TurnController>();
                        turnController.SetTurnDirectionByCellType(cell.Type);
                    }
                    else if (cell.Type == CellType.Door)
                    {
                        var doorGameObj = CreateDoorIfNotExists(cell, cellState.DoorState, cellState.DoorType);
                        var doorController = doorGameObj.GetComponent<DoorController>();
                        doorController.State = cellState.DoorState;
                        doorController.Type = cellState.DoorType;
                    }
                    else if (cell.Type == CellType.Empty)
                    {
                        var turnGameObj = GetTurn(cell);
                        if (turnGameObj != null)
                        {
                            GameObject.Destroy(turnGameObj);
                        }
                    }
                }
            }

            SetCurrentPuzzle(state.PuzzleLevel, state.PuzzlePosition.r, state.PuzzlePosition.c);

            for (int i = 0; i < _initialPuzzles.Length; i++)
            {
                _initialPuzzles[i].PickupCount = state.InitialPickupCount[i];
            }

            for (int l = 0; l < state.PickupCount.Length; l++)
            {
                for (int i = 0; i < state.PickupCount[l].GetLength(0); i++)
                {
                    for (int j = 0; j < state.PickupCount[l].GetLength(1); j++)
                    {
                        var puzzle = _puzzles[l][i, j];
                        puzzle.PickupCount = state.PickupCount[l][i, j];
                    }
                }
            }
        }
    }

    public GameObject GetLayer(string name)
    {
        return GameObject.Find(name);
    }

    public GameObject CreateLayer(string name)
    {
        var floorLayer = new GameObject(name);
        floorLayer.transform.position = Vector3.zero;
        floorLayer.transform.parent = gameObject.transform;
        return floorLayer;
    }

    public GameObject GetFloor(Cell cell)
    {
        var name = string.Format("Floor_{0}_{1}", cell.Position.r, cell.Position.c);
        return GameObject.Find(name);
    }

    public GameObject CreateFloor(Cell cell, GameObject floorLayer = null)
    {
        if (floorLayer == null)
        {
            floorLayer = GameObject.Find("Floor");
        }

        var floor = GameObject.Instantiate(FloorPrefab);
        floor.name = string.Format("{0}_{1}_{2}", "Floor", cell.Position.r, cell.Position.c);
        floor.transform.position = cell.WorldPosition;
        floor.transform.parent = floorLayer.transform;
        return floor;
    }

    public GameObject GetWall(Cell cell)
    {
        var name = string.Format("Wall_{0}_{1}", cell.Position.r, cell.Position.c);
        return GameObject.Find(name);
    }

    public GameObject CreateWall(Cell cell, GameObject wallsLayer = null)
    {
        if (wallsLayer == null)
        {
            wallsLayer = GameObject.Find("Walls");
        }

        var wall = GameObject.Instantiate(WallPrefab);
        wall.name = string.Format("{0}_{1}_{2}", "Wall", cell.Position.r, cell.Position.c);
        wall.transform.position = cell.WorldPosition;
        wall.transform.parent = wallsLayer.transform;
        return wall;
    }

    public GameObject GetPickup(Cell cell)
    {
        var name = string.Format("Pickup_{0}_{1}", cell.Position.r, cell.Position.c);
        return GameObject.Find(name);
    }

    public GameObject CreatePickup(Cell cell, GameObject pickupsLayer = null)
    {
        if (pickupsLayer == null)
        {
            pickupsLayer = GameObject.Find("Pickups");
        }

        var pickup = GameObject.Instantiate(PickupPrefab);
        pickup.name = string.Format("{0}_{1}_{2}", "Pickup", cell.Position.r, cell.Position.c);
        pickup.transform.position = cell.WorldPosition;
        pickup.transform.parent = pickupsLayer.transform;
        return pickup;
    }

    public GameObject CreatePickupIfNotExists(Cell cell, GameObject pickupsLayer = null)
    {
        return GetPickup(cell) ?? CreatePickup(cell, pickupsLayer);
    }

    public void DestroyPickup(Cell cell)
    {
        var name = string.Format("Pickup_{0}_{1}", cell.Position.r, cell.Position.c);
        var pickupGameObj = GameObject.Find(name);
        if (pickupGameObj != null)
        {
            GameObject.Destroy(pickupGameObj);
        }
    }

    public GameObject GetTurn(Cell cell)
    {
        var name = string.Format("Turn_{0}_{1}", cell.Position.r, cell.Position.c);
        return GameObject.Find(name);
    }

    public GameObject CreateTurn(Cell cell, GameObject turnsLayer = null)
    {
        if (turnsLayer == null)
        {
            turnsLayer = GameObject.Find("Turns");
        }

        var turnGameObj = GameObject.Instantiate(TurnPrefab);
        turnGameObj.name = string.Format("Turn_{0}_{1}", cell.Position.r, cell.Position.c);
        turnGameObj.transform.position = cell.WorldPosition;
        turnGameObj.transform.parent = turnsLayer.transform;
        return turnGameObj;
    }

    public GameObject CreateTurnIfNotExists(Cell cell, GameObject turnsLayer = null)
    {
        return GetTurn(cell) ?? CreateTurn(cell, turnsLayer);
    }

    public void DestroyTurn(Cell cell)
    {
        var name = string.Format("Turn_{0}_{1}", cell.Position.r, cell.Position.c);
        var turnGameObj = GameObject.Find(name);
        if (turnGameObj != null)
        {
            GameObject.Destroy(turnGameObj);
        }
    }

    public GameObject GetDoor(Cell cell)
    {
        var name = string.Format("Door_{0}_{1}", cell.Position.r, cell.Position.c);
        return GameObject.Find(name);
    }

    public GameObject CreateDoor(Cell cell, DoorState state, DoorType type, GameObject doorsLayer = null)
    {
        if (doorsLayer == null)
        {
            doorsLayer = GameObject.Find("Doors");
        }

        var doorGameObj = GameObject.Instantiate(DoorPrefab);
        doorGameObj.name = string.Format("Door_{0}_{1}", cell.Position.r, cell.Position.c);
        doorGameObj.transform.position = cell.WorldPosition;
        doorGameObj.transform.parent = doorsLayer.transform;

        var doorController = doorGameObj.GetComponent<DoorController>();
        doorController.State = state;
        doorController.Type = type;

        return doorGameObj;
    }

    public GameObject CreateDoorIfNotExists(Cell cell, DoorState state, DoorType type, GameObject doorsLayer = null)
    {
        return GetDoor(cell) ?? CreateDoor(cell, state, type, doorsLayer);
    }

    public void SetCurrentPuzzle(int level, int r, int c)
    {
        if (level == (LevelCount - 1) && r == 0 && c < 0)
        {
            Assert.IsTrue(c >= -4, "Col out of range");

            _currentPuzzle = _initialPuzzles[4 - Mathf.Abs(c)];
        }
        else
        {
            Assert.IsTrue(level >= 0 && level < _puzzles.Length, "Level out of range");
            Assert.IsTrue(r >= 0 && r < _puzzles[level].GetLength(0), "Row out of range");
            Assert.IsTrue(c >= 0 && c < _puzzles[level].GetLength(1), "Col out of range");

            _currentPuzzle = _puzzles[level][r, c];
        }

        Assert.IsNotNull(_currentPuzzle);

        var position = Camera.main.transform.position;
        var newPosition = new Vector3(
            _currentPuzzle.WorldPosition.x,
            _currentPuzzle.WorldPosition.y,
            -10.0f);

        var size = Camera.main.orthographicSize;
        var newSize = _cameraSizeByLevel[level];

        Tweener.Instance.AddTween(
            Camera.main,
            0.5f,
            0,
            1,
            (param, t) =>
            {
                var camera = (Camera)param;
                camera.transform.position = Vector3.Lerp(position, newPosition, t);
                camera.orthographicSize = Mathf.Lerp(size, newSize, t);
            }, TweenEffectsFactory.LinearEffect);
    }

    public void ChangeCurrentDoorsStates(DoorState state, bool ignoreSealed)
    {
        if (_currentPuzzle.UpDoor != null)
        {
            ChangeDoorState(_currentPuzzle.UpDoor.Value, state, ignoreSealed);
        }

        if (_currentPuzzle.DownDoor != null)
        {
            ChangeDoorState(_currentPuzzle.DownDoor.Value, state, ignoreSealed);
        }

        if (_currentPuzzle.LeftDoor != null)
        {
            ChangeDoorState(_currentPuzzle.LeftDoor.Value, state, ignoreSealed);
        }

        if (_currentPuzzle.RightDoor != null)
        {
            ChangeDoorState(_currentPuzzle.RightDoor.Value, state, ignoreSealed);
        }
    }

    public void ChangeDoorState(Position position, DoorState state, bool ignoreSealed = false)
    {
        var cell = Cells[position.r, position.c];
        var doorGameObj = GetDoor(cell);
        var doorController = doorGameObj.GetComponent<DoorController>();
        if (!ignoreSealed || doorController.State != DoorState.Sealed)
        {
            doorController.State = state;
        }
    }

    public Puzzle GetPuzzle(Position position, Position direction)
    {
        var r = position.r;
        var c = position.c;

        // This is for the firsts 4 puzzles
        if (position.c < 20)
        {
            if (position.r > 5)
            {
                return null;
            }

            var index = c / 5;
            var offset = c % 5;

            if (offset == 0 && (direction == Position.Left && index != 0))
            {
                return _initialPuzzles[index - 1];
            }

            return _initialPuzzles[index];
        }

        // not count the initial puzzles
        c -= 20;

        var puzzleRow = r / 5;
        var offsetRow = r % 5;
        if (offsetRow == 0 && (direction == Position.Up && puzzleRow != 0))
        {
            puzzleRow--;
        }

        var puzzleCol = c / 5;
        var offsetCol = c % 5;
        if (offsetCol == 0 && (direction == Position.Left && puzzleCol != 0))
        {
            puzzleCol--;
        }

        // this method assume is always called to know the puzzle in the last level
        return _puzzles[LevelCount - 1][puzzleRow, puzzleCol];
    }

    public Position GetCellPositionFromPoint(Vector3 point)
    {
        return new Position(Rows - 1 - (int)point.y, (int)point.x);
    }

    public bool Inside(int r, int c)
    {
        return r >= 0 && r < Rows && c >= 0 && c < Cols;
    }

    public Position GetFirstCellOfPuzzle(Puzzle puzzle)
    {
        var r = puzzle.Position.r * 4 + puzzle.Position.r + 1;
        var c = (puzzle.Position.c + 4) * 4 + (puzzle.Position.c + 4) + 1;
        return new Position(r, c);
    }

    public bool InsideCurrentPuzzle(int r, int c)
    {
        if (_currentPuzzle == null)
        {
            return false;
        }

        var position = GetFirstCellOfPuzzle(_currentPuzzle);
        return Inside(r, c) && r >= position.r && r < position.r + 4 && c >= position.c && c < position.c + 4;
    }
}

public enum MapState
{
    Editing = 0,
    Running = 1,
    Stopped = 2,
    Preview = 3,
    Won = 4
}

public struct GameState
{
    public Position PlayerPosition;
    public Position PlayerDirection;
    public int PuzzleLevel;
    public Position PuzzlePosition;
    public CellState[,] Cells;
    public int[] InitialPickupCount;
    public int[][,] PickupCount;
}

public struct CellState
{
    public CellType Type;
    public DoorState DoorState;
    public DoorType DoorType;
}

public static class SoundNames
{
    public const string Turn = "turn";
    public const string UI = "ui";
    public const string Doors = "doors";
    public const string Pickup = "pickup";
}

public static class MapUtils
{
    public static Cell[,] LoadMap(string mapFileName, out int rows, out int cols, out Position startPosition)
    {
        var mapFile = Path.Combine(Application.dataPath, mapFileName);
        startPosition = Position.Zero;

        using (var reader = new StreamReader(mapFile))
        {
            rows = reader.ReadNextInt();
            cols = reader.ReadNextInt();
            var cells = new Cell[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var str = reader.ReadNextString();
                    cells[r, c] = FromString(str, r, c, rows, cols);

                    if (cells[r, c].Type == CellType.Start)
                    {
                        startPosition = new Position(r, c);
                    }
                }
            }

            return cells;
        }
    }

    private static Cell FromString(string cellStr, int r, int c, int rows, int cols)
    {
        var cell = new Cell();
        cell.Position = new Position(r, c);
        cell.WorldPosition = new Vector3(c + 0.5f, rows - 0.5f - r, 0.0f);

        switch (cellStr)
        {
            case "S":
            {
                cell.Type = CellType.Start;
                break;
            }
            case "Q":
            {
                cell.Type = CellType.Finish;
                break;
            }
            case "B":
            {
                cell.Type = CellType.Block;
                break;
            }
            case "P":
            {
                cell.Type = CellType.Pickup;
                break;
            }
            case "D":
            {
                cell.Type = CellType.Door;
                break;
            }
            default:
            {
                cell.Type = CellType.Empty;
                break;
            }
        }

        return cell;
    }
}
