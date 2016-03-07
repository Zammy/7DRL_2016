using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class ActionDataOption : ActionDataDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Set through Unity
    public Image Background;

    public Color HighlightColor;
    //

    public bool IsHighlighted
    {
        get
        {
            return this.Background.color == this.HighlightColor;
        }
        set
        {
            if (value)
            {
                this.Background.color = this.HighlightColor;
            }
            else
            {
                this.Background.color = Color.black;
            }
        }
    }

    public event Action<ActionDataOption> Clicked;

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        this.Clicked(this);
    }

    #endregion

    #region IPointerEnterHandler implementation

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.IsHighlighted = true;
    }

    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        this.IsHighlighted = false;
    }

    #endregion
}
