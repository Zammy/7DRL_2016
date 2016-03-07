using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class Overlay : MonoBehaviour, IPointerClickHandler 
{
    public event Action OverlayClicked;

    public bool IsSwallowingClicks
    {
        get
        {
            return this.GetComponent<Image>().enabled;
        }
        set
        {
            this.GetComponent<Image>().enabled = value;
        }
            
    }

    void Start()
    {
        this.IsSwallowingClicks = false;
    }
    
    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this.OverlayClicked != null)
        {
            this.OverlayClicked();
        }
    }

    #endregion

}
