using UnityEngine;
using System.Collections;

public class MoveToPlayer : AIBehavior 
{
    //Set through Unity
    public GameActionData Attack; //attack type that needs to get in range of
    //

    AttackComponent attackComponent;

    protected override void Start()
    {
        base.Start();

        this.attackComponent = this.Attack.GetComponent<AttackComponent>();
    }

    public override bool ShouldActivate()
    {
        //if out of range
        return !this.IsInRangeOf(attackComponent, this.Attack.Range);
    }

    public override bool ShouldDeactivate()
    {
        return this.IsInRangeOf(attackComponent, this.Attack.Range);
    }

    public override void DecideAndQueueAction()
    {
        Point[] path = LevelMng.Instance.PathFromAtoB(Self.Pos, LevelMng.Instance.Player.Pos);
        Point toGoTo = path[0];
        Debug.Log("Move to " + path[0]);
        ActionExecutor.Instance.EnqueueAction(Self, this.Action, LevelMng.Instance.GetTileBehavior(toGoTo));
    }
}
