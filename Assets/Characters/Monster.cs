using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : Character 
{
    //Set through Unity
    public int DetectionRange; //in what range monster detects 
    //

    Dictionary<string, GameActionData> actions;

    void Start()
    {
        ActionExecutor.Instance.ActionExecutionCompleted += this.OnActionExecCompleted;

        this.actions = new Dictionary<string, GameActionData>();
        foreach (var gameActionData in this.AvailableActions)
        {
            this.actions.Add(gameActionData.Name, gameActionData);
        }
    }

    void OnDestroy()
    {
        ActionExecutor.Instance.ActionExecutionCompleted -= this.OnActionExecCompleted;
    }

    public void DecideAndQueueAction()
    {
        Point playerPos = LevelMng.Instance.GetPlayerPos();
        Point selfPos = LevelMng.Instance.GetCharacterPos(this);
        if (this.Stamina < 4)
        {
            ActionExecutor.Instance.EnqueueAction(this, this.actions["Bark"], LevelMng.Instance.GetTileBehavior(playerPos));
        }

        Point diff = playerPos - selfPos;
        if (diff.Length <= this.DetectionRange)
        {
            //attack player
            if (diff.Length == 1)
            {
                ActionExecutor.Instance.EnqueueAction(this, this.actions["Bite"], LevelMng.Instance.GetTileBehavior(playerPos));
            }
            else
            {
                Point[] path = LevelMng.Instance.PathFromAtoB(selfPos, playerPos);
                Debug.LogFormat("Dog trying to walk to player from {0} to {1}", selfPos, playerPos);
                Point toGoTo = path[0];
                ActionExecutor.Instance.EnqueueAction(this, this.actions["Walk"], LevelMng.Instance.GetTileBehavior(toGoTo));
            }
        }
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
