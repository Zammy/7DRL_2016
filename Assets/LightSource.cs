using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSource : MonoBehaviour 
{
    public int Range = 10;

    public float Flicker = 0.02f;

    public void LocationChanged()
    {
        this.StopAllCoroutines();
        StartCoroutine(this.LightSurroundings());
    }

    List<TileBehavior> tilesInSight = new List<TileBehavior>();

    IEnumerator LightSurroundings()
    {
        foreach (var tile in this.tilesInSight)
        {
            tile.LightLevel = 0;
        }

        var wait = new WaitForSeconds(0.1f);
        var thisTileBhv = this.GetComponent<TileBehavior>();
        if (thisTileBhv == null)
        {
            thisTileBhv = this.transform.parent.GetComponent<TileBehavior>();
        }
        Point lightPos = thisTileBhv.Pos;

        this.tilesInSight = LevelMng.Instance.TilesAroundInSight(lightPos, this.Range);

        while (true)
        {
            thisTileBhv.LightLevel = 1f;

            foreach (TileBehavior tileBhv in this.tilesInSight)
            {
                int range = (lightPos - tileBhv.Pos).Length;
                float lightLevel = ( 1f - (float)range / (float)this.Range );
                lightLevel = Random.Range(lightLevel - this.Flicker, lightLevel + this.Flicker);
                tileBhv.LightLevel = Mathf.Max(0f, Mathf.Min(1f, lightLevel ) );
            }

            yield return wait;
        }
    }
}

