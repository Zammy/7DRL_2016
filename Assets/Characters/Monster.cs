using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : Character 
{
    //Set through Unity
    public int DetectionRange; //in what range monster detects 
    public string Description;
    public GameObject AIBehaviors;

    //

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
//        if (activeBehavior == null || activeBehavior.ShouldDeactivate())
//        {
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
//        }

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
