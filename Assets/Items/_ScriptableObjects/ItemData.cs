using UnityEngine;
using System.Collections;

[CreateAssetMenuAttribute(fileName="Item", menuName="Item")]
public class ItemData : ScriptableObject 
{
    public string Name;
    public string Description;

    public GameActionData[] Actions;
}
