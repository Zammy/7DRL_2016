using UnityEngine;
using System.Collections.Generic;

public class GameAction
{
    public GameActionData ActionData { get; private set; }
    public Character Character { get; private set; }
    public int TimeLeft { get; private set; }

    public bool Started { get; private set; }

    public int TimeFinished { get; private set; }


    List<GameActionComponent> components = new List<GameActionComponent>();

    public GameAction(GameActionData actionData, Character character, TileBehavior[] targets)
    {
        this.ActionData = actionData;
        this.Character = character;
        this.TimeLeft = actionData.Length;

        foreach (var component in actionData.Components)
        {
            var moveCmp = component as MoveComponent;
            if (moveCmp != null)
            {
                this.components.Add(new Move(this, this.Character.GetTileBhv(), targets[0]));
                continue;
            }

            var rechCmp = component as RechargeComponent;
            if (rechCmp != null)
            {
                this.components.Add(new Recharge(this, rechCmp.Recharge));
                continue;
            }

            var attackCmp = component as AttackComponent;
            if (attackCmp != null)
            {
                this.components.Add( new Attack(this, attackCmp, targets));
                continue;
            }
        }
    }

    public T GetComponent<T>() where T : GameActionComponent
    {
        foreach (var component in components)
        {
            if (component.GetType() == typeof(T))
            {
                return component as T;
            }
        }

        return null;
    }

    public virtual bool Tick()
    {
        if (!this.Started)
        {
            this.Start();
        }

        this.TimeLeft--;

        foreach (var cmp in components)
        {
            cmp.Tick(this.TimeLeft == 0);
        }

        if (this.TimeLeft == 0)
        {
            this.TimeFinished = ActionExecutor.Instance.GameTime;
            this.Character.ActionExecuted = null;
            return true;
        }


        return false;
    }

    public void Display(bool display)
    {
        foreach (var comp in components)
        {
            comp.Display(display);
        }
    }

    protected virtual void Start()
    {
        this.Started = true;

        this.Character.Stamina -= this.ActionData.StaminaCost;

        foreach (var cmp in components)
        {
            cmp.Start();
        }
    }
}

public abstract class GameActionComponent
{
    protected GameAction owner;

    public GameActionComponent(GameAction owner)
    {
        this.owner = owner;
    }

    public abstract void Start();
    public abstract void Tick(bool finished);
    public abstract void Display(bool display);
}

public class Move : GameActionComponent
{
    public TileBehavior From {get; private set;}
    public TileBehavior To {get; private set;}

    Vector3 movePerTick;

    public Move (GameAction owner, TileBehavior from, TileBehavior to)
        : base(owner)
    {
        this.To = to;
        this.From = from;
    }

    public override void Start()
    {
        var difference = this.To.transform.position - this.From.transform.position;
        this.movePerTick = difference / (float)this.owner.TimeLeft;

        this.From.Character = null;
    }

    public override void Tick(bool arrived)
    {
        this.owner.Character.transform.position += movePerTick;

        if (arrived)
        {
            this.To.Character = this.owner.Character;
        }
    }

    public override void Display(bool display)
    {
        if (display)
        {
            this.From.ShowHintMovementTo(this.To);
        }
        else
        {
            this.From.HideHints();
        }
    }
}

public class Recharge : GameActionComponent
{
    public readonly int recharge;

    TileBehavior rechargeTile;

    public Recharge(GameAction owner, int recharge) 
        : base(owner) 
    {
        this.recharge = recharge;
    }

    public override void Start()
    {
        Character character = owner.Character;

        character.Stamina += recharge;

        rechargeTile = character.GetTileBhv();

        if (character.Stamina > character.StartStamina)
        {
            character.Stamina = character.StartStamina;
        }
    }
    public override void Tick(bool arrived) {}

    public override void Display(bool display)
    {
        this.rechargeTile.IsHighlighted = display;
    }
}
    

public class Attack : GameActionComponent
{
    public readonly int Damage;
    public readonly TileBehavior[] Targets;

    public List<Character> TargetsHit {get;set;}

    TileBehavior attackedFrom;

    public Attack (GameAction owner, AttackComponent attackCmp, TileBehavior[] targets)
        : base (owner)
    {
        this.Damage = attackCmp.Damage;
        this.Targets = targets;

        this.TargetsHit = new List<Character>();
    }

    public override void Start()
    {
        this.attackedFrom = owner.Character.GetTileBhv();
    }

    public override void Tick(bool done)
    {
        if (!done)
        {
            return;
        }

        foreach (var targetTile in Targets)
        {
            targetTile.FlashAttack();

            int damage = this.Damage;
            Character opp = targetTile.Character;
            if (opp != null)
            {
                //TODO implement defend
//                if (opp.ActionExecuted != null && opp.ActionExecuted is DefendGameAction)
//                {
//                    damage = ((DefendGameAction)opp.ActionExecuted).DefendAgainst(this);
//                }
            }
            else
            {
                float howClose;
                ActionExecutor.Instance.CharacterCloseTo(targetTile, out opp, out howClose);
                float randomValue = UnityEngine.Random.value;

//                Debug.LogFormat("Tries to hit {0} < {1} ", randomValue, howClose - 0.5f);
                if (opp != null
                    && randomValue > (howClose - 0.5f))
                {
                    opp = null;
                }
            }

            if ( opp != null)
            {
                opp.Health -= damage;
                this.TargetsHit.Add(opp);
            }
        }
    }

    public override void Display(bool display)
    {
        if (display)
        {
            attackedFrom.ShowHintAttackTo(Targets);
        }
        else
        {
            attackedFrom.HideHints();
        }
    }
}

//public class DefendGameAction : GameAction
//{
//    int flatDmgReduction;
//
//    public DefendGameAction(GameActionData actionData, Character character, TileBehavior target)
//        : base (actionData, character, target)
//    {
//        var defendData = actionData as DefendGameActionData;
//        this.flatDmgReduction = defendData.FlatDamageReduction;
//    }
//
//    public int DefendAgainst(AttackGameAction attackAction)
//    {
//        return Mathf.Max(attackAction.Damage - this.flatDmgReduction, 0);
//    }
//}