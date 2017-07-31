using UnityEngine;

public class TurnController : MonoBehaviour
{
    public TurnDirection TurnDirection;
    public Sprite Turn00Sprite;
    public Sprite Turn01Sprite;
    public Sprite Turn10Sprite;
    public Sprite Turn11Sprite;

    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        switch (TurnDirection)
        {
            case TurnDirection.Turn00:
            {
                _renderer.sprite = Turn00Sprite;
                break;
            }
            case TurnDirection.Turn01:
            {
                _renderer.sprite = Turn01Sprite;
                break;
            }
            case TurnDirection.Turn10:
            {
                _renderer.sprite = Turn10Sprite;
                break;
            }
            case TurnDirection.Turn11:
            {
                _renderer.sprite = Turn11Sprite;
                break;
            }
            default:
            {
                _renderer.sprite = null;
                break;
            }
        }
    }

    public void SetTurnDirectionByCellType(CellType type)
    {
        var turnController = GetComponent<TurnController>();
        switch (type)
        {
            case CellType.Turn00:
            {
                turnController.TurnDirection = TurnDirection.Turn00;
                break;
            }
            case CellType.Turn01:
            {
                turnController.TurnDirection = TurnDirection.Turn01;
                break;
            }
            case CellType.Turn10:
            {
                turnController.TurnDirection = TurnDirection.Turn10;
                break;
            }
            case CellType.Turn11:
            {
                turnController.TurnDirection = TurnDirection.Turn11;
                break;
            }
        }
    }
}
