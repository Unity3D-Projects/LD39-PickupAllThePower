using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Solution : List<Step>
{
    private Position _startDirection;
    private Position _startPosition;
    private Position _finishPosition;

    public Solution(Position startDirection)
      : this(startDirection, Enumerable.Empty<Step>())
    {
    }

    public Solution(Position startDirection, IEnumerable<Step> steps)
      : base(steps)
    {
        _startDirection = startDirection;
    }

    public override bool Equals(object obj)
    {
        var sol = obj as Solution;
        if (sol == null)
            return false;

        if (Count != sol.Count)
            return false;

        for (int i = 0; i < Count; i++)
        {
            if (!this[i].Equals(sol[i]))
                return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        for (int i = 0; i < Count; i++)
        {
            hash += (i + 1) * this[i].GetHashCode();
        }
        return hash;
    }

    public void Add(Position position, Action action, TurnDirection turnDirection = TurnDirection.None)
    {
        Add(new Step() { Action = action, TurnDirection = turnDirection, Position = position });
    }

    public void RemoveToLast(int index)
    {
        RemoveRange(index, Count - index);
    }

    public Solution Clone()
    {
        return new Solution(_startDirection, this.AsEnumerable())
        {
            StartPosition = StartPosition,
            FinishPosition = FinishPosition
        };
    }

    public Position StartDirection
    {
        get { return _startDirection; }
    }

    public Position StartPosition
    {
        get { return _startPosition; }
        set { _startPosition = value; }
    }

    public Position FinishPosition
    {
        get { return _finishPosition; }
        set { _finishPosition = value; }
    }

    public string ToStringInline(bool minified)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            builder.Append(this[i].ToStringInline(minified));

            if (!minified)
                builder.Append(" ");
        }
        return builder.ToString();
    }

    public Step First
    {
        get { return this[0]; }
    }

    public Step Last
    {
        get { return this[Count - 1]; }
    }
}
