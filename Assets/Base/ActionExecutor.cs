using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RogueLib;

public class ActionExecutor : MonoBehaviour 
{
    public static ActionExecutor Instance;

//    public Map Map;

//    public Announcements Announcements;

    void Awake()
    {
        Instance = this;
    }

    List<GameAction> actions = new List<GameAction>();

    bool isExecutingActions = false;

    public void Play()
    {
        if (this.isExecutingActions)
            return;

//        var charactersWithoutActions = new List<Character>();
//        foreach(Character character in this.Map.Characters)
//        {
//            if (character.ActionExecuted == null)
//            {
//                charactersWithoutActions.Add(character);
//            }
//        }
//        if (charactersWithoutActions.Count > 0)
//        {
//            this.Announcements.ShowCharactersNotTakenAction(charactersWithoutActions);
//            return;
//        }
//
        this.isExecutingActions = true;
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

    void RemovePreviousAcitonForCharacter(Character character)
    {
        for (int i = this.actions.Count - 1; i >= 0; i--)
        {
            if (this.actions[i].Character == character)
            {
                this.actions.RemoveAt(i);
                return;
            }
        }
    }

    IEnumerator ExecuteActions()
    {
        bool actionFinished = false;
        while(!actionFinished)
        {
            for (int x = 0; x < 50; x++)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--) 
                {
                    var action = this.actions[i];
                    if (!action.Started)
                    {
                        action.Start();
                    }

                    if (action.Tick())
                    {
                        actionFinished = true;
                        this.actions.RemoveAt(i);
                    }
                }
            }
            yield return null;
        }

        this.isExecutingActions = false;
    }

//    GameAction SpawnGameAction(Tile target)
//    {
//        var moveActionData = this.focusedAction as MoveGameActionData;
//        if (moveActionData != null)
//        {
//            return new MoveGameAction(moveActionData, this.focusedCharacter, this.focusedRange.From, target);
//        }
//
//        var rechargeActionData = this.focusedAction as RechargeGameActionData;
//        if (rechargeActionData != null)
//        {
//            return new RechargeGameAction(rechargeActionData, this.focusedCharacter, target);
//        }
//
//        var attackActionData = this.focusedAction as AttackGameActionData;
//        if (attackActionData != null)
//        {
//            return new AttackGameAction(attackActionData, this.focusedCharacter, target);
//        }
//
//        var defendActionData = this.focusedAction as DefendGameActionData;
//        if (defendActionData != null)
//        {
//            return new DefendGameAction(defendActionData, this.focusedCharacter, target);
//        }
//        return null;
//    }
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

    public virtual void Start()
    {
        this.Started = true;

        this.Character.Stamina -= this.ActionData.StaminaCost;
    }

    public virtual bool Tick()
    {
        this.TimeLeft--;
        if (this.TimeLeft == 0)
        {
            this.Character.ActionExecuted = null;
            return true;
        }
        return false;
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

    public override void Start()
    {
        base.Start();
        var difference = this.Target.transform.position - this.From.transform.position;
        this.movePerTick = difference / (float)this.TimeLeft;
    }

    public override bool Tick()
    {
        this.Character.transform.position += movePerTick;

        bool arrived = base.Tick();
        if (arrived)
        {
            this.Character.transform.SetParent(this.Target.transform);
            this.Character.transform.localPosition = Vector3.zero;
        }
        return arrived;
    }
}

public class RechargeGameAction : GameAction
{
    public RechargeGameAction(GameActionData actionData, Character character, TileBehavior target)
        : base(actionData, character, target)
    {
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