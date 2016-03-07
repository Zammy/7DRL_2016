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

    TileBehavior tileClicked;

    void Start()
    {
        this.ActionExecutor.ActionExecutionComplete += this.OnActionExecutionCompleted;
    }

    void OnDestroy()
    {
        this.ActionExecutor.ActionExecutionComplete -= this.OnActionExecutionCompleted;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.ActionExecutorList.PlayClicked();
        }
    }

    public void TileClicked(TileBehavior tileClicked)
    {
        if (this.selectedAction == null)
        {
            if (this.tileClicked != null) //todo: && !this.Player.ActionExecuted.Started)
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
                this.tileClicked = tileClicked;
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

    public void SetSelectedAction(GameActionData actionData)
    {
        this.selectedAction = actionData;

        int range = actionData.Range;
        Point playerPos = this.LevelMng.GetPosOfCharacter(this.Player);
        TileBehavior[] tilesInRange = this.LevelMng.TilesAroundInRange(playerPos, range);
        foreach (var tile in tilesInRange)
        {
            tile.IsHighlighted = true;
        }
        this.highlightedTiles.AddRange(tilesInRange);
    }

    public void ClearHighlightedTiles()
    {
        foreach (var tile in highlightedTiles)
        {
            tile.IsHighlighted = false;
        }
        this.highlightedTiles.Clear();
    }

    IEnumerator RemoveClickedTileAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);

        this.tileClicked = null;
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

    void OnActionExecutionCompleted()
    {
        if (!this.Player.HasEnemiesInSight())
        {
            this.ActionExecutor.Play();
        }
    }
}
