﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class ActionSelector : ActionDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    bool disabled = false;
    public bool Disabled
    {
        get
        {
            return disabled;
        }
        set
        {
            disabled = value;

            if (value)
            {
                this.Background.color = Color.gray;
            }
            else
            {
                this.IsHighlighted = this.IsHighlighted;
            }
        }
    }

    public event Action<ActionSelector> Clicked;

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        if (disabled)
            return;
        this.Clicked(this);
    }

    #endregion

    #region IPointerEnterHandler implementation

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disabled)
            return;
        this.IsHighlighted = true;
    }

    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        if (disabled)
            return;
        this.IsHighlighted = false;
    }

    #endregion
}
