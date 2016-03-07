using UnityEngine;

public enum AttackPattern
{
    One,
    ThreeLine,
    TwoStright,
}

[CreateAssetMenuAttribute(fileName="Action", menuName="AttackAction")]
public class AttackGameActionData : GameActionData
{
    public int Damage;
}