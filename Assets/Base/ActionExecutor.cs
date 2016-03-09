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
        GameAction gameAction = SpawnGameAction(character, gameActionData, target);

        //check if move to not valid target
        var moveGameAction = gameAction as MoveGameAction;
        if (moveGameAction != null &&
            (moveGameAction.Target.Character || this.GetActionMovingToTile(target) != null))
        {
            return false;
        }

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

    public MoveGameAction GetActionMovingToTile(TileBehavior tile)
    {
        foreach(var action in this.actions)
        {
            if (action is MoveGameAction && action.Target == tile)
            {
                return action as MoveGameAction;
            }
        }

        return null;
    }

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
        return this.GetActionOfCharacter( LevelMng.Instance.Player ) != null;
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
            for (int x = 0; x < 50; x++)
            {
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

                        this.ActionExecutionCompleted(action);

                        this.actions.RemoveAt(i);
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

    GameAction SpawnGameAction(Character character, GameActionData actionData, TileBehavior target)
    {
        var moveActionData = actionData as MoveGameActionData;
        if (moveActionData != null)
        {
            var charPos = LevelMng.Instance.GetCharacterPos(character);
            var fromTile = LevelMng.Instance.GetTileBehavior(charPos);
            return new MoveGameAction(moveActionData, character, fromTile, target);
        }

        var rechargeActionData = actionData as RechargeGameActionData;
        if (rechargeActionData != null)
        {
            return new RechargeGameAction(rechargeActionData, character, target);
        }

        var attackActionData = actionData as AttackGameActionData;
        if (attackActionData != null)
        {
            return new AttackGameAction(attackActionData, character, target);
        }

        var defendActionData = actionData as DefendGameActionData;
        if (defendActionData != null)
        {
            return new DefendGameAction(defendActionData, character, target);
        }
        return null;
    }
}

public abstract class GameAction
{
    public GameActionData ActionData { get; private set; }
    public Character Character { get; private set; }
    public int TimeLeft { get; private set; }
    public TileBehavior Target {get; private set;}

    public bool Started { get; private set; }

    public GameAction(GameActionData actionData, Character character, TileBehavior target)
    {
        this.ActionData = actionData;
        this.Character = character;
        this.Target = target;
        this.TimeLeft = actionData.Length;
    }

    public virtual bool Tick()
    {
        if (!this.Started)
        {
            this.Start();
        }

        this.TimeLeft--;
        if (this.TimeLeft == 0)
        {
            this.Character.ActionExecuted = null;
            return true;
        }
        return false;
    }

    protected virtual void Start()
    {
        this.Started = true;

        this.Character.Stamina -= this.ActionData.StaminaCost;
    }
}

public class MoveGameAction : GameAction
{
    public TileBehavior From {get; private set;}

    Vector3 movePerTick;

    public MoveGameAction (GameActionData actionData, Character character, TileBehavior from, TileBehavior to)
        : base (actionData, character, to)
    {
        this.From = from;
    }

    protected override void Start()
    {
        base.Start();
        var difference = this.Target.transform.position - this.From.transform.position;
        this.movePerTick = difference / (float)this.TimeLeft;

        this.From.Character = null;
    }

    public override bool Tick()
    {
        bool arrived = base.Tick();
        if (this.Started)
        {
            this.Character.transform.position += movePerTick;

            if (arrived)
            {
                this.Target.Character = this.Character;
            }
        }

        return arrived;
    }

    public override string ToString()
    {
        return string.Format("[{0} Who={1} From={2} To={3}]]", ActionData.Name, Character.Name, From, Target);
    }
}

public class RechargeGameAction : GameAction
{
    public RechargeGameAction(GameActionData actionData, Character character, TileBehavior target)
        : base(actionData, character, target)
    {
    }

    protected override void Start()
    {
        base.Start();

        if (this.Character.Stamina > this.Character.StartStamina)
        {
            this.Character.Stamina = this.Character.StartStamina;
        }
    }
}
    

public class AttackGameAction : GameAction
{
    public readonly int Damage;

    public AttackGameAction (GameActionData actionData, Character character, TileBehavior target)
        : base (actionData, character, target)
    {
        var attackData = actionData as AttackGameActionData;
        this.Damage = attackData.Damage;
    }

    public override bool Tick()
    {
        bool hit = base.Tick();
        if (hit)
        {
//            int damage = this.Damage;
//            var opp = this.Target.GetComponentInChildren<Character>();
//            if (opp != null)
//            {
//                if (opp.ActionExecuted != null && opp.ActionExecuted is DefendGameAction)
//                {
//                    damage = ((DefendGameAction)opp.ActionExecuted).DefendAgainst(this);
//                }
//            }
//            else
//            {
//                var moveGameAction = ActionExecutor.Instance.GetActionMovingToTile(this.Target);
//                if (moveGameAction != null)
//                {
//                    float percentLeftToReach = (float)moveGameAction.TimeLeft / (float)moveGameAction.ActionData.Length;
//                    if (percentLeftToReach < Random.Range(0f, 0.5f)) 
//                    {
//                        opp = moveGameAction.Character;
//                    }
//                }
//            }
//
//            if (opp != null)
//            {
//                opp.Health -= damage;
//            }
        }
        return hit;
    }
}

public class DefendGameAction : GameAction
{
    int flatDmgReduction;

    public DefendGameAction(GameActionData actionData, Character character, TileBehavior target)
        : base (actionData, character, target)
    {
        var defendData = actionData as DefendGameActionData;
        this.flatDmgReduction = defendData.FlatDamageReduction;
    }

    public int DefendAgainst(AttackGameAction attackAction)
    {
        return Mathf.Max(attackAction.Damage - this.flatDmgReduction, 0);
    }
}