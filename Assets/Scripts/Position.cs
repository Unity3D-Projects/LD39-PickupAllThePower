public struct Position
{
    public static readonly Position Zero = new Position(0, 0);
    public static readonly Position Left = new Position(0, -1);
    public static readonly Position Right = new Position(0, 1);
    public static readonly Position Up = new Position(-1, 0);
    public static readonly Position Down = new Position(1, 0);
    public static readonly Position LeftUp = new Position(-1, -1);
    public static readonly Position LeftDown = new Position(1, -1);
    public static readonly Position RightUp = new Position(-1, 1);
    public static readonly Position RightDown = new Position(1, 1);

    public int r, c;

    public Position(int r, int c)
    {
        this.r = r;
        this.c = c;
    }

    public override bool Equals(object obj)
    {
        var pos = (Position)obj;
        return r == pos.r && c == pos.c;
    }

    public override int GetHashCode()
    {
        return r + 2 * c;
    }

    public static Position operator +(Position p1, Position p2)
    {
        return new Position(p1.r + p2.r, p1.c + p2.c);
    }

    public static Position operator -(Position p1, Position p2)
    {
        return new Position(p1.r - p2.r, p1.c - p2.c);
    }

    public static Position operator +(Position p, int v)
    {
        return new Position(p.r + v, p.c + v);
    }

    public static Position operator -(Position p, int v)
    {
        return new Position(p.r - v, p.c - v);
    }

    public static Position operator *(int v, Position p)
    {
        return new Position(v * p.r, v * p.c);
    }

    public static Position operator *(Position p, int v)
    {
        return v * p;
    }

    public static bool operator ==(Position p1, Position p2)
    {
        return p1.Equals(p2);
    }

    public static bool operator !=(Position p1, Position p2)
    {
        return !p1.Equals(p2);
    }

    public static Position TurnLeft(Position dir)
    {
        return new Position(-dir.c, dir.r);
    }

    public static Position TurnRight(Position dir)
    {
        return new Position(dir.c, -dir.r);
    }

    public static Position Inverse(Position dir)
    {
        return -1 * dir;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", r, c);
    }
}