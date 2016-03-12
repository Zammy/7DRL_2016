using UnityEngine;

public struct Point
{
    public int X;
    public int Y;

    public Point (int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public Point(Point toCopy)
    {
        this.X = toCopy.X;
        this.Y = toCopy.Y;
    }

    public int Length
    {
        get
        {
            //manhattan
            return Mathf.Abs(this.X) + Mathf.Abs(this.Y);
        }
    }

    public static bool operator ==(Point a, Point b) 
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Point a, Point b) 
    {
        return !(a.X == b.X && a.Y == b.Y);
    }

    public static Point operator +(Point a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y);
    }

    public static Point operator -(Point a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }

    public override bool Equals(object obj)
    {
        if (obj is Point)
        {
            return this == (Point)obj;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return 17 + this.X + this.Y * 23;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", this.X, this.Y);
    }

    static Point zero = new Point();
    public static Point Zero
    {
        get
        {
            return zero;
        }
    }
}

public enum TileType
{
    Wall,
    Ground,
    Start,
    End,
    ShadowDoor
}

public class Tile
{
    public Tile()
    {
        this.Type = TileType.Ground;
    }

    public TileType Type;

    public Room Room;

    public bool IsPassable
    {
        get
        {
            if (this.Type == TileType.ShadowDoor)
                return !this.TempImpassable;

            return this.Type != TileType.Wall;
        }
    }

    public bool IsVisibleThrough
    {
        get
        {
            return this.Type != TileType.Wall
                && this.Type != TileType.ShadowDoor;
        }
    }

    public bool TempImpassable
    {
        get;
        set;
    }
}

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public static class DirectionExt
{
    public static Direction Opposite(this Direction dir)
    {
        return (Direction) (((int)dir + 2) % 4);
    }
}
