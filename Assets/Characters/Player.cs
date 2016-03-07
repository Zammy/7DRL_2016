using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Player : Character, IPointerClickHandler 
{
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

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        this.ActionMenu.IsVisible = true;
    }

    #endregion
}
