public struct Step
{
    public Action Action { get; set; }
    public TurnDirection TurnDirection { get; set; }
    public Position Position { get; set; }

    public override string ToString()
    {
        return string.Format("Position: {0}, Action: {1}", Position, Action);
    }

    public string ToStringInline(bool minified)
    {
        if (Action == Action.Start)
            return minified ? "S" : "Start";
        if (Action == Action.Finish)
            return minified ? "Q" : "Finish";
        if (Action == Action.Forward)
            return minified ? "F" : "Forward";
        if (Action == Action.TurnLeft)
            return minified ? "L" : "TurnLeft";
        if (Action == Action.TurnRight)
            return minified ? "R" : "TurnRight";
        if (Action == Action.Bounce)
            return minified ? "B" : "Bounce";
        if (Action == Action.TurnLeftAndRemove)
            return minified ? "A" : "TurnLeftAndRemove";
        if (Action == Action.TurnRightAndRemove)
            return minified ? "D" : "TurnRightAndRemove";
        if (Action == Action.PickUp)
            return minified ? "P" : "PickUp";

        return string.Empty;
    }
}

public enum Action
{
    None,
    Start,
    Finish,
    Forward,
    TurnLeft,
    TurnRight,
    Bounce,
    TurnLeftAndRemove,
    TurnRightAndRemove,
    PickUp
}

public enum TurnDirection
{
    /// <summary>
    /// None turn direction.
    /// </summary>
    None = 0,
    /// <summary>
    /// /| turn direction.
    /// </summary>
    Turn00 = 1,
    /// <summary>
    /// |\ turn direction.
    /// </summary>
    Turn01 = 2,
    /// <summary>
    /// \| turn direction.
    /// </summary>
    Turn10 = 3,
    /// <summary>
    /// |/ turn direction.
    /// </summary>
    Turn11 = 4,
    /// <summary>
    /// / turn direction.
    /// </summary>
    Turn0011 = 5,
    /// <summary>
    /// \ turn direction.
    /// </summary>
    Turn0110 = 6,
}