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
   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.PlayClicked();
        }
    }

	public void AddAction(GameAction gameAction)
    {
        var displayGo = Instantiate(this.GameActionDisplayPrefab);
        var display = displayGo.GetComponent<GameActionDisplay>();
        display.GameAction = gameAction;
        display.Finish += this.OnGameActionFinished;
        display.Cancel += this.OnGameActionCanceled;

        displayGo.transform.SetParent(this.Queue);
    }

    public void PlayClicked()
    {
        this.ActionExecutor.Play();
    }

    void OnGameActionFinished(GameActionDisplay display)
    {
        this.RemoveDisplay(display);
    }

    void OnGameActionCanceled(GameActionDisplay display)
    {
        this.ActionExecutor.CancelAction(display.GameAction);
        this.RemoveDisplay(display);
    }

    void RemoveDisplay(GameActionDisplay display)
    {
        display.Finish -= this.OnGameActionFinished;
        display.Cancel -= this.OnGameActionCanceled;

        Destroy(display.gameObject);
    }
}
