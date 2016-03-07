using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour 
{
    //Set through Unity
    public GameActionData[] AvailableActions;
    public int StartHealth;
    public int StartStamina;
    //

    int health;
    public int Health
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
    public int Stamina
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

    GameAction actionExecuted;
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

    void Start()
    {
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
        this.Health = this.StartHealth;
        this.Stamina = this.StartStamina;
    }
}