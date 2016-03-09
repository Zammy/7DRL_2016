using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Player : Character, IPointerClickHandler 
{
    //Set through Unity
    public LightSource Torch;
    //

    List<TileBehavior> tilesInSight = new List<TileBehavior>();
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

    public bool IsIntractable
    {
        get
        {
            return this.GetComponent<BoxCollider2D>().enabled;
        }
        set
        {
            this.GetComponent<BoxCollider2D>().enabled = value;
        }
    }

    void Start()
    {
        this.Torch = this.GetComponent<LightSource>();

        this.UpdateLightAndSight();

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
        this.tilesInSight = LevelMng.Instance.TilesAroundInSight(LevelMng.Instance.GetPlayerPos(), this.Torch.Range);

        var lightTargets = new List<LightTarget>();
        lightTargets.Add(this.GetComponent<LightTarget>());
        lightTargets.Add(this.transform.parent.GetComponent<LightTarget>());
        foreach (var tile in this.tilesInSight)
        {
            lightTargets.AddRange(tile.GetComponentsInChildren<LightTarget>());
        }
        this.Torch.UpdateLighting( lightTargets );

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
