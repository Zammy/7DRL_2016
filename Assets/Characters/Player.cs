using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Player : Character, IPointerClickHandler 
{
    //Set through Unity
    public LightSource Torch;
    //

    List<Monster> monstersInSight = new List<Monster>();

    RadialActionMenu actionMenu;
    public RadialActionMenu ActionMenu
    {
        get
        {
            return this.actionMenu;
        }
        set
        {
            this.actionMenu = value;
            this.actionMenu.LoadActions(this.AvailableActions);
        }
    }

    public MoveGameActionData DefaultMoveAction
    {
        get;
        private set;
    }

    void Start()
    {
        this.Torch = this.GetComponent<LightSource>();

        foreach (var actionData in this.AvailableActions)
        {
            if (actionData is MoveGameActionData)
            {
                if (actionData.Name == "Walk")
                {
                    this.DefaultMoveAction = actionData as MoveGameActionData;
                    break;
                }
            }
        }
    }

    public override void LocationChanged()
    {
        this.UpdateLightAndSight();
    }

    public bool HasEnemiesInSight()
    {
        return this.monstersInSight.Count > 0;
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        this.ActionMenu.IsVisible = true;
    }

    #endregion

    public void UpdateLightAndSight()
    {
        this.Torch.UpdateLighting();
        List<TileBehavior> tilesInSight = this.Torch.TilesInSight;
        this.monstersInSight.Clear();
        foreach (var tile in tilesInSight)
        {
            if (tile.Character && tile.Character is Monster)
            {
                Debug.Log("Adding monster to sight " + tile.Character.Name);
                this.monstersInSight.Add(tile.Character as Monster);
            }
        }
    }
}
