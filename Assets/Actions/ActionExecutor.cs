using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionExecutor : MonoBehaviour 
{
    static ActionExecutor _instance = null;
    public static ActionExecutor Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    //Set through Unity
    public ActionExecutorList ActionExecutorList;
    //

    public event Action<GameAction> ActionExecutionStarted;
    public event Action<GameAction> ActionExecutionCompleted;

    public bool IsExecutingActions {get; private set;}
    public int GameTime {get; private set;}
    public int TicksPerFrame  {get; set;}

    List<GameAction> actions = new List<GameAction>();

    public ActionExecutor()
    {
        this.TicksPerFrame = 50;
    }

    public void ResetGameTime()
    {
        this.GameTime = 0;
    }

    public bool EnqueueAction(Character character, GameActionData gameActionData, TileBehavior target)
    {
        return EnqueueAction(character, gameActionData, new TileBehavior[] { target });
    }

    public bool EnqueueAction(Character character, GameActionData gameActionData, TileBehavior[] targets)
    {
        GameAction gameAction =  new GameAction(gameActionData, character, targets);

        //check if move to not valid target
//        var moveGameAction = gameAction as MoveGameAction;
//        if (moveGameAction != null &&
//            (moveGameAction.Target.Character || this.GetActionMovingToTile(target) != null))
//        {
//            return false;
//        }

        //check if enough stamina to execute
        if (character.Stamina < gameActionData.StaminaCost)
        {
            return false;
        }

        Debug.LogFormat("EnqueueAction {0} will {1}", character.Name, gameActionData.Name);

        this.Enqueue(gameAction, character);
        if (character is Player)
        {
            this.Play();
        }
        return true;
    }

    void Enqueue(GameAction gameAction, Character character)
    {
        this.ActionExecutorList.AddAction(gameAction);
        this.actions.Add(gameAction);
    }

    public void CancelAction(GameAction gameAction)
    {
        gameAction.Character.ActionExecuted = null;
        this.actions.Remove(gameAction);
    }

    public void Play()
    {
        if (this.IsExecutingActions 
            || this.actions.Count == 0
            || !this.HasPlayerQueuedAction())
        {
            return;
        }

        this.IsExecutingActions = true;

        var charactes = LevelMng.Instance.Characters;
        foreach (var character in charactes)
        {
            var action = this.GetActionOfCharacter(character);
            if (action == null)
            {
                ((Monster)character).DecideAndQueueAction();
            }
        }

        StartCoroutine( this.ExecuteActions() );
    }

//    public MoveGameAction GetActionMovingToTile(TileBehavior tile)
//    {
//        foreach(var action in this.actions)
//        {
//            if (action is MoveGameAction && action.Target == tile)
//            {
//                return action as MoveGameAction;
//            }
//        }
//
//        return null;
//    }
//
    public GameAction GetActionOfCharacter(Character character)
    {
        foreach(var action in this.actions)
        {
            if (action.Character == character)
            {
                return action;
            }
        }

        return null;
    }

    public void PlayWithDelay()
    {
        this.Invoke("Play", 0.01f);
    }

    public bool HasPlayerQueuedAction()
    {
        var action = this.GetActionOfCharacter( LevelMng.Instance.Player );
        if (action != null)
        {
            return action.TimeLeft > 0;
        }
        return false;
    }

    //inclues characteres moving in or out these tiles
    public List<Character> AllCharactersMovingToOrFromTiles(TileBehavior[] tiles)
    {
        List<Character> chars = new List<Character>();

        System.Action<Character> tryAdd = (Character c) =>
        {
            if (chars.Contains(c))
                return;

            chars.Add(c);
        };

        var moves = new List<Move>();
        foreach(var action in this.actions)
        {
            var move = action.GetComponent<Move>();
            if (move != null)
            {
                moves.Add(move);
            }
        }

        foreach (var tile in tiles)
        {
            foreach (var move in moves)
            {
                if (move.To == tile
                    || move.From == tile)
                {
                    tryAdd(move.Owner.Character);
                }
            }
        }

        return chars;
    }

    public void RemoveAllActionsOfCharacter(Character character)
    {
        for (int i = this.actions.Count - 1; i >= 0; i--)
        {
            if (this.actions[i].Character == character)
            {
                this.actions.RemoveAt(i);
            }
        }
    }

    public bool IsTileAvailableForMove(TileBehavior tile)
    {
        foreach(var action in this.actions)
        {
            var move = action.GetComponent<Move>();
            if (move == null)
                continue;

            if (move.To == tile)
            {
                return false;
            }
        }

        return tile.Tile.IsPassable 
            && tile.Character == null;
    }

    IEnumerator ExecuteActions()
    {
        bool playerActionFinished = false;
        while(!playerActionFinished)
        {
            var finishedGameActions = new List<GameAction>(this.actions.Count);
            for (int x = 0; x < this.TicksPerFrame; x++)
            {
                this.GameTime++;

                finishedGameActions.Clear();

                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    GameAction action = this.actions[i];

                    bool hasStarted = action.Started;

                    bool finished = action.Tick();

                    if (!hasStarted && action.Started) //if has just started
                    {
                        this.ActionExecutionStarted(action);
                    }

                    if (finished)
                    {
                        if (action.Character is Player)
                        {
                            playerActionFinished = true;
                        }

                        finishedGameActions.Add(action);

                        this.actions.RemoveAt(i);
                    }
                }

                if (finishedGameActions.Count > 0)
                {
                    foreach (var action in finishedGameActions)
                    {
                        this.ActionExecutionCompleted(action);
                    }
                }

                if (playerActionFinished)
                {
                    break;
                }
            }
            yield return null;
        }

        this.IsExecutingActions = false;
    }
}

