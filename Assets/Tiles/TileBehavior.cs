using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class TileBehavior : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Set through Unity
    public GameObject Background;
    public GameObject Static;

    public Color HighlightColor;
    public LightTarget LightTarget;
    //

    public Action<TileBehavior> Clicked;
    public Action<TileBehavior> HoverIn;
    public Action<TileBehavior> HoverOut;

    public Tile Tile
    {
        get;
        set;
    }

    public Point Pos
    {
        get;
        set;
    }

    private Character character;
    public Character Character
    {
        get
        {
            return this.character;
        }
        set
        {
            this.character = value;
            if (value != null)
            {
                value.transform.SetParent(this.transform);
                value.transform.localPosition = Vector3.zero;
                this.Static.SetActive(false);

                this.Character.LocationChanged();
            }
            else
            {
                this.Static.SetActive(true);
            }
        }
    }

    public bool IsHighlighted
    {
        get
        {
            return this.Background.GetComponent<SpriteRenderer>().color == this.HighlightColor;
        }
        set
        {
            this.LightTarget.enabled = !value;
            var sprite = this.Background.GetComponent<SpriteRenderer>();
            if (value)
            {
                sprite.color = this.HighlightColor;
            }
            else
            {
                sprite.color = Color.black;
            }
        }
    }

    void Start()
    {
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        this.Clicked(this);
    }

    #endregion

    #region IPointerEnterHandler implementation

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.HoverIn(this);
    }

    #endregion

    #region IPointerExitHandler implementation

    public void OnPointerExit(PointerEventData eventData)
    {
        this.HoverOut(this);
    }

    #endregion

}
