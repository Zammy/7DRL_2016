using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionExecutorList : MonoBehaviour 
{
    //Set through Unity
    public ActionExecutor ActionExecutor;
    public GameObject GameActionDisplayPrefab;

    public Transform Queue;
    //

	public void AddAction(GameAction gameAction)
    {
        var displayGo = Instantiate(this.GameActionDisplayPrefab);
        var display = displayGo.GetComponent<GameActionDisplay>();
        display.GameAction = gameAction;
//        display.Finish += this.OnGameActionFinished;
        display.Cancel += this.OnGameActionCanceled;

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

//        Debug.Log("============Start===================");
        foreach (var item in displays)
        {
//            Debug.LogFormat("{0} TimeLeft {1}", item.GameAction.Character.Name, item.GameAction.TimeLeft);
            item.transform.SetParent(this.Queue);
        }
//        Debug.Log("=============End===================");
    }
}
