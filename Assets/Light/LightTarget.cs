using UnityEngine;
using System.Collections;

public class LightTarget : MonoBehaviour 
{
    //Set through Unity
    public Color Color;
    public SpriteRenderer Sprite;
    public bool ShouldFlicker;

    public bool ShouldBeLit { get; set; }
    //

    public float LightLevel;

    public Point Pos
    {
        get
        {
            var thisTileBhv = this.GetComponent<TileBehavior>();
            if (thisTileBhv == null)
            {
                thisTileBhv = this.transform.parent.GetComponent<TileBehavior>();
            }
            return thisTileBhv.Pos;
        }
    }


    void Start()
    {
        this.ShouldBeLit = true;
        this.LightLevel = 0f;
        StartCoroutine ( this.DoLight(this.Sprite, this.Color) );
    }

    IEnumerator DoLight(SpriteRenderer sprite, Color color)
    {
        float h;
        float s;
        float v;
        ColorManipulator.ColorToHSV(color, out h, out s, out v);

        var wait = new WaitForSeconds(0.25f);
        while (true)
        {
            if (!this.ShouldBeLit)
            {
                yield return null;
                continue;
            }

            v = Mathf.Lerp(0, 1, this.LightLevel);
            
            sprite.color = ColorManipulator.ColorFromHSV(h, s, v);

            yield return wait;
        }
    }
}
