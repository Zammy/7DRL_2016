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

    public bool TempImpassable
    {
        get
        {
            return this.Tile.TempImpassable;
        }
        set
        {
            this.Tile.TempImpassable = value;

            StartCoroutine ( this.Rotate90 (this.Static.transform, !value) );
        }
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

                if (this.Tile.Type == TileType.End
                    && value is Player
                    && LevelMng.Instance.IsRoomClear(this.Tile.Room))
                {
                    Debug.Log("LEVEL COMPLETEDED!!!!!");
                }
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

    public void FlashAttack()
    {
        StartCoroutine( this.Flash(Color.red, Color.black ) );
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

    IEnumerator Flash(Color flashColor, Color prevColor)
    {
        const int frames = 12;

        this.GetComponent<LightTarget>().ShouldBeLit = false;

        flashColor.a = 0f;
        var sprite = this.Background.GetComponent<SpriteRenderer>();
        for (int i = 0; i < frames; i++)
        {
            flashColor.a += 1/(float)frames;
            sprite.color = flashColor;
            yield return null;
        }

        for (int i = 0; i < frames; i++)
        {
            flashColor.a -= 1/(float)frames;
            sprite.color = flashColor;
            yield return null;
        }

        this.GetComponent<LightTarget>().ShouldBeLit = true;

        sprite.color = prevColor;
    }

    IEnumerator Rotate90(Transform transform, bool open)
    {
        yield return new WaitForSeconds(1f);

        const int frames = 30;
        float change = 90/frames;
        float angle = -90f;
        if (!open)
        {
            angle = 0;
            change = -change;
        }

        var quat = transform.rotation;

        for (int i = 0; i < frames; i++)
        {
            angle += change;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return null;
        }
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
