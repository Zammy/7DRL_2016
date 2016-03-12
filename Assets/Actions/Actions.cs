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
                this.components.Add(new Move(this, moveCmp, this.Character.GetTileBhv(), targets[0]));
                continue;
            }

            var rechCmp = component as RechargeComponent;
            if (rechCmp != null)
            {
                this.components.Add(new Recharge(this, rechCmp));
                continue;
            }

            var attackCmp = component as AttackComponent;
            if (attackCmp != null)
            {
                this.components.Add( new Attack(this, attackCmp, targets));
                continue;
            }

            var defendComp = component as DefendComponent;
            if (defendComp != null)
            {
                this.components.Add( new Defend(this, defendComp));
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
        this.Character.ActionExecuted = this;

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

    public readonly bool Displaces;

    Vector3 movePerTick;

    public Move (GameAction owner, MoveComponent moveComp, TileBehavior from, TileBehavior to)
        : base(owner)
    {
        this.Displaces = moveComp.Displace;

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

    public Recharge(GameAction owner, RechargeComponent rechargeComp) 
        : base(owner) 
    {
        this.recharge = rechargeComp.Recharge;
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
            if (opp == null)
            {
                float chance;
                ActionExecutor.Instance.CharacterCloseTo(targetTile, out opp, out chance);
                if (opp != null)
                {
                    float randomValue = UnityEngine.Random.value;

//                    chance = chance - 0.25f;

                    chance = ChanceReductionFromDefend(opp, chance);

                    Debug.Log("### Chance to hit " + chance);
    
                    if( randomValue > chance)
                    {
                        opp = null;
                    }
                }
            }

            if ( opp != null)
            {
                damage = DamageReductionFromDefend(opp, damage);
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

    static int DamageReductionFromDefend(Character opp, int damage)
    {
        if (opp.ActionExecuted != null)
        {
            var defend = opp.ActionExecuted.GetComponent<Defend>();
            if (defend != null)
            {
                var prevDmg = damage;
                damage = Mathf.Max(0, damage - defend.FlatDamageReduction);
                if (prevDmg != damage)
                {
                    Debug.LogFormat("### Damage reduction by {0}", prevDmg - damage);
                }
            }
        }
        return damage;
    }

    static float ChanceReductionFromDefend(Character opp, float chance)
    {
        var defend = opp.ActionExecuted.GetComponent<Defend>();
        if (defend != null)
        {
            return defend.ChanceReduction(chance);
        }
        return chance;
    }
}

public class Defend : GameActionComponent
{
    public readonly int  FlatDamageReduction;
    public readonly int  ChanceToHitReduction;

    TileBehavior defendTile;

    public Defend(GameAction owner, DefendComponent defendCmp)
        : base(owner)
    {
        this.FlatDamageReduction = defendCmp.FlatDamageReduction;
        this.ChanceToHitReduction = defendCmp.ChanceToHitReduction;
    }

    public override void Start()
    {
        this.defendTile = this.owner.Character.GetTileBhv();
    }

    public override void Tick(bool finished)
    {
    }

    public override void Display(bool display)
    {
        defendTile.IsHighlighted = display;
    }

    public float ChanceReduction(float chance)
    {
        if (chance < 0f || ChanceToHitReduction == 0)
            return chance;

        return chance * ((float)ChanceToHitReduction/100f);
    }
}