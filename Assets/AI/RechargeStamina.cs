using UnityEngine;
using System.Collections;

public class RechargeStamina : AIBehavior 
{
    public int UnderStamina;
    public int Until;

    public override bool ShouldActivate()
    {
        return UnderStamina > Self.Stamina;
    }

    public override bool ShouldDeactivate()
    {
        return Self.Stamina > Until;
    }

    public override void DecideAndQueueAction()
    {
        ActionExecutor.Instance.EnqueueAction(Self, Action, Self.GetTileBhv() );
    }

}
