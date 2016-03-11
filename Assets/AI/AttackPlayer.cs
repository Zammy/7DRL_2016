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
        var tile = LevelMng.Instance.GetTileBehavior( playerPos );
        ActionExecutor.Instance.EnqueueAction(Self, this.Action, tile);
    }
}
