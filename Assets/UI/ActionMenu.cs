using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionMenu : MonoBehaviour 
{
    //Set through Unity
    public GameObject Menu;
    public Overlay Overlay;
    public ActionDisplay SelectedAction;
    public InputManager InputManager;

    public GameObject ItemAndDescPrefab;
    public GameObject ActionSelectorPrefab;
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

    public void LoadBuild(ItemData[] items)
    {
        foreach (var item in items)
        {
            var nameAndDescGo = (GameObject)Instantiate(this.ItemAndDescPrefab);
            nameAndDescGo.transform.SetParent( this.Menu.transform );
            nameAndDescGo.transform.localScale = Vector3.one;
            var nameAndDesc = nameAndDescGo.GetComponent<ItemNameAndDesc>();
            nameAndDesc.Name.text = item.Name;
            nameAndDesc.Description.text = item.Description;

            foreach (var action in item.Actions) 
            {
                var actionSelector = this.InstantiateActionOption(this.Menu.transform);
                actionSelector.GameActionData = action;
            }
        }
    }

    void OnOverlayClicked()
    {
        this.IsVisible = false;
    }

    void OnActionClicked(ActionSelector actionOpt)
    {
        this.SelectedAction.GameActionData = actionOpt.GameActionData;
        this.SelectedAction.gameObject.SetActive(true);
        this.Overlay.IsSwallowingClicks = false;
        this.Menu.SetActive(false);

        actionOpt.IsHighlighted = false;

        this.InputManager.SetSelectedAction(actionOpt.GameActionData);

        LevelMng.Instance.Player.IsIntractable = false;
    }

    ActionSelector InstantiateActionOption(Transform parent)
    {
        var go = (GameObject) Instantiate(this.ActionSelectorPrefab);
        go.transform.SetParent(parent);
        go.transform.localScale = Vector3.one;
        var ao = go.GetComponent<ActionSelector>();
        ao.Clicked += this.OnActionClicked;
        return ao;
    }

    void DisableTooExpensiveActions()
    {
        int stamina = LevelMng.Instance.Player.Stamina;

        foreach (Transform child in this.Menu.transform) 
        {
            ActionSelector actionOpt = child.GetComponent<ActionSelector>();
            if (actionOpt != null)
            {
                actionOpt.Disabled = actionOpt.GameActionData.StaminaCost > stamina;
            }
        }
    }
}
