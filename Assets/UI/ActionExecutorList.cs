using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ActionExecutorList : MonoBehaviour 
{
    //Set through Unity
    public ActionExecutor ActionExecutor;
    public GameObject GameActionDisplayPrefab;

    public Transform Queue;

    public GameObject MonsterPanelInfo;
    public Text MonsterName;
    public Text MonsterDescription;
    public StatInt MonsterHealthStat;
    public StatInt MonsterStaminaStat;
    //

	public void AddAction(GameAction gameAction)
    {
        var displayGo = Instantiate(this.GameActionDisplayPrefab);
        var display = displayGo.GetComponent<GameActionDisplay>();
        display.GameAction = gameAction;
//        display.Finish += this.OnGameActionFinished;
        display.Cancel += this.OnGameActionCanceled;
        display.MouseHoverIn += this.OnMouseHoverIn;
        display.MouseHoverOut += this.OnMouseHoverOut;

        displayGo.transform.SetParent(this.Queue);
        displayGo.transform.localScale = Vector3.one;

        this.SortGameActionDisplays();
    }

    public void RemoveAction(GameAction gameAction)
    {
        foreach (Transform item in this.Queue)
        {
            item.GetComponent<GameActionDisplay>();
        }
        this.SortGameActionDisplays();
    }

//    void OnGameActionFinished(GameActionDisplay display)
//    {
//        this.RemoveDisplay(display);
//    }

    void OnGameActionCanceled(GameActionDisplay display)
    {
        this.ActionExecutor.CancelAction(display.GameAction);
        this.RemoveDisplay(display);
    }

    void RemoveDisplay(GameActionDisplay display)
    {
//        display.Finish -= this.OnGameActionFinished;
        display.Cancel -= this.OnGameActionCanceled;

        Destroy(display.gameObject);
    }

    void SortGameActionDisplays()
    {
        var displays = new List<GameActionDisplay> (this.Queue.childCount);
        foreach (Transform item in this.Queue)
        {
            displays.Add(item.GetComponent<GameActionDisplay>());
        }

        this.Queue.DetachChildren();

        displays.Sort(new GameActionDisplay.Comparer());

        displays.Reverse();

        foreach (var item in displays)
        {
            item.transform.SetParent(this.Queue);
        }
    }

    void OnMouseHoverIn(GameActionDisplay display)
    {
        var gameAction = display.GameAction;

        gameAction.Display(true);

        if (gameAction.Character is Player)
        {   
            var attack = gameAction.GetComponent<Attack>();
            if (attack != null && attack.TargetHit != null)
            {
                DisplayMonsterInfo( attack.TargetHit as Monster );
            }
        }
        else
        {
            DisplayMonsterInfo(gameAction.Character as Monster);
        }
    }

    void OnMouseHoverOut(GameActionDisplay display)
    {
        var gameAction = display.GameAction;
        gameAction.Display(false);

        this.MonsterPanelInfo.SetActive(false);
    }

    void DisplayMonsterInfo(Monster monster)
    {
        this.MonsterPanelInfo.SetActive(true);
        this.MonsterName.text = monster.Name;
        this.MonsterName.color = monster.GetComponent<LightTarget>().Color;
        this.MonsterDescription.text = monster.Description;
        this.MonsterHealthStat.Value = monster.Health;
        this.MonsterStaminaStat.Value = monster.Stamina;
    }
}
