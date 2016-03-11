using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveAwayFromPlayer : AIBehavior 
{

    public int UnderStamina;
    public int ExtraDistance;

    public override bool ShouldActivate()
    {
        var player = LevelMng.Instance.Player;
        int attackRange = player.GetAttackRange();
        int distanceToPlayer = (Self.Pos - player.Pos).Length;

        return UnderStamina > Self.Stamina
            && distanceToPlayer <= attackRange + ExtraDistance
            && Self.Stamina > Action.StaminaCost;
    }

    public override bool ShouldDeactivate()
    {
        return !this.IsInPlayerRange();
    }

    public override void DecideAndQueueAction()
    {
        Point selfPos = Self.Pos;
        var player = LevelMng.Instance.Player;
        int currentDistance = (Self.Pos - player.Pos).Length;

        var options = new List<Point>()
        {
            new Point(selfPos.X, selfPos.Y + 1),
            new Point(selfPos.X, selfPos.Y - 1),
            new Point(selfPos.X + 1, selfPos.Y),
            new Point(selfPos.X - 1, selfPos.Y),
        };

        for (int i = 4 - 1; i >= 0; i--) 
        {
            Point option = options[i];
            if (LevelMng.Instance.GetTileBehavior( option ) == null)
            {
                options.RemoveAt(i);
                continue;
            }
        }

        Point bestOption = Point.Zero;
        int highestDistance = int.MinValue;
        foreach (var point in options)
        {
            int distance = (point - player.Pos).Length;
            if (distance > highestDistance)
            {
                bestOption = point;
                highestDistance = distance;
            }
        }

        ActionExecutor.Instance.EnqueueAction(Self, Action, LevelMng.Instance.GetTileBehavior(bestOption));
    }

    bool IsInPlayerRange()
    {
        var player = LevelMng.Instance.Player;
        int attackRange = player.GetAttackRange();
        int distanceToPlayer = (Self.Pos - player.Pos).Length;

        return distanceToPlayer <= attackRange + ExtraDistance;
    }

}
