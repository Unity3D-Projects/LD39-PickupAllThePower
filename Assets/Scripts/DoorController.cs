using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Sprite OpenedSprite;
    public Sprite ClosedSprite;
    public Sprite SealedSprite;

    public DoorState State;
    public DoorType Type;

    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        switch (State)
        {
            case DoorState.Opened:
            {
                _renderer.sprite = OpenedSprite;
                break;
            }
            case DoorState.Closed:
            {
                _renderer.sprite = ClosedSprite;
                break;
            }
            case DoorState.Sealed:
            {
                _renderer.sprite = SealedSprite;
                break;
            }
        }
    }
}

public enum DoorState
{
    Opened,
    Closed,
    Sealed
}

public enum DoorType
{
    Horizontal,
    Vertical
}

