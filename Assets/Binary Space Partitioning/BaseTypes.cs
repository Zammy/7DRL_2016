using System.Collections.Generic;
using UnityEngine;

public class GenOptions
{
    public int Seed = 1;

    public int DUN_WIDTH = 100;
    public int DUN_HEIGHT= 100;

    public int MAX_LEAF_AREA = 1000;
    public int MIN_LEAF_SIDE = 10;
    public int MIN_ROOM_SIDE = 4;
}

public class Dungeon
{
    public Tile[,] Tiles;
    public Point PlayerStartPos;
    public List<Room> Rooms;
}

public class BinaryNode
{
    public Point LocalPos;
    public Point GlobalPos;
    public int Width;
    public int Height;

    public BinaryNode[] Children = new BinaryNode[2];

    public Room Room {get; private set; }

    public BinaryNode(Point local, Point glob, int width, int height)
    {
        this.LocalPos = local;
        this.GlobalPos = glob;
        this.Width = width;
        this.Height = height;
    }

    public int Area()
    {
        return this.Width * this.Height;
    }

    public bool IsLeaf()
    {
        return this.Children[0] == null && this.Children[1] == null;
    }

    public void FillRoomsRecursively(List<Room> rooms)
    {
        if (this.Room != null)
        {
            rooms.Add(this.Room);
        }

        if (this.IsLeaf())
            return;

        this.Children[0].FillRoomsRecursively(rooms);
        this.Children[1].FillRoomsRecursively(rooms);
    }

    public Room GenRoom(int MIN_ROOM_SIDE)
    {
        var room = new Room();

        var newPos = new Point ();
        newPos.X = Random.Range(0, Mathf.Min(this.Width/2, this.Width - MIN_ROOM_SIDE));
        newPos.Y = Random.Range(0, Mathf.Min(this.Height/2, this.Height - MIN_ROOM_SIDE));
        room.LocalPos = newPos;

        room.GlobalPos = this.GlobalPos + room.LocalPos;

        room.Width = Random.Range(MIN_ROOM_SIDE, this.Width - newPos.X  );
        room.Height = Random.Range(MIN_ROOM_SIDE, this.Height - newPos.Y );

        this.Room = room;
        return room;
    }
}

public class Room
{
    public Point LocalPos;
    public Point GlobalPos;
    public int Width;
    public int Height;

    public bool IsInRoom(Point point)
    {
        return point.X >= this.GlobalPos.X && 
            point.Y >= this.GlobalPos.Y && 
            point.X <= this.GlobalPos.X + this.Width &&
            point.Y <= this.GlobalPos.Y + this.Height;
    }

    public Point GetRandomPointInsideRoom(int padding = 0)
    {
        return new Point( Random.Range(this.GlobalPos.X+1 + padding, this.GlobalPos.X + this.Width - padding), 
            Random.Range(this.GlobalPos.Y+1 + padding, this.GlobalPos.Y + this.Height - padding) );
    }

    public Point GetCenter()
    {
        return new Point( this.GlobalPos.X + this.Width/2, this.GlobalPos.Y + this.Height/2 );
    }
}

public class Corridor
{
    public List<Point> Points;

    public Corridor(Point startingPoint)
    {
        this.Points = new List<Point>( );
        this.Points.Add(startingPoint);
    }
}