using UnityEngine;
using System.Collections;

public abstract class AIBehavior : MonoBehaviour 
{
    public GameActionData Action;
    public Monster Self;

    protected virtual void Start()
    {
    }

    public abstract bool ShouldActivate();

    public abstract bool ShouldDeactivate();

    public abstract void DecideAndQueueAction();


    protected bool IsInRangeOf(AttackComponent attackComp, int range)
    {
        Point playerPos = LevelMng.Instance.Player.Pos;
        if (attackComp.Pattern == AttackPattern.One)
        {
            return (Self.Pos - playerPos).Length == range;
        }
        else if (attackComp.Pattern == AttackPattern.TwoInARow)
        {
            return (Self.Pos - playerPos).Length == 2
                && ((Self.Pos.X == playerPos.X) || (Self.Pos.Y == playerPos.Y));
        }

        return false;
    }

//    public abstract void Activated();
//    public abstract void Deactivated();
}
