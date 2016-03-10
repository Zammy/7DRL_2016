using UnityEngine;
using System.Collections;

public enum StatusEffect
{
    Wounded, //slow down by 50%
    Hidden, //hide, opponents cannot detect 
}

[CreateAssetMenuAttribute(fileName="ActionComponent", menuName="Component/AttackStatus")]
public class AttackStatusComponent : ActionComponent 
{
    public StatusEffect StatusEffect;
    public int Duration;
}
