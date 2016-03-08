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

    private Character charOnTile;
    public Character Character
    {
        get
        {
            return this.charOnTile;
        }
        set
        {
            this.charOnTile = value;
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

    public float LightLevel = 1f;

    void Start()
    {
        StartCoroutine( this.DoLight() );
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

    IEnumerator DoLight()
    {
        SpriteRenderer sprite = null;
        Color color = new Color(255, 55, 0);
        if (this.Tile.Type == TileType.Wall)
        {
            sprite = this.Background.GetComponent<SpriteRenderer>();
        }
        else if (this.Tile.Type == TileType.Ground)
        {
            sprite = this.Static.GetComponent<SpriteRenderer>();
            color = Color.white;
        }
        else if (this.Tile.Type == TileType.End)
        {
            sprite = this.Static.GetComponent<SpriteRenderer>();
            ColorUtility.TryParseHtmlString("CAA300FF", out color);
        }

        float h;
        float s;
        float v;
        ColorManipulator.ColorToHSV(color, out h, out s, out v);

        var wait = new WaitForSeconds(0.25f);
        while (true)
        {
            if (!this.IsHighlighted)
            {
                v = Mathf.Lerp(0, 1, this.LightLevel);
                
                sprite.color = ColorManipulator.ColorFromHSV(h, s, v);
            }

            yield return wait;
        }
    }
}
