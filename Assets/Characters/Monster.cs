using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : Character 
{
    //Set through Unity
    public string Description;
    public GameObject AIBehaviors;
    //

    public bool IsActive
    {
        get;
        set;
    }

    AIBehavior activeBehavior;

    protected override void Start()
    {
        base.Start();

        ActionExecutor.Instance.ActionExecutionCompleted += this.OnActionExecCompleted;
    }

    void OnDestroy()
    {
        ActionExecutor.Instance.ActionExecutionCompleted -= this.OnActionExecCompleted;
    }

    public void DecideAndQueueAction()
    {
        if (!this.IsActive)
        {
            return;
        }

        foreach (var beh in AIBehaviors.GetComponents<AIBehavior>())
        {
            if (beh == activeBehavior)
            {
                if (beh.ShouldDeactivate())
                    continue;
                else
                    break;
            }

            if (beh.ShouldActivate())
            {
                Debug.LogFormat("[{0}]  ({1}) activated", this.Name, beh.GetType().Name);
                activeBehavior = beh;
                break;
            }
        }

        Debug.LogFormat("[{0}]  ({1}) deciding", this.Name, activeBehavior.GetType().Name);

        activeBehavior.DecideAndQueueAction();
    }

    void OnActionExecCompleted(GameAction gameAction)
    {
        if (gameAction.Character != this)
        {
            return;
        }

        if (!ActionExecutor.Instance.HasPlayerQueuedAction())
        {
            return;
        }

        this.DecideAndQueueAction();
    }

}
