using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//binary space partitioning
public class BSPGenerator 
{
    GenOptions opts;

    BinaryNode root;

    List<Corridor> corridors = new List<Corridor>();
    List<Room> rooms = new List<Room>();

    public BSPGenerator()
    {
    }

    public Dungeon GenAndDrawRooms(GenOptions opts)
    {
        this.opts = opts;

        Random.seed = opts.Seed;

        this.rooms.Clear();
        this.corridors.Clear();

        root = new BinaryNode(Point.Zero, Point.Zero, opts.DUN_WIDTH, opts.DUN_HEIGHT);
       
        Divide(root);

        GenRooms(root);

        GenCorridors(root);

        Tile[,] tiles = this.GenTiles();

        var lastRoomGenned = this.rooms[this.rooms.Count - 1];
        Point endPoint = lastRoomGenned.GetRandomPointInsideRoom(1);
        tiles[endPoint.X, endPoint.Y].Type = TileType.End;

        return new Dungeon()
        {
            Tiles = tiles,
            PlayerStartPos = this.rooms[0].GetRandomPointInsideRoom(1),
            Rooms = rooms
        };
    }

    void Divide(BinaryNode node)
    {
        if (node.Area() < opts.MAX_LEAF_AREA)
        {
            return;
        }

        int direction = 0;
        if (node.Width < node.Height)
        {
            direction = 1;
        }

        if (direction == 0) // split vertically
        {
            int x = Random.Range(opts.MIN_LEAF_SIDE, node.Width-opts.MIN_LEAF_SIDE);
            node.Children[0] = new BinaryNode(Point.Zero, new Point(node.GlobalPos), x, node.Height);
            node.Children[1] = new BinaryNode(new Point(x, 0), new Point(node.GlobalPos.X + x, node.GlobalPos.Y), node.Width - x, node.Height);
        }
        else // split horizontally
        {
            int y = Random.Range(opts.MIN_LEAF_SIDE, node.Height-opts.MIN_LEAF_SIDE);
            node.Children[0] = new BinaryNode(Point.Zero, new Point(node.GlobalPos), node.Width, y);
            node.Children[1] = new BinaryNode(new Point(0, y), new Point(node.GlobalPos.X, node.GlobalPos.Y + y), node.Width, node.Height - y);
        }

        Divide(node.Children[0]);
        Divide(node.Children[1]);
    }

    void GenRooms(BinaryNode node)
    {
        if (node.IsLeaf())
        {
            this.rooms.Add( node.GenRoom(opts.MIN_ROOM_SIDE) );
        }
        else
        {
            GenRooms(node.Children[0]);
            GenRooms(node.Children[1]);
        }
    }

    void GenCorridors(BinaryNode node)
    {
        if (node.IsLeaf())
            return;

        if (!node.Children[0].IsLeaf())
        {
            GenCorridors(node.Children[0]);
        }
        if (!node.Children[1].IsLeaf())
        {
            GenCorridors(node.Children[1]);
        }

        BinaryNode leaf1 = node.Children[0];
        BinaryNode leaf2 = node.Children[1];

        var rooms1 = new List<Room>();
        var rooms2 = new List<Room>();

        leaf1.FillRoomsRecursively(rooms1);
        leaf2.FillRoomsRecursively(rooms2);

        Room[] roomsToConnect = FindRoomPairToConnect(rooms1, rooms2);
        Room room1 = roomsToConnect[0];
        Room room2 = roomsToConnect[1];

        //make a corridor
        if (leaf1.GlobalPos.Y == leaf2.GlobalPos.Y)
        {
            this.corridors.Add( DigCorridorRightTo(room1, room2) );
        }
        else
        {
            this.corridors.Add( DigCorridorUpTo(room1, room2) );
        }
    }

    Room[] FindRoomPairToConnect(List<Room> rooms1, List<Room> rooms2)
    {
        int smallestDist = int.MaxValue;
        Room[] closestsRooms = new Room[2];
        foreach (var r1 in rooms1)
        {
            foreach (var r2 in rooms2) 
            {
                var diff = r1.GetCenter() - r2.GetCenter();
                int dist = diff.Length;
                if (dist < smallestDist)
                {
                    smallestDist = dist;
                    closestsRooms[0] = r1;
                    closestsRooms[1] = r2;
                }
            }   
        }
        return closestsRooms;
    }

    Corridor DigCorridorRightTo(Room fromRoom, Room goalRoom)
    {
        var startPoint = new Point( fromRoom.GlobalPos.X + fromRoom.Width-1,
            Random.Range(fromRoom.GlobalPos.Y+1, fromRoom.GlobalPos.Y + fromRoom.Height -1) );
        var corridor = new Corridor(startPoint);
        Point goal = goalRoom.GetRandomPointInsideRoom(2);
        corridor.Points.Add( new Point( goal.X, startPoint.Y) );
        corridor.Points.Add( new Point( goal.X, goal.Y) );
        return corridor;
    }

    Corridor DigCorridorUpTo(Room fromRoom, Room goalRoom)
    {
        var startPoint = new Point( Random.Range( fromRoom.GlobalPos.X+1, fromRoom.GlobalPos.X + fromRoom.Width -1),
             fromRoom.GlobalPos.Y + fromRoom.Height-1);
        var corridor = new Corridor(startPoint);
        Point goal = goalRoom.GetRandomPointInsideRoom(2);
        corridor.Points.Add( new Point( startPoint.X, goal.Y) );
        corridor.Points.Add( new Point( goal.X, goal.Y) );
        return corridor;
    }

    Tile[,] GenTiles()
    {
        Tile[,] grid = new Tile[opts.DUN_WIDTH, opts.DUN_HEIGHT];
        System.Action<Tile, Point> addTile = (Tile tile, Point pos) => 
        {
            grid[pos.X, pos.Y] = tile;
        };
        foreach (var room in this.rooms)
        {
            int bottomY = room.GlobalPos.Y;
            int topY = room.GlobalPos.Y + room.Height -1;
            for (int x = room.GlobalPos.X; x < room.GlobalPos.X + room.Width; x++)
            {
                var tile = new Tile();
                tile.Room = room;
                tile.Type = TileType.Wall;
                addTile(tile, new Point(x, bottomY));
                tile = new Tile();
                tile.Room = room;
                tile.Type = TileType.Wall;
                addTile(tile, new Point(x, topY));
            }
            int leftX = room.GlobalPos.X ;
            int rightX = room.GlobalPos.X + room.Width -1;
            for (int y = room.GlobalPos.Y; y < room.GlobalPos.Y + room.Height; y++)
            {
                var tile = new Tile();
                tile.Type = TileType.Wall;
                tile.Room = room;
                addTile(tile, new Point(leftX, y));

                tile = new Tile();
                tile.Type = TileType.Wall;
                tile.Room = room;
                addTile(tile, new Point(rightX, y));
            }
            for (int x = room.GlobalPos.X +1; x < room.GlobalPos.X + room.Width -1; x++)
            {
                for (int y = room.GlobalPos.Y +1; y < room.GlobalPos.Y + room.Height-1; y++)
                {
                    var tile = new Tile();
                    tile.Room = room;
                    tile.Type = TileType.Ground;
                    addTile(tile,  new Point(x, y));
                }
            }
        }

        System.Action<Point> ifNothingAddWall = (Point p) =>
        {
            if (grid[ p.X , p.Y] == null)
            {
                addTile( new Tile()
                {
                    Type = TileType.Wall
                }, p);
            }
        };

        System.Action<Point, Point> addUp = (Point a, Point b) => 
        {
            for (int y = a.Y; y <= b.Y; y++) 
            {
                if( grid[a.X, y] == null )
                {
                    addTile( new Tile(), new Point( a.X, y ));
                }

                grid[a.X, y].Type = TileType.Ground;

                ifNothingAddWall (new Point(a.X -1, y));
                ifNothingAddWall (new Point(a.X +1, y));
            }

            ifNothingAddWall (new Point(a.X - 1 , b.Y + 1));
            ifNothingAddWall (new Point(a.X + 1 , b.Y + 1));

        };

        System.Action<Point, Point> addRight = (Point a, Point b) => 
        {
            for (int x = a.X; x <= b.X; x++) 
            {
                if( grid[x, a.Y] == null )
                {
                    addTile( new Tile(),  new Point( x, a.Y ));
                }

                grid[x, a.Y].Type = TileType.Ground;


                ifNothingAddWall (new Point(x, a.Y-1));
                ifNothingAddWall (new Point(x, a.Y+1));
            }

            ifNothingAddWall (new Point(b.X +1, a.Y-1));
            ifNothingAddWall (new Point(b.X +1, a.Y+1));
        };

        foreach (var corridor in this.corridors)
        {
            Point a = corridor.Points[0];
            for (int i = 0; i < corridor.Points.Count; i++)
            {
                Point b = corridor.Points[i];
                if (a.X == b.X)
                {
                    if (a.Y > b.Y)
                    {
                        addUp(b, a);
                    }
                    else
                    {
                        addUp(a, b);
                    }
                }
                else
                {
                    if (a.X > b.X)
                    {
                        addRight(b, a);
                    }
                    else
                    {
                        addRight(a, b);
                    }
                }

                a = b;
            }
        }

        System.Action<int, int> makeDoor = (x, y) =>
        {
            try
            {
                var tile = grid[x, y];
                if (tile.Type == TileType.Ground)
                    tile.Type = TileType.ShadowDoor;
            }
            catch
            {}
        };

        foreach (var room in this.rooms)
        {
            int bottomY = room.GlobalPos.Y;
            int topY = room.GlobalPos.Y + room.Height -1;
            for (int x = room.GlobalPos.X; x < room.GlobalPos.X + room.Width; x++)
            {
                makeDoor(x, bottomY);
                makeDoor(x, topY);
            }
            int leftX = room.GlobalPos.X ;
            int rightX = room.GlobalPos.X + room.Width -1;
            for (int y = room.GlobalPos.Y; y < room.GlobalPos.Y + room.Height; y++)
            {
                makeDoor(leftX, y);
                makeDoor(rightX, y);
            }
        }

        return grid;
    }
}
