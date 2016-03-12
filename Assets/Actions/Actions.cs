using UnityEngine;
using System.Collections.Generic;
using System;

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
    public readonly GameAction Owner;

    public GameActionComponent(GameAction owner)
    {
        this.Owner = owner;
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
        this.movePerTick = difference / (float)this.Owner.TimeLeft;

        this.From.Character = null;
    }

    public override void Tick(bool arrived)
    {
        this.Owner.Character.transform.position += movePerTick;

        if (!arrived)
            return;

        if (Displaces && this.To.Character)
        {
            var displacedChar = this.To.Character;

            Point moveDir = (this.To.Pos - this.From.Pos);

            Point[] options = new Point[4];
            options[0] = this.To.Pos + moveDir;
            if (this.To.Pos.Y == this.From.Pos.Y)
            {
                options[1] = new Point(this.To.Pos.X, this.To.Pos.Y + 1);
                options[2] = new Point(this.To.Pos.X, this.To.Pos.Y - 1);
            }
            else
            {
                options[1] = new Point(this.To.Pos.X + 1, this.To.Pos.Y);
                options[2] = new Point(this.To.Pos.X - 1, this.To.Pos.Y);
            }
            options[3] = this.From.Pos;

            foreach (var option in options)
            {
                var tile = LevelMng.Instance.GetTileBehavior(option);
                if (tile != null && ActionExecutor.Instance.IsTileAvailableForMove(tile))
                {
                    DisplaceCharacterTo(displacedChar, tile);
                    break;
                }
            }
        }

        this.To.Character = this.Owner.Character;
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

    void DisplaceCharacterTo(Character character, TileBehavior tile)
    {
        Point displacement = character.Pos - tile.Pos;

        tile.Character = character;

        if (character.ActionExecuted == null)
            return;

        var attack = character.ActionExecuted.GetComponent<Attack>();
        if (attack != null)
        {
            List<TileBehavior> newTargets = new List<TileBehavior>();
            foreach (var target in attack.Targets)
            {
                var newTarget = LevelMng.Instance.GetTileBehavior( target.Pos + displacement );
                newTargets.Add(newTarget);
            }
            attack.Targets = newTargets.ToArray();
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
        Character character = Owner.Character;

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
    public TileBehavior[] Targets;

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
        this.attackedFrom = Owner.Character.GetTileBhv();
    }

    public override void Tick(bool done)
    {
        if (!done)
        {
            return;
        }

        this.attackedFrom = Owner.Character.GetTileBhv();

        List<Character> hit = new List<Character>();

        Action<Character> tryAdd = (Character c) =>
        {
            if(!hit.Contains(c))
                hit.Add(c);
        };

        foreach (var targetTile in Targets)
        {
            targetTile.FlashAttack();

            Character character = targetTile.Character;
            if (character != null
                && character != this.Owner.Character)
            {
                tryAdd(character);
            }
        }

        List<Character> possibleTargets = ActionExecutor.Instance.AllCharactersMovingToOrFromTiles(this.Targets);
        possibleTargets.Remove( this.Owner.Character ); //lets not hit ourselves

        foreach (var character in possibleTargets)
        {
            var move = character.ActionExecuted.GetComponent<Move>();
            if (Array.IndexOf(Targets, move.To) != -1
                && Array.IndexOf(Targets, move.From) != -1)
            {
                Debug.Log("### Hit because movement between attacked tiles!");
                tryAdd(character);
                continue;
            }

            if (move.To == attackedFrom
                && Owner.ActionData.Range == 1) //melee attack
            {
                Debug.Log("### Hit because movement into attacked from tile and melee attack.");
                tryAdd(character);
                continue;
            }

            float chance;
            if (Array.IndexOf(Targets, move.From) != -1)
            {
                chance = (float)move.Owner.TimeLeft / (float)move.Owner.ActionData.Length;
            }
            else if (Array.IndexOf(Targets, move.To) != -1)
            {
                chance = 1f -((float)move.Owner.TimeLeft / (float)move.Owner.ActionData.Length);
            }
            else
            {
                throw new UnityException("This should not happen");
            }

            float randomValue = UnityEngine.Random.value;

            chance = ChanceReductionFromDefend(character, chance);

            Debug.Log("### Chance to hit " + chance);
    
            if( chance > randomValue)
            {
                tryAdd(character);
            }
        }

        foreach (var victim in hit)
        {
            HitCharacter(victim);
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

    void HitCharacter(Character character)
    {
        int damage = this.Damage;
        damage = DamageReductionFromDefend(character, damage);
        character.Health -= damage;
        this.TargetsHit.Add(character);
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
        this.defendTile = this.Owner.Character.GetTileBhv();
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