﻿using UnityEngine;
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
                LevelMng.Instance.Player.IsIntractable = true;
            }

            this.InputManager.ClearHighlightedTiles();

            if (value)
            {
                this.DisableTooExpensiveActions();
            }
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
            ActionDataOption actOpt;
            if (actionData is MoveGameActionData)
                actOpt = InstantiateActionOption(this.MoveActionList.transform);
            else if (actionData is AttackGameActionData)
                actOpt = InstantiateActionOption(this.AttackActionList.transform);
            else if (actionData is RechargeGameActionData)
                actOpt = InstantiateActionOption(this.RechargeActionList.transform);
            else if (actionData is DefendGameActionData)
                actOpt = InstantiateActionOption(this.DefendActionList.transform);
            else
                throw new UnityException("This should not happen!");

            actOpt.GameActionData = actionData;
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

        LevelMng.Instance.Player.IsIntractable = false;
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

    void DisableTooExpensiveActions()
    {
        int stamina = LevelMng.Instance.Player.Stamina;
        System.Action<Transform> checkStamina = (Transform trans) =>
        {
            foreach (Transform child in trans) 
            {
                ActionDataOption actionOpt = child.GetComponent<ActionDataOption>();
                actionOpt.Disabled = actionOpt.GameActionData.StaminaCost > stamina;
            }
        };

        checkStamina(this.MoveActionList.transform);
        checkStamina(this.AttackActionList.transform);
        checkStamina(this.RechargeActionList.transform);
        checkStamina(this.DefendActionList.transform);
    }
}
