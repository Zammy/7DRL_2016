using UnityEngine;

[CreateAssetMenuAttribute(fileName="Action", menuName="Component/Defend")]
public class DefendComponent : ActionComponent
{
    public int FlatDamageReduction;
    public int ChanceToHitReduction;
}