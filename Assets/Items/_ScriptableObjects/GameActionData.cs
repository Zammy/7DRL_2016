using UnityEngine;

[CreateAssetMenuAttribute(fileName="GameAction", menuName="GameAction")]
public class GameActionData : ScriptableObject
{
    public string Name;
    public string Description;

    public int Length;
    public int StaminaCost;
    public int Range;

    public ActionComponent[] Components;

    public T GetComponent<T>() where T : ActionComponent
    {
        foreach (var component in Components)
        {
            if (component.GetType() == typeof(T))
            {
                return component as T;
            }
        }

        return null;
    }
}

public static class GameActionDataExt
{
    public static float GetLengthInSecs(int length)
    {
        return ((float)length / 1000f);
    }
}