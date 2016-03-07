using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadialActionMenu : MonoBehaviour 
{
    //Set through Unity
    public GameObject Menu;
    public Overlay Overlay;
    public GameObject ActionOptPrefab;
    public ActionDataDisplay SelectedAction;
    public InputManager InputManager;

    public GameObject MoveActionList;
    public GameObject AttackActionList;
    public GameObject RechargeActionList;
    public GameObject DefendActionList;
    //

    void Start()
    {
        this.Overlay.OverlayClicked += this.OnOverlayClicked;
    }

    void OnDestroy()
    {
        this.Overlay.OverlayClicked -= this.OnOverlayClicked;
    }

    public bool IsVisible
    {
        get
        {
            return this.Menu.activeSelf;
        }
        set
        {
            this.Menu.SetActive(value);
            this.Overlay.IsSwallowingClicks = value;
            if (!value)
            {
                this.SelectedAction.gameObject.SetActive(false);
            }

            this.InputManager.ClearHighlightedTiles();
        }
    }

    public void LoadActions(GameActionData[] actions)
    {
        this.RemoveAllActionOpts(this.MoveActionList);
        this.RemoveAllActionOpts(this.AttackActionList);
        this.RemoveAllActionOpts(this.RechargeActionList);
        this.RemoveAllActionOpts(this.DefendActionList);

        foreach (var actionData in actions)
        {
            var moveActionData = actionData as MoveGameActionData;
            if (moveActionData != null)
            {
                var actOpt = InstantiateActionOption(this.MoveActionList.transform);
                actOpt.GameActionData = moveActionData;
            }
        }
    }

    void OnOverlayClicked()
    {
        this.IsVisible = false;
    }

    void OnActionClicked(ActionDataOption actionOpt)
    {
        this.SelectedAction.GameActionData = actionOpt.GameActionData;
        this.SelectedAction.gameObject.SetActive(true);
        this.Overlay.IsSwallowingClicks = false;
        this.Menu.SetActive(false);

        actionOpt.IsHighlighted = false;

        this.InputManager.SetSelectedAction(actionOpt.GameActionData);
    }

    ActionDataOption InstantiateActionOption(Transform parent)
    {
        var go = (GameObject) Instantiate(this.ActionOptPrefab);
        go.transform.SetParent(parent);
        var ao = go.GetComponent<ActionDataOption>();
        ao.Clicked += this.OnActionClicked;
        return ao;
    }

    void RemoveAllActionOpts(GameObject go)
    {   
        List<GameObject> toDestroy = new List<GameObject>();
        foreach(Transform childTrans in go.transform)
        {
            toDestroy.Add(childTrans.gameObject);
        }

        foreach (var item in toDestroy)
        {
            item.GetComponent<ActionDataOption>().Clicked -= this.OnActionClicked;
            item.transform.SetParent(null);
            Destroy(item);
        }
    }
        
}
