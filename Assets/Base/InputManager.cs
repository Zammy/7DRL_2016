using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour 
{
    //Set through Unity
    public ActionExecutorList ActionExecutorList;
    public ActionExecutor ActionExecutor;
    public LevelMng LevelMng;
    //

    public Player Player
    {
        get;
        set;
    }

    List<TileBehavior> highlightedTiles = new List<TileBehavior>();
    GameActionData selectedAction;

    TileBehavior prevTileClicked;

    List<Point> moveQueue = new List<Point>();

    void Start()
    {
        this.ActionExecutor.ActionExecutionCompleted += this.OnActionExecutionCompleted;
        this.ActionExecutor.ActionExecutionStarted += this.OnActionExecutionStarted;
    }

    void OnDestroy()
    {
        this.ActionExecutor.ActionExecutionCompleted -= this.OnActionExecutionCompleted;
        this.ActionExecutor.ActionExecutionStarted -= this.OnActionExecutionStarted;
    }
    
    void Update()
    {
        if (this.ActionExecutor.IsExecutingActions)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Point origin = this.LevelMng.GetPlayerPos();
            Point destination = new Point(origin);
            destination.X -= 1;
            this.CheckIfPlayerCanMoveToTileAndQueue(destination);
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Point origin = this.LevelMng.GetPlayerPos();
            Point destination = new Point(origin);
            destination.Y += 1;
            this.CheckIfPlayerCanMoveToTileAndQueue(destination);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Point origin = this.LevelMng.GetPlayerPos();
            Point destination = new Point(origin);
            destination.X += 1;
            this.CheckIfPlayerCanMoveToTileAndQueue(destination);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Point origin = this.LevelMng.GetPlayerPos();
            Point destination = new Point(origin);
            destination.Y -= 1;
            this.CheckIfPlayerCanMoveToTileAndQueue(destination);
        }
    }

    public void OnTileClicked(TileBehavior tileClicked)
    {
        if (this.selectedAction == null)
        {
            if (this.prevTileClicked != null) //todo: && !this.Player.ActionExecuted.Started)
            {
                Point destination = tileClicked.Pos;
                this.EnqueueMovePath(destination);
            }
            else
            {
                this.prevTileClicked = tileClicked;
                StartCoroutine( this.RemoveClickedTileAfterDelay() );
            }
            return;
        }

        if (highlightedTiles.Contains(tileClicked))
        {
            this.ActionExecutor.EnqueueAction( this.Player, this.selectedAction, tileClicked );
        }

        this.Player.ActionMenu.IsVisible = false;

        this.selectedAction = null;
    }

    public void OnTileHoveredIn(TileBehavior tileHovered)
    {
        return;

        if (this.selectedAction)
        {
            return;
        }

        this.ClearHighlightedTiles();

        if (tileHovered.Character != null && tileHovered.Character is Player)
        {
            return;
        }

        if (tileHovered.GetComponent<LightTarget>().LightLevel < 0.1f)
        {
            return;
        }

        if (!tileHovered.Tile.IsPassable)
        {
            return;
        }

        Point playerPos = this.LevelMng.GetPlayerPos();

        Point[] path = this.LevelMng.PathFromAtoB(playerPos, tileHovered.Pos);

        TileBehavior[] toHighlight = new TileBehavior[ path.Length ];
        for (int i = 0; i < path.Length; i++)
        {
            TileBehavior tileBhv = this.LevelMng.GetTileBehavior(path[i]);
            toHighlight[i] = tileBhv;
        }

        this.HighlightTiles(toHighlight);
    }

    public void OnTileHoveredOut(TileBehavior tileHovered)
    {
        return;

        if (this.selectedAction)
        {
            return;
        }

        if (tileHovered.Tile.Type == TileType.Ground)
        {
            this.ClearHighlight(tileHovered);
        }
    }

    public void SetSelectedAction(GameActionData actionData)
    {
        this.selectedAction = actionData;

        int range = actionData.Range;
        Point playerPos = this.LevelMng.GetPlayerPos();
        TileBehavior[] tilesInRange = this.LevelMng.TilesAroundInRange(playerPos, range);
        this.HighlightTiles(tilesInRange);
    }

    public void ClearHighlightedTiles()
    {
        foreach (var tile in highlightedTiles)
        {
            tile.IsHighlighted = false;
        }
        this.highlightedTiles.Clear();
    }

    void ClearHighlight(TileBehavior tileBhv)
    {
        this.highlightedTiles.Remove(tileBhv);
        tileBhv.IsHighlighted = false;
    }

    void HighlightTiles(TileBehavior[] tilesInRange)
    {
        foreach (var tile in tilesInRange)
        {
            tile.IsHighlighted = true;
        }
        this.highlightedTiles.AddRange(tilesInRange);
    }

    IEnumerator RemoveClickedTileAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);

        this.prevTileClicked = null;
    }

    void EnqueueMovePath(Point destination)
    {
        this.moveQueue.Clear();

        Point playerPos = this.LevelMng.GetPlayerPos();

        Point[] path = this.LevelMng.PathFromAtoB(playerPos, destination);

        this.moveQueue.AddRange(path);

        this.EnqueueNextMoveAction();
    }

    void OnActionExecutionStarted(GameAction gameAction)
    {
        this.ClearHighlightedTiles();
        this.Player.ActionMenu.IsVisible = false;
    }

    void OnActionExecutionCompleted(GameAction gameAction)
    {
        if (gameAction.Character != this.Player ||
            this.Player.HasEnemiesInSight())
        {
            return;
        }

        this.EnqueueNextMoveAction();
    }

    void CheckIfPlayerCanMoveToTileAndQueue(Point destination)
    {
        var destTile = this.LevelMng.GetTileBehavior(destination);
        if (destTile != null && destTile.Tile.IsPassable)
        {
            if (this.ActionExecutor.EnqueueAction(this.Player, this.Player.DefaultMoveAction, destTile))
            {
                this.ActionExecutor.PlayWithDelay();
            }
        }
    }

    void EnqueueNextMoveAction()
    {
        if (this.moveQueue.Count == 0 )
        {
            return;
        }

        Point nextDest = this.moveQueue[0];
        this.moveQueue.RemoveAt(0);
        this.CheckIfPlayerCanMoveToTileAndQueue(nextDest);
    }
}
