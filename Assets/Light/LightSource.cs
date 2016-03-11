using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSource : MonoBehaviour 
{
    public int Range = 10;

    public float Flicker = 0.02f;

    private Point lightPos;
    private List<LightTarget> prevLitTargets = new List<LightTarget>();

    public void UpdateLighting(List<LightTarget> tilesInLight)
    {
        this.lightPos = this.transform.parent.GetComponent<TileBehavior>().Pos;

        foreach (var item in this.prevLitTargets)
        {
            item.LightLevel = 0;
        }

        this.StopAllCoroutines();
        StartCoroutine(this.LightSurroundings(tilesInLight));

        this.prevLitTargets = tilesInLight;
    }

    IEnumerator LightSurroundings(List<LightTarget> lightTargets)
    {
        var wait = new WaitForSeconds(0.1f);

        while (true)
        {
            foreach (LightTarget lightTarget in lightTargets)
            {
                if (lightTarget == null)
                    continue;

                int range = (lightPos - lightTarget.Pos).Length;
                float lightLevel = ( 1f - (float)range / (float)this.Range );
                if (lightTarget.ShouldFlicker)
                {
                    lightLevel = Random.Range(lightLevel - this.Flicker, lightLevel + this.Flicker);
                }
                lightTarget.LightLevel = Mathf.Max(0f, Mathf.Min(1f, lightLevel ) );
            }

            yield return wait;
        }
    }
}

