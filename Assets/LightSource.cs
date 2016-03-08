using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSource : MonoBehaviour 
{
    public int Range = 10;

    public float Flicker = 0.02f;

    private Point lightPos;
    private TileBehavior tileBhv;

    public List<TileBehavior> TilesInSight = new List<TileBehavior>();

    public void UpdateLighting()
    {
        foreach (var tile in this.TilesInSight)
        {
            tile.LightLevel = 0;
        }

        var thisTileBhv = this.GetComponent<TileBehavior>();
        if (thisTileBhv == null)
        {
            thisTileBhv = this.transform.parent.GetComponent<TileBehavior>();
        }
        this.tileBhv = thisTileBhv;
        this.lightPos = thisTileBhv.Pos;

        this.TilesInSight = LevelMng.Instance.TilesAroundInSight(lightPos, this.Range);

        this.StopAllCoroutines();
        StartCoroutine(this.LightSurroundings());
    }

    IEnumerator LightSurroundings()
    {
        var wait = new WaitForSeconds(0.1f);

        while (true)
        {
            this.tileBhv.LightLevel = 1f;

            foreach (TileBehavior tileBhv in this.TilesInSight)
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

