using UnityEngine;
using System.Collections;

[CreateAssetMenuAttribute(fileName="ActionComponent", menuName="Component/Status")]
public class StatusComponent : ActionComponent 
{
    public StatusEffect StatusEffect;
    public int Duration;
}
