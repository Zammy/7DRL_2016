﻿using System.Collections.Generic;
using System;
using UnityEngine;

public class LevelMng : MonoBehaviour
{
    static LevelMng _instance = null;
    public static LevelMng Instance
    {
        get
        {
            return _instance;
        }
    }

    //Set through Unity
    public GameObject GroundPrefab;
    public GameObject WallPrefab;
    public GameObject EndPrefab;
    public GameObject ShadowDoorPrefab;

    public GameObject FeralDogPrefab;
    public GameObject RottingCorpsePrefab;

    public float TileSize;

    public Transform Level;
    public InputManager InputManager;
    //

    public Player Player { get; set; }
    public List<Character> Characters { get; set; }

    private TileBehavior[,] level;
    private Dictionary<MonsterType, GameObject> monsterPrefabs;
    private Dictionary<Room, List<Monster>> roomMonster = new Dictionary<Room, List<Monster>>();
    private Dictionary<Room, List<TileBehavior>> roomShadowDoor = new Dictionary<Room, List<TileBehavior>>();

    void Awake()
    {
        _instance = this;

        this.monsterPrefabs = new Dictionary<MonsterType, GameObject>()
        {
            { MonsterType.FeralDog, this.FeralDogPrefab },
            { MonsterType.RottingCorpse, this.RottingCorpsePrefab }
        };
    }

    void Start()
    {
        this.Characters = new List<Character>();
    }

    void OnDestroy()
    {
        //TODO: need to cleanup all tiles event handlers
    }

    public void LoadLevel(Dungeon dungeon)
    {
        roomMonster.Clear();
        roomShadowDoor.Clear();
        AddTilesTo(dungeon.Tiles, this.Level);
        AddMonsters(dungeon.monsters);
    }

    public void AddCharacterOnPos(Character character, Point pos)
    {
        if (character is Player)
        {
            this.Player = (Player)character;
        }

        this.level[pos.X, pos.Y].Character = character;

        this.Characters.Add(character);

        if (character is Player)
        {
            return;
        }

        var room = this.level[pos.X, pos.Y].Tile.Room;
        if (!this.roomMonster.ContainsKey(room))
        {
            this.roomMonster[room] = new List<Monster>();
        }
        this.roomMonster[room].Add(character as Monster);
    }

    public void KillCharacter(Character character)
    {
        this.Characters.Remove(character);
        if (character is Monster)
        {
            var room = character.GetTileBhv().Tile.Room;
            this.roomMonster[ room ].Remove((Monster)character);
            if (this.roomMonster[ room ].Count == 0)
            {
                this.DeactivateRoom(room);
            }
        }
        character.GetTileBhv().Character = null;
        Destroy(character.gameObject);

        ActionExecutor.Instance.RemoveAllActionsOfCharacter(character);
    }

    void AddTilesTo(Tile[,] tiles, Transform parent)
    {
        this.level = new TileBehavior[tiles.GetLength(0), tiles.GetLength(1)];

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Vector2 pos = new Vector2(TileSize * x, TileSize * y);
                var tile = tiles[x, y];

                if (tile == null)
                    continue;
                   
                GameObject prefabToUse = null;
                switch (tile.Type)
                {
                    case TileType.Ground:
                    {
                        prefabToUse = this.GroundPrefab;
                        break;
                    }
                    case TileType.Wall:
                    {
                        prefabToUse = this.WallPrefab;
                        break;
                    }
                    case TileType.End:
                    {
                        prefabToUse = this.EndPrefab;
                        break;
                    }
                    case TileType.ShadowDoor:
                    {
                        prefabToUse = this.ShadowDoorPrefab;
                        break;
                    }
                    default:
                        break;
                }
                var tileGo = (GameObject)Instantiate(prefabToUse, pos, Quaternion.identity);
                tileGo.transform.SetParent(parent);

                var behavior = tileGo.GetComponent<TileBehavior>();
                behavior.Tile = tile;
                behavior.Pos = new Point(x, y);
                behavior.Clicked += this.InputManager.OnTileClicked;
                behavior.HoverIn += this.InputManager.OnTileHoveredIn;
                behavior.HoverOut += this.InputManager.OnTileHoveredOut;
                this.level[x, y] = behavior;

                if (tile.Type == TileType.ShadowDoor)
                {
                    if (!roomShadowDoor.ContainsKey(tile.Room))
                    {
                        roomShadowDoor[tile.Room] = new List<TileBehavior>();
                    }
                    this.roomShadowDoor[tile.Room].Add(behavior);
                }
            }
        }
    }

    void AddMonsters(Dictionary<Point, MonsterType> monsters )
    {
        foreach (var kvp in monsters)
        {
            Point pos = kvp.Key;
            MonsterType mobType = kvp.Value;
            GameObject prefab = this.monsterPrefabs[mobType];
            
            var go = (GameObject)Instantiate(prefab);
            var monster = go.GetComponent<Monster>();

            this.AddCharacterOnPos(monster as Character, pos);
        }
    }

    #region Map Queries
    public TileBehavior GetTileBehavior(Point pos)
    {
        try
        {
            return this.level[pos.X, pos.Y];
        }
        catch
        {
            return null;
        }
    }

    public TileBehavior[] TilesAroundInRange(Point pos, int range)
    {
        var tiles = new List<TileBehavior>();

        System.Action<Point> tryAddTileBehavior = (p) =>
        {
            var tile = GetTileBehavior( p );
            if (tile != null && tile.Tile.IsPassable)
            {
                tiles.Add(tile);
            }
        };

        Point[] pointsInRange = this.PointsInRange(pos, range);

        foreach (var p in pointsInRange)
        {
            tryAddTileBehavior(p);
        }

        return tiles.ToArray();
    }

    public Point GetCharacterPos(Character character)
    {
        var tileBhv = character.GetTileBhv();
        if (tileBhv == null)
        {
            throw new UnityException(" Character not on any tile! ");
        }
        return tileBhv.Pos;
    }

    Point[] PointsInRange(Point pos, int range)
    {
        List<Point> points = new List<Point>();

        for (int i = -range; i <= range; i++)
        {
            int x = pos.X + i;
            int left = range - Mathf.Abs(i);

            if (left == 0)
            {
                points.Add( new Point(x, pos.Y) );
            }
            else
            {
                points.Add( new Point(x, pos.Y + left) );
                points.Add( new Point(x, pos.Y - left) );
            }
        }

        return points.ToArray();
    }

    #endregion

    #region Path Finder

    private class Step : IEquatable<Step>
    {
        public Point Pos;
        public int Heuristic;
        public int FromStart;

        public Step Parent = null;

        public Step()
        {
        }

        public Step (Point pos, int Heuristic, int FromStart)
        {
            this.Pos = pos;
            this.Heuristic = Heuristic;
            this.FromStart = FromStart;
        }

        public int Score { get { return Heuristic + FromStart; } }

        #region IEquatable implementation

        public bool Equals(Step other)
        {
            if (other == null) 
                return false;

            return other.Pos == this.Pos;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("[Step ({0}) : Heuristic={1} FromStart={2} Score={3}]", this.Pos, this.Heuristic, this.FromStart, this.Score);
        }
    }

    List<Step> closedList = new List<Step>();
    List<Step> openList = new List<Step>();

    public Point[] PathFromAtoB(Point start, Point goal)
    {
//        Debug.LogFormat("======= {0} to {1} =======", start, goal);

        closedList.Clear();
        openList.Clear();

        openList.Add( new Step(start, (start-goal).Length, 0) );

        Step stepGoal = null;
        while(true)
        {
            var lowestScoreStep = GetLowestScoreFromList(openList, goal);
//            Debug.LogFormat("lowestScoreStep {0}", lowestScoreStep);
            if (lowestScoreStep == null)
            {
                throw new UnityException("Open list should never be empty!");
            }
            if (lowestScoreStep.Pos == goal) 
            {
//                Debug.Log("Found goal!");
                stepGoal = lowestScoreStep;
                break;
            }

            openList.Remove(lowestScoreStep);
            closedList.Add(lowestScoreStep);

            Step[] stepsAround = GetStepsAroundStep(lowestScoreStep, goal);
            foreach(var step in stepsAround)
            {
                if (step == null)
                {
                    continue;
                }

                if (closedList.Contains(step))
                {
                    continue;
                }

                var sameStep = FindStepInPos(openList, step.Pos);

                if (sameStep != null)
                {
                    if (sameStep.Score <= step.Score)
                    {
                        continue;
                    }

                    openList.Remove( sameStep );
                }

//                Debug.Log("Adding to open list " + step.ToString());

                openList.Add(step);
            }
        }

        return ExtractPath(stepGoal);
    }

    Step GetLowestScoreFromList(List<Step> steps, Point goal)
    {
        int minScore = int.MaxValue;
        Step bestStep = null;
        foreach (var step in steps)
        {
            if (step.Pos == goal)
                return step;

            // >= so that if we have steps with equal score we will add the most recent addition
            if (minScore >= step.Score)
            {
                minScore = step.Score;
                bestStep = step;
            }
        }
        return bestStep;
    }

    Step[] GetStepsAroundStep(Step around, Point goal)
    {
        Step[] steps = new Step[4];

        System.Func<Point, Step> addStep = (Point pos) =>
        {
            if (pos == goal)
            {
                return new Step()
                {
                    Pos = pos,
                    Heuristic = 0,
                    FromStart = around.FromStart + 1,
                    Parent = around
                };
            }

            var tile = GetTileBehavior(pos);
            if (tile == null)
            {
                return null;
            }

            if (!ActionExecutor.Instance.IsTileAvailableForMove(tile))
            {
                return null;
            }

            return new Step()
            {
                Pos = pos,
                Heuristic = (pos-goal).Length,
                FromStart = around.FromStart + 1,
                Parent = around
            };
        };

        steps[0] = addStep( new Point(around.Pos.X + 1, around.Pos.Y) );
        steps[1] = addStep( new Point(around.Pos.X - 1, around.Pos.Y) );
        steps[2] = addStep( new Point(around.Pos.X, around.Pos.Y + 1) );
        steps[3] = addStep( new Point(around.Pos.X, around.Pos.Y - 1) );

        return steps;
    }

    Step FindStepInPos(List<Step> steps, Point pos)
    {
        foreach (var step in steps)
        {
            if (step.Pos == pos)
            {
                return step;
            }
        }

        return null;
    }

    Point[] ExtractPath(Step step)
    {
        List<Point> path = new List<Point>();

        while(step != null && step.Parent != null)
        {
            path.Add(step.Pos);

            if (step.Parent != null)
            {
                step = step.Parent;
            }
        }

        path.Reverse();

        return path.ToArray();
    }
    #endregion

    #region Line of Sight

    List<TileBehavior> tilesCache = new List<TileBehavior>();
    Point requestedPos;
    int requestedRange;

    public List<TileBehavior> TilesAroundInSight(Point pos, int range)
    {
        if (this.requestedPos == pos && requestedRange == range)
        {
            return tilesCache;
        }

        this.tilesCache.Clear();
        requestedPos = pos;
        requestedRange = range;

        Vector2 dir;
        for (float i = 0; i <= Mathf.PI*2; i += Mathf.PI/128f)
        {
            dir.x = Mathf.Sin(i);
            dir.y = Mathf.Cos(i);

            var behaviorsInDir = GetAllTileBehaviorsInDirection(pos, dir, range);
            foreach(var tileBhv in behaviorsInDir)
            {
                if (!tilesCache.Contains(tileBhv))
                {
                    tilesCache.Add(tileBhv);
                }

                if (!tileBhv.Tile.IsVisibleThrough)
                {
                    break;
                }
            }
        }

        return tilesCache;
    }

    List<TileBehavior> GetAllTileBehaviorsInDirection(Point origin, Vector2 direction, float distance)
    {
        var from = new Vector2(origin.X, origin.Y);
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, direction, distance, LayerMask.GetMask("Level") );

        List<TileBehavior> tiles = new List<TileBehavior>(hits.Length);
        foreach (RaycastHit2D hit in hits)
        {
            var tileBhv = hit.transform.GetComponent<TileBehavior>();
            if (tileBhv != null && tileBhv.Pos != origin)
            {
                tiles.Add(tileBhv);
            }
        }
        return tiles;
    }

    #endregion

    #region Room Activation

    public void ActivateRoom(Room room)
    {
        foreach (var monster in roomMonster[room])
        {
            monster.IsActive = true;
        }

        if (IsRoomClear(room))
        {
            return;
        }

        foreach (var shadow in roomShadowDoor[room])
        {
            shadow.TempImpassable = true;
        }
    }

    public void DeactivateRoom(Room room)
    {
        foreach (var shadow in roomShadowDoor[room])
        {
            shadow.TempImpassable = false;
        }
    }

    public bool IsRoomClear(Room room)
    {
        return roomMonster[room].Count == 0;
    }

    #endregion

}