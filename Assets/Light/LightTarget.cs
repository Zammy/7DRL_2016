using UnityEngine;
using System.Collections;

public class LightTarget : MonoBehaviour 
{
    //Set through Unity
    public Color Color;
    public SpriteRenderer Sprite;
    public bool ShouldFlicker;
    //

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

    public float LightLevel;

    void Start()
    {
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
            v = Mathf.Lerp(0, 1, this.LightLevel);
            
            sprite.color = ColorManipulator.ColorFromHSV(h, s, v);

            yield return wait;
        }
    }
}
