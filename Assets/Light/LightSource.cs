using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSource : MonoBehaviour 
{
    public int Range = 10;

    public float Flicker = 0.02f;

    private Point lightPos;
    private LightTarget thisLightTarget;
    private List<LightTarget> prevLitTargets = new List<LightTarget>();

    void Start()
    {
    }

    public void UpdateLighting(List<LightTarget> tilesInLight)
    {
        foreach (var item in this.prevLitTargets)
        {
            item.LightLevel = 0;
        }

        this.thisLightTarget = this.GetComponent<LightTarget>();
        if (this.thisLightTarget == null)
        {
            this.thisLightTarget = this.transform.parent.GetComponent<LightTarget>();
        }

        this.lightPos = this.thisLightTarget.Pos;

        this.StopAllCoroutines();
        StartCoroutine(this.LightSurroundings(tilesInLight));

        this.prevLitTargets = tilesInLight;
    }

    IEnumerator LightSurroundings(List<LightTarget> lightTargets)
    {
        var wait = new WaitForSeconds(0.1f);

        while (true)
        {
            this.thisLightTarget.LightLevel = 1f;

            foreach (LightTarget lightTarget in lightTargets)
            {
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

