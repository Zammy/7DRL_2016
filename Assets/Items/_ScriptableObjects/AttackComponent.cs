using UnityEngine;

public enum AttackPattern
{
    One,
    ThreeAround
}

[CreateAssetMenuAttribute(fileName="ActionComponent", menuName="Component/Attack")]
public class AttackComponent : ActionComponent
{
    public int Damage;
    public AttackPattern Pattern;
}