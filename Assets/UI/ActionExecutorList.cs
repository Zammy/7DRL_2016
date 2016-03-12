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
    public MonsterInfo MonsterInfo;
   
    public Text GameTime;
    public Text GameSpeed;
    //

    void Update()
    {
        this.GameTime.text = GameActionDataExt.GetLengthInSecs( ActionExecutor.Instance.GameTime ).ToString("F");
        this.GameSpeed.text = ActionExecutor.Instance.TicksPerFrame.ToString();
    }

	public void AddAction(GameAction gameAction)
    {
        var displayGo = Instantiate(this.GameActionDisplayPrefab);
        var display = displayGo.GetComponent<GameActionDisplay>();
        display.GameAction = gameAction;
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

    public void IncreaseGameSpeed()
    {
        ActionExecutor.Instance.TicksPerFrame += 5;
    }

    public void DecreaseGameSpeed()
    {
        if (ActionExecutor.Instance.TicksPerFrame <= 5)
            return;

        ActionExecutor.Instance.TicksPerFrame -= 5;
    }

    void OnGameActionCanceled(GameActionDisplay display)
    {
        this.ActionExecutor.CancelAction(display.GameAction);
        this.RemoveDisplay(display);
    }

    void RemoveDisplay(GameActionDisplay display)
    {
        display.MouseHoverIn -= this.OnMouseHoverIn;
        display.MouseHoverOut -= this.OnMouseHoverOut;
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
            if (attack != null 
                && attack.TargetsHit.Count == 1 
                && attack.TargetsHit[0] != null)
            {
                this.MonsterInfo.ShowMonsterInfo( attack.TargetsHit[0] as Monster );
            }
        }
        else if (gameAction.Character != null)
        {
            this.MonsterInfo.ShowMonsterInfo(gameAction.Character as Monster);
        }
    }

    void OnMouseHoverOut(GameActionDisplay display)
    {
        var gameAction = display.GameAction;
        gameAction.Display(false);

        this.MonsterInfo.Hide();
    }
}
