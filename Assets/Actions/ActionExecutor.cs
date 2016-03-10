using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RogueLib;
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

    List<GameAction> actions = new List<GameAction>();

    public bool IsExecutingActions {get; private set;}

    public bool EnqueueAction(Character character, GameActionData gameActionData, TileBehavior target)
    {
        GameAction gameAction =  new GameAction(gameActionData, character, new TileBehavior[]{target});

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

        this.RemovePreviousAcitonForCharacterNotStarted(character);

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

        Debug.Log("Play()");

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

    public float CharacterCloseTo(TileBehavior target)
    {
        foreach(var action in this.actions)
        {
            var move = action.GetComponent<Move>();
            if (move != null)
            {
                if (move.To == target)
                {
                    return (float)action.TimeLeft / (float)action.ActionData.Length;
                }
                if (move.From == target)
                {
                    return 1 - ((float)action.TimeLeft / (float)action.ActionData.Length);
                } 
            }
        }
        return 0f;
    }

    void RemovePreviousAcitonForCharacterNotStarted(Character character)
    {
        for (int i = this.actions.Count - 1; i >= 0; i--)
        {
            var action = this.actions[i];
            if (action.Character == character && !action.Started)
            {
                this.ActionExecutorList.RemoveAction(action);
                this.actions.RemoveAt(i);
                break;
            }
        }
    }

    IEnumerator ExecuteActions()
    {
        bool playerActionFinished = false;
        while(!playerActionFinished)
        {
            var finishedGameActions = new List<GameAction>(this.actions.Count);
            for (int x = 0; x < 25; x++)
            {
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

