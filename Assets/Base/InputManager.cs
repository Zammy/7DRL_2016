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

    void Start()
    {
        this.ActionExecutor.ActionExecutionComplete += this.OnActionExecutionCompleted;
        this.ActionExecutor.ActionExecutionStarted += this.OnActionExecutionStarted;
    }

    void OnDestroy()
    {
        this.ActionExecutor.ActionExecutionComplete -= this.OnActionExecutionCompleted;
        this.ActionExecutor.ActionExecutionStarted -= this.OnActionExecutionStarted;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.ActionExecutorList.PlayClicked();
        }
    }

    public void OnTileClicked(TileBehavior tileClicked)
    {
        if (this.selectedAction == null)
        {
            if (this.prevTileClicked != null) //todo: && !this.Player.ActionExecuted.Started)
            {
                Point destination = tileClicked.Pos;
                this.QueueDefaultMoveTo(destination);

                if (!this.Player.HasEnemiesInSight())
                {
                    this.ActionExecutor.Play();
                }
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
        this.ClearHighlightedTiles();

        if (tileHovered.Character != null && tileHovered.Character is Player)
        {
            return;
        }

        if (tileHovered.LightLevel < 0.1f)
        {
            return;
        }

        Point playerPos = this.LevelMng.GetPosOfCharacter(this.Player);

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
        this.ClearHighlight(tileHovered);
    }

    public void SetSelectedAction(GameActionData actionData)
    {
        this.selectedAction = actionData;

        int range = actionData.Range;
        Point playerPos = this.LevelMng.GetPosOfCharacter(this.Player);
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

    void QueueDefaultMoveTo(Point destination)
    {
        Point playerPos = this.LevelMng.GetPosOfCharacter(this.Player);

        Point[] path = this.LevelMng.PathFromAtoB(playerPos, destination);

        var defaultMove = this.Player.DefaultMoveAction;

        Point from = playerPos;
        for (int i = 0; i < path.Length; i++)
        {
            Point to = path[i];

            TileBehavior fromTile = this.LevelMng.GetTileBehavior(from);
            TileBehavior target = this.LevelMng.GetTileBehavior(to);
            this.ActionExecutor.EnqueueMoveAction(this.Player, defaultMove, fromTile, target, defaultMove.Length * i);

            from = to;
        }
    }

    void OnActionExecutionStarted()
    {
        this.ClearHighlightedTiles() ;  
    }

    void OnActionExecutionCompleted()
    {
        if (!this.Player.HasEnemiesInSight())
        {
            this.ActionExecutor.Play();
        }
    }
}
