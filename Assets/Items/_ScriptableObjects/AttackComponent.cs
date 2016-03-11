using UnityEngine;

public enum AttackPattern
{
    One,
    ThreeAround,
    TwoInARow,
    ThreeInARow,
    AllAround
}

[CreateAssetMenuAttribute(fileName="ActionComponent", menuName="Component/Attack")]
public class AttackComponent : ActionComponent
{
    public int Damage;
    public AttackPattern Pattern;
}