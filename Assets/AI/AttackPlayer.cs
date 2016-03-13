using UnityEngine;
using System.Collections;

public class AttackPlayer : AIBehavior
{
    AttackComponent attackComponent;

    protected override void Start()
    {
        base.Start();

        this.attackComponent = this.Action.GetComponent<AttackComponent>();
    }

    public override bool ShouldActivate()
    {
        return this.IsInRangeOf(attackComponent, this.Action.Range);
    }

    public override bool ShouldDeactivate()
    {
        return this.Action.StaminaCost > Self.Stamina;
    }

    public override void DecideAndQueueAction()
    {
        var playerPos = LevelMng.Instance.Player.Pos;
        var target = LevelMng.Instance.GetTileBehavior( playerPos );
        var from = LevelMng.Instance.GetTileBehavior (Self.Pos);

        ActionExecutor.Instance.EnqueueAction(Self, this.Action, GetTargetsForAttack(from, target, attackComponent.Pattern ));
    }

    TileBehavior[] GetTargetsForAttack(TileBehavior from, TileBehavior target, AttackPattern attackPattern)
    {
        switch (attackPattern)
        {
            case AttackPattern.One:
            {
                return new TileBehavior[]{ target };
            }
            case AttackPattern.TwoInARow:
            {
                var targets = new TileBehavior[2];
                targets[0] = target;
                Point pos = target.Pos;
                if (target.Pos.X == from.Pos.X)
                {
                    if (target.Pos.X > from.Pos.X)
                    {
                        pos.X--;
                    }
                    else
                    {
                        pos.X++;
                    }
                }
                else
                {
                    if (target.Pos.Y > from.Pos.Y)
                    {
                        pos.Y--;
                    }
                    else
                    {
                        pos.Y++;
                    }
                }
                targets[1] = LevelMng.Instance.GetTileBehavior( pos );
                return targets;
            }
            default:
                break;
        }

        return null;
    }
}
