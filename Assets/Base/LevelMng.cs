using System.Collections.Generic;
using System;
using UnityEngine;
using RogueLib;

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

    public float TileSize;

    public Transform Level;
    public InputManager InputManager;
    //

    private TileBehavior[,] level;

    void Awake()
    {
        _instance = this;
    }

    public void LoadLevel(Dungeon dungeon)
    {
        AddTilesTo(dungeon.Tiles, this.Level);
    }

    public void AddCharacterOnPos(Character character, Point pos)
    {
        this.level[pos.X, pos.Y].Character = character;
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
//                        case TileType.Start:
//                        {
//                            prefabToUse = this.StartPrefab;
//                            break;
//                        }
//                        case TileType.End:
//                        {
//                            prefabToUse = this.EndPrefab;
//                            break;
//                        }
                    default:
                        break;
                }
                var tileGo = (GameObject)Instantiate(prefabToUse, pos, Quaternion.identity);
                tileGo.transform.SetParent(parent);

                var behavior = tileGo.GetComponent<TileBehavior>();
                behavior.Tile = tile;
                behavior.Pos = new Point(x, y);
                behavior.Clicked += this.InputManager.TileClicked;
                this.level[x, y] = behavior;
            }
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
            if (tile != null)
            {
                tiles.Add(tile);
            }
        };

        for (int i = -range; i <= range; i++)
        {
            int x = pos.X + i;
            int left = range - Mathf.Abs(i);

            if (left == 0)
            {
                tryAddTileBehavior( new Point(x, pos.Y) );
            }
            else
            {
                tryAddTileBehavior( new Point(x, pos.Y + left) );
                tryAddTileBehavior( new Point(x, pos.Y - left) );
            }
        }

        return tiles.ToArray();
    }

    public Point GetPosOfCharacter(Character character)
    {
        var tileBhv = character.transform.parent.GetComponent<TileBehavior>();
        if (tileBhv == null)
        {
            throw new UnityException(" Character not on any tile! ");
        }
        return tileBhv.Pos;
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
    }

    List<Step> closedList = new List<Step>();
    List<Step> openList = new List<Step>();

    public Point[] PathFromAtoB(Point start, Point goal)
    {
        closedList.Clear();
        openList.Clear();

        openList.Add( new Step(start, (start-goal).Length, 0) );

        Step stepGoal;
        while(true)
        {
            var lowestScoreStep = GetLowestScoreFromList(openList, goal);
            if (lowestScoreStep.Pos == goal) 
            {
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

                openList.Add(step);
            }
        }

        return ExtractPath(stepGoal);
    }

    bool IsPassable(Point p)
    {
        try 
        {
            if (this.level[p.X, p.Y] == null)
                return false;
        }
        catch
        {
            return false;
        }

        return this.level[p.X, p.Y].Tile.IsPassable;
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
            if (!this.IsPassable(pos))
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
        do
        {
            path.Add(step.Pos);
            step = step.Parent;
        }
        while(step.Parent != null);

        path.Reverse();

        return path.ToArray();
    }
    #endregion
}