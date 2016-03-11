using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class TileBehavior : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Set through Unity
    public GameObject Background;
    public GameObject Static;

    public Color HighlightColor;
    public LightTarget LightTarget;

    public GameObject ArrowPrefab;
    //

    public Action<TileBehavior> Clicked;
    public Action<TileBehavior> HoverIn;
    public Action<TileBehavior> HoverOut;

    private List<GameObject> arrows = new List<GameObject>();

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

    bool isHighlighted = false;
    public bool IsHighlighted
    {
        get
        {
            return isHighlighted;
        }
        set
        {
            if (this.Tile.Type != TileType.Ground)
                return;

            isHighlighted = value;

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

    public bool IsPatternHighlighted
    {
        set
        {
            this.LightTarget.enabled = !value;
            var sprite = this.Background.GetComponent<SpriteRenderer>();
            if (value)
            {
                sprite.color = Color.red;
            }
            else
            {
                this.IsHighlighted = this.IsHighlighted;
            }
        }
    }

    public void ShowHintMovementTo(TileBehavior to)
    {
        var arrowGo = SpawnArrowTo( to );
        var color = this.HighlightColor;
        color.a = 0.75f;
        arrowGo.GetComponent<SpriteRenderer>().color = color;
    }

    public void ShowHintAttackTo(TileBehavior[] targets)
    {
        foreach (var target in targets)
        {
            var arrowGo = SpawnArrowTo( target );
            var color = Color.red;
            color.a = 0.75f;
            arrowGo.GetComponent<SpriteRenderer>().color = color;
        }
    }

    public void HideHints()
    {
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
    }

    GameObject SpawnArrowTo(TileBehavior tile)
    {
        Quaternion rotation = Quaternion.identity;
        Point diff = tile.Pos - this.Pos;
        if (diff.X == 1 && diff.Y == 0)
        {
            rotation = Quaternion.AngleAxis(-90, Vector3.forward);
        }
        else if (diff.X == 0 && diff.Y == -1)
        {
            rotation = Quaternion.AngleAxis(-180, Vector3.forward);
        }
        else if (diff.X == -1 && diff.Y == 0)
        {
            rotation = Quaternion.AngleAxis(-270, Vector3.forward);
        }
        var arrowGo = (GameObject)Instantiate(this.ArrowPrefab, this.transform.position, rotation);
        arrowGo.transform.SetParent(this.transform);
        arrowGo.transform.localPosition += arrowGo.transform.up * 0.5f;

        this.arrows.Add(arrowGo);

        return arrowGo;
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
