using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour 
{
    //Set through Unity
    public string Name;

    public int StartHealth;
    public int StartStamina;
    //

    int health;
    public virtual int Health
    {
        get
        {
            return this.health;
        }
        set
        {
            this.health = value;
        }
    }

    int stamina;
    public virtual int Stamina
    {
        get
        {
            return this.stamina;
        }
        set
        {
            this.stamina = value;
        }
    }

    GameAction actionExecuted = null;
    public GameAction ActionExecuted 
    {
        get
        {
            return this.actionExecuted;
        }
        set
        {
            this.actionExecuted = value;
        }
    }

    public Point Pos
    {
        get
        {
            if (this.ActionExecuted != null)
            {
                var move = this.ActionExecuted.GetComponent<Move>();
                if (move != null)
                {
                    return move.To.Pos;
                }
            }
            return this.GetTileBhv().Pos;
        }
    }

    protected virtual void Start()
    {
        this.Health = this.StartHealth;
        this.Stamina = this.StartStamina;

//        Debug.Log("======= Character Actions =======");
//        foreach (GameActionData action in this.AvailableActions)
//        {
//            string s = string.Format("Name=[{0}] Length=[{1}] StaminaCost=[{2}] ", action.Name, action.Length, action.StaminaCost);
//            if (action is AttackGameActionData)
//            {
//                var attackAction = action as AttackGameActionData;
//                s += string.Format("Damage=[{0}] ", attackAction.Damage);
//            }
//            s += string.Format("Description = {0} ", action.Description );
//            Debug.Log(s);
//        }
//        Debug.Log("==========================");
    }

    public TileBehavior GetTileBhv()
    {
        return this.transform.parent.GetComponent<TileBehavior>();
    }

    public virtual void LocationChanged() {}
}